using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Microsoft.CodeAnalysis;
using Utf8Json;

namespace GoLive.Generator.BlazorInterop;

[Generator]
public class JSInteropGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }
    
    private string SanitizeFileName(string fileName) => Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c, '_'));

    public void Execute(GeneratorExecutionContext context)
    {
        var config = LoadConfig(context);

        if (config == null)
        {
            return;
        }

        foreach (var settingsFile in config.Files)
        {
            string source = GenerateSource(config.InvokeString, config.InvokeVoidString, settingsFile);
            
            if (!string.IsNullOrWhiteSpace(source))
            {
                if (string.IsNullOrWhiteSpace(settingsFile.Output))
                {
                    context.AddSource($"Generated.{SanitizeFileName(settingsFile.ClassName)}.cs", source);
                }
                else
                {
                    if (File.Exists(settingsFile.Output))
                    {
                        File.Delete(settingsFile.Output);
                    }

                    File.WriteAllText(settingsFile.Output, source);
                }
            }
        }
    }

    private string GenerateSource(string invokeString, string invokeVoidString, SettingsFile file)
    {
        var ssb = new SourceStringBuilder();

        string source = System.IO.File.ReadAllText(file.Source);
            
        var engine = new Engine();
        foreach (var s in file.Init)
        {
            engine.Execute(s);
        }

        engine.Execute(source);
            
        var ob = engine.Evaluate(file.ObjectToInterop).ToObject() as ExpandoObject;
        List<JavascriptItem> items = new();
            
        VisitExpandoObject(ref items, file.ObjectToInterop,"", ob);
        ssb.AppendLine("using System.Threading.Tasks;");
        ssb.AppendLine("using Microsoft.JSInterop;");
        ssb.AppendLine($"namespace {file.Namespace}");
        ssb.AppendOpenCurlyBracketLine();
        ssb.AppendLine($"public static class {file.ClassName}");
        ssb.AppendOpenCurlyBracketLine();
            
        foreach (var item in items)
        {
            ssb.AppendLine($"public static string _{item.Name.Replace(".","_")} => \"{item.Name}\";");
                
            ssb.AppendLine($"public static async Task {item.DisplayName.Replace(".","_")}VoidAsync (this IJSRuntime JSRuntime {GetParamsObjectString(item)})");
            ssb.AppendOpenCurlyBracketLine();
            ssb.AppendLine(string.Format(invokeVoidString, item.Name, item.Params?.Count > 0 ? string.Join(",", item.Params.Select(e=>$"@{e}")) : "null" )  );
            ssb.AppendCloseCurlyBracketLine();
                
            ssb.AppendLine($"public static async Task<T> {item.DisplayName.Replace(".","_")}Async<T> (this IJSRuntime JSRuntime {GetParamsObjectString(item)})");
            ssb.AppendOpenCurlyBracketLine();
            ssb.AppendLine(string.Format(invokeString, item.Name, string.Join(",", item.Params?.Count > 0 ? string.Join(",", item.Params.Select(e=>$"@{e}")) : "null") ));
            ssb.AppendCloseCurlyBracketLine();
        }

        ssb.AppendCloseCurlyBracketLine();
        ssb.AppendCloseCurlyBracketLine();
        return ssb.ToString();
    }

    private string GetParamsObjectString(JavascriptItem item)
    {
        if (item.Params == null || item.Params.Count == 0)
        {
            return string.Empty;
        }
            
        return $", {string.Join(",", item.Params.Select(f => $"object @{f}"))}";
    }

    static void VisitExpandoObject(ref List<JavascriptItem> Item, string ParentName, string DisplayName, ExpandoObject obj)
    {
        var item = obj as IDictionary<string, object>;

        foreach (KeyValuePair<string, object> value in item)
        {
            if (value.Value is ExpandoObject expandoValue)
            {
                VisitExpandoObject(ref Item, $"{ParentName}.{value.Key}", $"{DisplayName}.{value.Key}", expandoValue);
                continue;
            }
            else
            {
                var itemType = value.Value.GetType();

                if (itemType.GetGenericTypeDefinition() == typeof(Func<,,>).GetGenericTypeDefinition() && itemType.GenericTypeArguments[0] == typeof(JsValue) && itemType.GenericTypeArguments[1]  == typeof(JsValue[]) && itemType.GenericTypeArguments[2] == typeof(JsValue))
                {
                    var funcItem = (Func<JsValue, JsValue[], JsValue>)value.Value;
                        
                    var itemFound = new JavascriptItem()
                    {
                        Name = $"{ParentName}.{value.Key}",
                        DisplayName = $"{DisplayName}.{value.Key}"
                    };

                    if (itemFound.DisplayName.StartsWith("."))
                    {
                        itemFound.DisplayName = itemFound.DisplayName.Substring(1);
                    }
                            
                    foreach (var functionDeclarationParam in (funcItem.Target as ScriptFunctionInstance).FunctionDeclaration.Params.OfType<Esprima.Ast.Identifier>())
                    {
                        itemFound.Params.Add(functionDeclarationParam.Name);
                    }
                    Item.Add(itemFound);
                }
            }
        }
    }
        
        
    private Settings LoadConfig(GeneratorExecutionContext context)
    {
        var configFilePath = context.AdditionalFiles.FirstOrDefault(e => e.Path.EndsWith("BlazorInterop.json"));
        if (configFilePath == null)
        {
            return null;
        }
        var jsonString = File.ReadAllText(configFilePath.Path);
        var config = JsonSerializer.Deserialize<Settings>(jsonString) ?? new();
        var configFileDirectory = Path.GetDirectoryName(configFilePath.Path);

        string defaultNamespace = "DefaultNamespace";
        
        if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.rootnamespace", out var rootNamespace))
        {
            defaultNamespace = rootNamespace;
        }

        foreach (var settingsFile in config.Files)
        {
            if (string.IsNullOrWhiteSpace(settingsFile.ClassName))
            {
                settingsFile.ClassName = "JSInterop";
            }

            if (string.IsNullOrWhiteSpace(settingsFile.Namespace))
            {
                settingsFile.Namespace = defaultNamespace;
            }
            
            if (string.IsNullOrWhiteSpace(settingsFile.Output))
            {
                settingsFile.Output = "JSInterop.cs";
            }
            
            var fullPath = Path.Combine(configFileDirectory, settingsFile.Source);
            settingsFile.Source = Path.GetFullPath(fullPath);

            if (!string.IsNullOrWhiteSpace(settingsFile.Output))
            {
                settingsFile.Output = Path.GetFullPath(Path.Combine(configFileDirectory, settingsFile.Output));
            }
        }

        return config;
    }
}