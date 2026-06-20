using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Microsoft.CodeAnalysis;

namespace GoLive.Generator.BlazorInterop;

[Generator]
public class JSInteropGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor JsParseError = new(
        "BI001", "JS Parse Error",
        "Failed to parse JavaScript file '{0}': {1}",
        "BlazorInterop", DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ConfigError = new(
        "BI002", "Config Error",
        "Failed to load BlazorInterop config '{0}': {1}",
        "BlazorInterop", DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InferenceWarning = new(
        "BI003", "Type Inference Warning",
        "Could not infer types for function '{0}', defaulting to object",
        "BlazorInterop", DiagnosticSeverity.Warning, isEnabledByDefault: true);

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

                var source = GenerateSource(spc, config.InvokeString, config.InvokeVoidString, settingsFile);

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

    private string GenerateSource(SourceProductionContext spc, string invokeString, string invokeVoidString, SettingsFile file)
    {
        var ssb = new SourceStringBuilder();

        // Phase 1: Run Jint to discover runtime object shape
        var items = DiscoverJsFunctions(file, spc);
        if (items == null || items.Count == 0)
            return string.Empty;

        // Phase 2: Run Esprima to infer types from AST + JSDoc
        ApplyTypeInference(file, items, spc);

        // Phase 3: Generate C# source
        EmitUsings(ssb, file);
        EmitClass(ssb, file, items, invokeString, invokeVoidString);

        return ssb.ToString();
    }

    private List<JavascriptItem> DiscoverJsFunctions(SettingsFile file, SourceProductionContext spc)
    {
        try
        {
            var engine = new Engine(cfg => cfg
                .TimeoutInterval(TimeSpan.FromSeconds(5))
                .LimitMemory(4_000_000)
                .LimitRecursion(50));

            foreach (var s in file.Init) engine.Execute(s);
            engine.Execute(file.SourceContents);

            var rootValue = engine.Evaluate(file.ObjectToInterop);
            var items = new List<JavascriptItem>();

            WalkJsObject(rootValue, file.ObjectToInterop, "", items);

            return items;
        }
        catch (Exception ex)
        {
            spc.ReportDiagnostic(Diagnostic.Create(JsParseError, Location.None, file.Source, ex.Message));
            return null;
        }
    }

    private void ApplyTypeInference(SettingsFile file, List<JavascriptItem> items, SourceProductionContext spc)
    {
        Dictionary<string, InferredTypeInfo> inferredTypes;
        try
        {
            inferredTypes = JsTypeInference.InferTypes(file.SourceContents);
        }
        catch (Exception ex)
        {
            spc.ReportDiagnostic(Diagnostic.Create(JsParseError, Location.None, file.Source, $"Esprima parse error: {ex.Message}"));
            inferredTypes = new Dictionary<string, InferredTypeInfo>();
        }

        foreach (var item in items)
        {
            // Try matching by DisplayName, then by the last segment of DisplayName
            var matchKey = FindBestMatch(item.DisplayName, inferredTypes.Keys);

            if (matchKey != null && inferredTypes.TryGetValue(matchKey, out var info))
            {
                // Apply param types
                foreach (var param in item.Params)
                {
                    if (info.ParamTypes.TryGetValue(param.Name, out var jsType))
                    {
                        param.Type = MapJsTypeToCsType(jsType);
                    }
                }

                // Apply return type
                if (!string.IsNullOrEmpty(info.ReturnType))
                {
                    item.ReturnType = MapJsTypeToCsType(info.ReturnType);
                }
            }
            else
            {
                spc.ReportDiagnostic(Diagnostic.Create(InferenceWarning, Location.None, item.DisplayName));
            }
        }
    }

    private static string FindBestMatch(string displayName, IEnumerable<string> keys)
    {
        // Exact match
        if (keys.Contains(displayName))
            return displayName;

        // Match by last segment (for nested objects like "blazorInterop.showModal" matching "showModal")
        var lastSegment = displayName.Contains('.') ? displayName.Substring(displayName.LastIndexOf('.') + 1) : displayName;
        var match = keys.FirstOrDefault(k =>
        {
            var kLast = k.Contains('.') ? k.Substring(k.LastIndexOf('.') + 1) : k;
            return kLast == lastSegment;
        });

        return match;
    }

    private void EmitUsings(SourceStringBuilder ssb, SettingsFile file)
    {
        ssb.AppendLine("using System;");
        ssb.AppendLine("using System.Threading;");
        ssb.AppendLine("using System.Threading.Tasks;");
        ssb.AppendLine("using Microsoft.JSInterop;");
        ssb.AppendLine($"namespace {file.Namespace}");
        ssb.AppendOpenCurlyBracketLine();
        ssb.AppendLine($"public static class {file.ClassName}");
        ssb.AppendOpenCurlyBracketLine();
    }

    private void EmitClass(SourceStringBuilder ssb, SettingsFile file, List<JavascriptItem> items, string invokeString, string invokeVoidString)
    {
        foreach (var item in items)
        {
            ssb.AppendLine($"public static string _{item.Name.Replace(".", "_")} => \"{item.Name}\";");
            EmitItemMethods(ssb, item, invokeString, invokeVoidString);
        }

        ssb.AppendCloseCurlyBracketLine();
        ssb.AppendCloseCurlyBracketLine();
    }

    private void EmitItemMethods(SourceStringBuilder ssb, JavascriptItem item, string invokeString, string invokeVoidString)
    {
        var hasParams = item.Params?.Count > 0;
        var argsList = hasParams ? string.Join(",", item.Params.Select(e => $"@{e.Name}")) : "";
        var ctAndArgs = hasParams ? $"cancellationToken, {argsList}" : "cancellationToken";
        var paramsDecl = GetParamsObjectString(item);
        var ctParamDecl = ", CancellationToken cancellationToken = default";
        var isVoidReturn = string.Equals(item.ReturnType, "void", StringComparison.OrdinalIgnoreCase);
        var hasTypedReturn = !string.IsNullOrEmpty(item.ReturnType) && !isVoidReturn;
        var methodName = item.DisplayName.Replace(".", "_");

        // VoidAsync
        ssb.AppendLine($"public static async Task {methodName}VoidAsync (this IJSRuntime JSRuntime {paramsDecl}{ctParamDecl})");
        ssb.AppendOpenCurlyBracketLine();
        ssb.AppendLine(string.Format(invokeVoidString, item.Name, ctAndArgs));
        ssb.AppendCloseCurlyBracketLine();

        // Generic Async<T> (skip if void)
        if (!isVoidReturn)
        {
            ssb.AppendLine($"public static async Task<T> {methodName}Async<T> (this IJSRuntime JSRuntime {paramsDecl}{ctParamDecl})");
            ssb.AppendOpenCurlyBracketLine();
            ssb.AppendLine(string.Format(invokeString, item.Name, ctAndArgs));
            ssb.AppendCloseCurlyBracketLine();
        }

        // Strongly-typed Async (skip if void or no return type)
        if (hasTypedReturn)
        {
            var typedInvokeString = invokeString.Replace("<T>", $"<{item.ReturnType}>");

            ssb.AppendLine($"public static async Task<{item.ReturnType}> {methodName}Async (this IJSRuntime JSRuntime {paramsDecl}{ctParamDecl})");
            ssb.AppendOpenCurlyBracketLine();
            ssb.AppendLine(string.Format(typedInvokeString, item.Name, ctAndArgs));
            ssb.AppendCloseCurlyBracketLine();
        }
    }

    private static string GetParamsObjectString(JavascriptItem item)
    {
        if (item.Params == null || item.Params.Count == 0) return string.Empty;
        return $", {string.Join(",", item.Params.Select(f => $"{f.Type} @{f.Name}"))}";
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

        try
        {
            string configFileDirectory = Path.GetDirectoryName(configPath);
            var config = Utf8Json.JsonSerializer.Deserialize<Settings>(jsonString);
            var defaultNamespace = string.IsNullOrWhiteSpace(rootNamespace) ? "DefaultNamespace" : rootNamespace;

            var resolvedFiles = new List<SettingsFile>();
            foreach (var settingsFile in config.Files)
            {
                var resolved = new SettingsFile
                {
                    Output = settingsFile.Output,
                    ClassName = string.IsNullOrWhiteSpace(settingsFile.ClassName) ? "JSInterop" : settingsFile.ClassName,
                    Source = settingsFile.Source,
                    Namespace = string.IsNullOrWhiteSpace(settingsFile.Namespace) ? defaultNamespace : settingsFile.Namespace,
                    ObjectToInterop = settingsFile.ObjectToInterop,
                    Init = settingsFile.Init,
                };

                var fullPath = Path.Combine(configFileDirectory, settingsFile.Source);
                resolved.Source = Path.GetFullPath(fullPath);

                if (!string.IsNullOrWhiteSpace(settingsFile.Output))
                    resolved.Output = Path.GetFullPath(Path.Combine(configFileDirectory, settingsFile.Output));

                if (File.Exists(resolved.Source))
                    resolved.SourceContents = File.ReadAllText(resolved.Source);

                resolvedFiles.Add(resolved);
            }

            config.Files = resolvedFiles;
            return config;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}
