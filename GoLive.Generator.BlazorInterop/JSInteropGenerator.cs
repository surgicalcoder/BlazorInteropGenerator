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

            // #2: Report config load errors via diagnostics
            if (!string.IsNullOrEmpty(config.ConfigError))
            {
                spc.ReportDiagnostic(Diagnostic.Create(ConfigError, Location.None, config.ConfigPath ?? "BlazorInterop.json", config.ConfigError));
                return;
            }

            foreach (var settingsFile in config.Files)
            {
                // #9: Config validation
                if (string.IsNullOrWhiteSpace(settingsFile.SourceContents))
                {
                    spc.ReportDiagnostic(Diagnostic.Create(ConfigError, Location.None, settingsFile.Source ?? "BlazorInterop.json", "Source file is empty or does not exist"));
                    continue;
                }
                if (string.IsNullOrWhiteSpace(settingsFile.ObjectToInterop))
                {
                    spc.ReportDiagnostic(Diagnostic.Create(ConfigError, Location.None, settingsFile.Source ?? "BlazorInterop.json", "ObjectToInterop is required"));
                    continue;
                }

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
                    if (info.CallbackParams != null && info.CallbackParams.Contains(param.Name))
                    {
                        param.Type = "Action";
                    }
                    else if (info.ParamTypes.TryGetValue(param.Name, out var jsType))
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

        // Match by last segment, but only if unambiguous
        var lastSegment = displayName.Contains('.') ? displayName.Substring(displayName.LastIndexOf('.') + 1) : displayName;
        var matches = keys.Where(k =>
        {
            var kLast = k.Contains('.') ? k.Substring(k.LastIndexOf('.') + 1) : k;
            return kLast == lastSegment;
        }).ToList();

        // Return only if exactly one match (unambiguous)
        return matches.Count == 1 ? matches[0] : null;
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
        // #7: Group items by their parent path for nested class generation
        var rootItems = items.Where(i => !i.DisplayName.Contains('.')).ToList();
        var nestedGroups = items.Where(i => i.DisplayName.Contains('.'))
            .GroupBy(i => i.DisplayName.Substring(0, i.DisplayName.LastIndexOf('.')))
            .ToDictionary(g => g.Key, g => g.ToList());

        // Emit root-level items
        foreach (var item in rootItems)
        {
            ssb.AppendLine($"public static string _{item.Name.Replace(".", "_")} => \"{item.Name}\";");
            EmitItemMethods(ssb, item, invokeString, invokeVoidString);
        }

        // Emit nested classes
        foreach (var group in nestedGroups.OrderBy(g => g.Key))
        {
            var className = group.Key.Replace(".", "_");
            ssb.AppendLine($"public static class {className}");
            ssb.AppendOpenCurlyBracketLine();

            foreach (var item in group.Value)
            {
                var shortName = item.DisplayName.Substring(item.DisplayName.LastIndexOf('.') + 1);
                var shortItem = new JavascriptItem
                {
                    Name = item.Name,
                    DisplayName = shortName,
                    ReturnType = item.ReturnType,
                    Params = item.Params
                };
                ssb.AppendLine($"public static string _{item.Name.Replace(".", "_")} => \"{item.Name}\";");
                EmitItemMethods(ssb, shortItem, invokeString, invokeVoidString);
            }

            ssb.AppendCloseCurlyBracketLine();
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
            return new Settings { Files = new List<SettingsFile>(), InvokeString = "", InvokeVoidString = "", ConfigError = ex.Message, ConfigPath = configPath };
        }
    }
}
