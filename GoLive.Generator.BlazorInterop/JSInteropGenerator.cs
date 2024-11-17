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

namespace GoLive.Generator.BlazorInterop
{
    [Generator]
    public class JSInteropGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var config = LoadConfig(context);

            if (config == null)
            {
                return;
            }

            string source = GenerateSource(config);

            if (!string.IsNullOrWhiteSpace(source))
            {
                if (string.IsNullOrWhiteSpace(config.OutputToFile))
                {
                    context.AddSource("Generated.cs", source);
                }
                else
                {
                    if (File.Exists(config.OutputToFile))
                    {
                        File.Delete(config.OutputToFile);
                    }

                    File.WriteAllText(config.OutputToFile, source);
                }
            }
        }

        private string GenerateSource(Settings config)
        {
            var ssb = new SourceStringBuilder();

            string source = System.IO.File.ReadAllText(config.JavascriptFile);
            
            var engine = new Engine();
            foreach (var s in config.JSInit)
            {
                engine.Execute(s);
            }

            engine.Execute(source);
            
            var ob = engine.Evaluate(config.MainJsObject).ToObject() as ExpandoObject;
            List<JavascriptItem> items = new();
            
            VisitExpandoObject(ref items, config.MainJsObject,"", ob);
            ssb.AppendLine("using System.Threading.Tasks;");
            ssb.AppendLine("using Microsoft.JSInterop;");
            ssb.AppendLine($"namespace {config.Namespace}");
            ssb.AppendOpenCurlyBracketLine();
            ssb.AppendLine($"public static class {config.ClassName}");
            ssb.AppendOpenCurlyBracketLine();
            
            foreach (var item in items)
            {
                ssb.AppendLine($"public static string _{item.Name.Replace(".","_")} => \"{item.Name}\";");
                
                ssb.AppendLine($"public static async Task {item.DisplayName.Replace(".","_")}VoidAsync (this IJSRuntime JSRuntime {GetParamsObjectString(item)})");
                ssb.AppendOpenCurlyBracketLine();
                ssb.AppendLine(string.Format(config.InvokeVoidString, item.Name, item.Params?.Count > 0 ? string.Join(",", item.Params.Select(e=>$"@{e}")) : "null" )  );
                ssb.AppendCloseCurlyBracketLine();
                
                ssb.AppendLine($"public static async Task<T> {item.DisplayName.Replace(".","_")}Async<T> (this IJSRuntime JSRuntime {GetParamsObjectString(item)})");
                ssb.AppendOpenCurlyBracketLine();
                ssb.AppendLine(string.Format(config.InvokeString, item.Name, string.Join(",", item.Params?.Count > 0 ? string.Join(",", item.Params.Select(e=>$"@{e}")) : "null") ));
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
                            
                        foreach (var functionDeclarationParam in (funcItem.Target as ScriptFunction).FunctionDeclaration.Params.OfType<Esprima.Ast.Identifier>())
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

            if (string.IsNullOrWhiteSpace(config.Namespace))
            {
                if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.rootnamespace", out var rootNamespace))
                {
                    config.Namespace = rootNamespace;
                }
            }

            if (string.IsNullOrWhiteSpace(config.ClassName))
            {
                config.ClassName = "JSInterop";
            }

            if (string.IsNullOrWhiteSpace(config.OutputToFile))
            {
                config.OutputToFile = "JSInterop.cs";
            }
            
            
            var fullPath = Path.Combine(configFileDirectory, config.JavascriptFile);
            config.JavascriptFile = Path.GetFullPath(fullPath);

            if (!string.IsNullOrWhiteSpace(config.OutputToFile))
            {
                config.OutputToFile = Path.GetFullPath(Path.Combine(configFileDirectory, config.OutputToFile));
            }


            return config;
        }
    }
}