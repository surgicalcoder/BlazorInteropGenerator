using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Microsoft.CodeAnalysis;

namespace GoLive.Generator.BlazorInterop;

[Generator]
public class JSInteropGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var rootNamespace = context.AnalyzerConfigOptionsProvider
            .Select((e, _) => e.GlobalOptions.TryGetValue("build_property.rootnamespace", out var ns) ? ns : "DefaultNamespace");

        var configProvider = context.AdditionalTextsProvider.Where(IsConfigurationFile)
            .Combine(rootNamespace)
            .Select((textAndNamespace, cancellationToken) => LoadConfig(textAndNamespace.Left.GetText(cancellationToken)?.ToString(), textAndNamespace.Left.Path, textAndNamespace.Right ?? "DefaultNamespace"));

        context.RegisterSourceOutput(configProvider, (spc, config) =>
        {
            if (config == null) return;

            foreach (var settingsFile in config.Files)
            {
                if (string.IsNullOrWhiteSpace(settingsFile.SourceContents)) continue;

                var source = GenerateSource(config.InvokeString, config.InvokeVoidString, settingsFile);

                if (string.IsNullOrWhiteSpace(source)) continue;

                if (string.IsNullOrWhiteSpace(settingsFile.Output))
                {
                    spc.AddSource($"Generated.{SanitizeFileName(settingsFile.ClassName)}.cs", source);
                }
                else
                {
                    if (File.Exists(settingsFile.Output)) File.Delete(settingsFile.Output);
                    File.WriteAllText(settingsFile.Output, source);
                }
            }
        });
    }

    private static bool IsConfigurationFile(AdditionalText text) => text.Path.EndsWith("BlazorInterop.json");

    private string SanitizeFileName(string fileName)
        => Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c, '_'));

    private string GenerateSource(string invokeString, string invokeVoidString, SettingsFile file)
    {
        var ssb = new SourceStringBuilder();
        var engine = new Engine();

        foreach (var s in file.Init) engine.Execute(s);
        engine.Execute(file.SourceContents);

        var rootValue = engine.Evaluate(file.ObjectToInterop);
        List<JavascriptItem> items = new();

        WalkJsObject(rootValue, file.ObjectToInterop, "", items);

        if (file.MethodTypes != null)
        {
            foreach (var item in items)
            {
                if (file.MethodTypes.TryGetValue(item.DisplayName, out var paramTypes))
                {
                    foreach (var param in item.Params)
                    {
                        if (paramTypes.TryGetValue(param.Name, out var jsType))
                            param.Type = MapJsTypeToCsType(jsType);
                    }
                }
            }
        }

        ssb.AppendLine("using System;");
        ssb.AppendLine("using System.Threading;");
        ssb.AppendLine("using System.Threading.Tasks;");
        ssb.AppendLine("using Microsoft.JSInterop;");
        ssb.AppendLine($"namespace {file.Namespace}");
        ssb.AppendOpenCurlyBracketLine();
        ssb.AppendLine($"public static class {file.ClassName}");
        ssb.AppendOpenCurlyBracketLine();

        foreach (var item in items)
        {
            ssb.AppendLine($"public static string _{item.Name.Replace(".", "_")} => \"{item.Name}\";");

            var hasParams = item.Params?.Count > 0;
            var paramsStr = hasParams ? string.Join(",", item.Params.Select(e => $"@{e.Name}")) : "null";
            var paramsDecl = GetParamsObjectString(item);

            ssb.AppendLine($"public static async Task {item.DisplayName.Replace(".", "_")}VoidAsync (this IJSRuntime JSRuntime {paramsDecl})");
            ssb.AppendOpenCurlyBracketLine();
            ssb.AppendLine(string.Format(invokeVoidString, item.Name, paramsStr));
            ssb.AppendCloseCurlyBracketLine();

            ssb.AppendLine($"public static async Task {item.DisplayName.Replace(".", "_")}VoidAsync (this IJSRuntime JSRuntime {paramsDecl}{GetCTCParamString(item)})");
            ssb.AppendOpenCurlyBracketLine();
            ssb.AppendLine(string.Format(invokeVoidString, item.Name, hasParams ? $"cancellationToken, {paramsStr}" : "cancellationToken"));
            ssb.AppendCloseCurlyBracketLine();

            ssb.AppendLine($"public static async Task<T> {item.DisplayName.Replace(".", "_")}Async<T> (this IJSRuntime JSRuntime {paramsDecl})");
            ssb.AppendOpenCurlyBracketLine();
            ssb.AppendLine(string.Format(invokeString, item.Name, paramsStr));
            ssb.AppendCloseCurlyBracketLine();

            ssb.AppendLine($"public static async Task<T> {item.DisplayName.Replace(".", "_")}Async<T> (this IJSRuntime JSRuntime {paramsDecl}{GetCTCParamString(item)})");
            ssb.AppendOpenCurlyBracketLine();
            ssb.AppendLine(string.Format(invokeString, item.Name, hasParams ? $"cancellationToken, {paramsStr}" : "cancellationToken"));
            ssb.AppendCloseCurlyBracketLine();
        }

        ssb.AppendCloseCurlyBracketLine();
        ssb.AppendCloseCurlyBracketLine();
        return ssb.ToString();
    }

    private static string GetParamsObjectString(JavascriptItem item)
    {
        if (item.Params == null || item.Params.Count == 0) return string.Empty;
        return $", {string.Join(",", item.Params.Select(f => $"{f.Type} @{f.Name}"))}";
    }

    private static string GetCTCParamString(JavascriptItem item)
    {
        return ", CancellationToken cancellationToken";
    }

    private void WalkJsObject(JsValue value, string parentName, string displayName, List<JavascriptItem> items)
    {
        if (value is not JsObject jsObj) return;

        foreach (var prop in jsObj.GetOwnProperties())
        {
            var propKey = prop.Key.ToString();
            var fullName = $"{parentName}.{propKey}";
            var dispName = $"{displayName}.{propKey}".TrimStart('.');

            if (prop.Value.Value is ScriptFunction scriptFunc)
            {
                var item = new JavascriptItem { Name = fullName, DisplayName = dispName };

                var regex = new Regex(@"function\s*\(([^)]*)\)");
                var match = regex.Match(scriptFunc.ToString() ?? "");
                if (match.Success && match.Groups[1].Success)
                {
                    var names = match.Groups[1].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var n in names)
                    {
                        var clean = n.Trim();
                        if (!string.IsNullOrWhiteSpace(clean))
                            item.Params.Add(new MethodParameter { Name = clean });
                    }
                }

                items.Add(item);
            }
            else if (prop.Value.Value is JsObject nestedObj && !prop.Value.Value.IsCallable())
            {
                WalkJsObject(nestedObj, fullName, dispName, items);
            }
        }
    }

    private static string MapJsTypeToCsType(string jsType)
    {
        return (jsType ?? "").ToLowerInvariant() switch
        {
            "string" => "string",
            "number" or "int" or "integer" => "int",
            "long" => "long",
            "float" => "float",
            "double" => "double",
            "decimal" => "decimal",
            "boolean" or "bool" => "bool",
            "guid" => "Guid",
            "datetime" or "date" => "DateTime",
            "object" or "any" or "" => "object",
            _ => jsType
        };
    }

    private Settings LoadConfig(string jsonString, string configPath, string rootNamespace)
    {
        if (string.IsNullOrWhiteSpace(jsonString)) return null;

        string configFileDirectory = Path.GetDirectoryName(configPath);
        var config = Utf8Json.JsonSerializer.Deserialize<Settings>(jsonString);
        var defaultNamespace = string.IsNullOrWhiteSpace(rootNamespace) ? "DefaultNamespace" : rootNamespace;

        foreach (var settingsFile in config.Files)
        {
            if (string.IsNullOrWhiteSpace(settingsFile.ClassName))
                settingsFile.ClassName = "JSInterop";
            if (string.IsNullOrWhiteSpace(settingsFile.Namespace))
                settingsFile.Namespace = defaultNamespace;

            var fullPath = Path.Combine(configFileDirectory, settingsFile.Source);
            settingsFile.Source = Path.GetFullPath(fullPath);

            if (!string.IsNullOrWhiteSpace(settingsFile.Output))
                settingsFile.Output = Path.GetFullPath(Path.Combine(configFileDirectory, settingsFile.Output));

            if (File.Exists(settingsFile.Source))
                settingsFile.SourceContents = File.ReadAllText(settingsFile.Source);
        }

        return config;
    }
}
