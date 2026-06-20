using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Esprima;
using Esprima.Ast;

namespace GoLive.Generator.BlazorInterop;    public class InferredTypeInfo
{
    public Dictionary<string, string> ParamTypes { get; set; } = new Dictionary<string, string>();
    public string ReturnType { get; set; }
    public string Description { get; set; }
    public Dictionary<string, string> ParamDescriptions { get; set; } = new Dictionary<string, string>();
    public string ReturnDescription { get; set; }
    public HashSet<string> CallbackParams { get; set; } = new HashSet<string>();
}

public static class JsTypeInference
{
    public static Dictionary<string, InferredTypeInfo> InferTypes(string source)
    {
        var result = new Dictionary<string, InferredTypeInfo>();

        Program program;
        try
        {
            var options = new ParserOptions { Tolerant = true, Comments = true };
            var parser = new JavaScriptParser(options);
            program = parser.ParseScript(source);
        }
        catch
        {
            return result;
        }

        var comments = program.Comments ?? (IReadOnlyList<SyntaxComment>)Array.Empty<SyntaxComment>();

        // Walk top-level expression statements to find object assignments like window.blazorInterop = { ... }
        foreach (var node in program.Body)
        {
            if (node is ExpressionStatement exprStmt)
            {
                if (exprStmt.Expression is AssignmentExpression assignment)
                {
                    var prefix = GetAssignmentTargetName(assignment.Left);
                    if (assignment.Right is ObjectExpression objExpr)
                    {
                        WalkObjectExpression(objExpr, prefix, comments, result);
                    }
                }
            }
        }

        return result;
    }

    private static string GetAssignmentTargetName(Node node)
    {
        if (node is Identifier id)
            return id.Name;
        if (node is StaticMemberExpression member)
        {
            var obj = GetAssignmentTargetName(member.Object);
            var propName = (member.Property as Identifier)?.Name;
            if (propName == null) return obj;
            return string.IsNullOrEmpty(obj) ? propName : $"{obj}.{propName}";
        }
        return string.Empty;
    }

    private static void WalkObjectExpression(ObjectExpression objExpr, string prefix, IReadOnlyList<SyntaxComment> comments, Dictionary<string, InferredTypeInfo> result)
    {
        foreach (var property in objExpr.Properties)
        {
            if (property is not Property prop)
                continue;

            var propName = GetPropertyKey(prop);
            if (string.IsNullOrEmpty(propName))
                continue;

            var fullName = string.IsNullOrEmpty(prefix) ? propName : $"{prefix}.{propName}";

            // #3: Support arrow functions alongside regular functions
            // #11: Support async functions (both FunctionExpression and ArrowFunctionExpression have Async property)
            if (prop.Value is IFunction func)
            {
                var info = new InferredTypeInfo();

                // Get JSDoc comment for this property
                var jsDoc = FindLeadingJSDoc(prop, comments);

                // Extract param types from JSDoc
                if (!string.IsNullOrEmpty(jsDoc))
                {
                    var (paramTypes, paramDescs, summary) = ExtractParamTypesFromJSDoc(jsDoc);
                    info.ParamTypes = paramTypes;
                    info.ParamDescriptions = paramDescs;
                    info.Description = summary;
                    var (returnType, returnDesc) = ExtractReturnTypeFromJSDoc(jsDoc);
                    info.ReturnType = returnType;
                    info.ReturnDescription = returnDesc;
                }

                // Extract param names from AST
                ExtractParamNames(func.Params, info);

                // Infer return type from ReturnStatement if not set by JSDoc
                if (string.IsNullOrEmpty(info.ReturnType))
                {
                    if (func.Body is BlockStatement block)
                        info.ReturnType = InferReturnTypeFromBody(block);
                    else if (func.Body is Expression bodyExpr)
                        info.ReturnType = InferFromExpression(bodyExpr);
                    else
                        info.ReturnType = "void";
                }

                // #10: Detect callback params - params that are called as functions in the body
                if (func.Body is BlockStatement bodyBlock)
                {
                    DetectCallbackParams(bodyBlock, info);
                }

                result[fullName] = info;
            }
            else if (prop.Value is ObjectExpression nestedObj)
            {
                WalkObjectExpression(nestedObj, fullName, comments, result);
            }
        }
    }

    private static string GetPropertyKey(Property prop)
    {
        if (prop.Key is Identifier id)
            return id.Name;
        if (prop.Key is Literal lit)
            return lit.Value?.ToString();
        return null;
    }

    private static string FindLeadingJSDoc(Node node, IReadOnlyList<SyntaxComment> comments)
    {
        if (node == null)
            return null;

        var nodeStart = node.Location.Start.Line;

        // Find the closest block JSDoc comment that ends near (before) the node
        string closest = null;
        foreach (var comment in comments)
        {
            if (comment.Type == CommentType.Block &&
                comment.Value.StartsWith("*") &&
                comment.Location.End.Line >= nodeStart - 3 &&
                comment.Location.End.Line <= nodeStart)
            {
                closest = comment.Value;
            }
        }

        return closest;
    }

    private static (Dictionary<string, string> types, Dictionary<string, string> descriptions, string summary) ExtractParamTypesFromJSDoc(string jsDoc)
    {
        var types = new Dictionary<string, string>();
        var descriptions = new Dictionary<string, string>();
        var cleaned = CleanJSDocComment(jsDoc);
        var regex = new Regex(@"@param\s+\{([^}]+)\}\s+([a-zA-Z0-9_]+)(?:\s*-\s*(.+))?");
        var matches = regex.Matches(cleaned);
        foreach (Match match in matches)
        {
            if (match.Groups[1].Success && match.Groups[2].Success)
            {
                types[match.Groups[2].Value] = MapJSDocType(match.Groups[1].Value);
                if (match.Groups[3].Success)
                    descriptions[match.Groups[2].Value] = match.Groups[3].Value.Trim();
            }
        }
        var summary = ExtractSummary(jsDoc);
        return (types, descriptions, summary);
    }

    private static string ExtractSummary(string jsDoc)
    {
        if (string.IsNullOrWhiteSpace(jsDoc))
            return null;

        // Strip leading * from each line and clean up
        var cleaned = CleanJSDocComment(jsDoc);

        // Get text before first @ tag
        var regex = new Regex(@"^([\s\S]*?)(?=\s*@\w)");
        var match = regex.Match(cleaned);
        if (match.Success)
        {
            var summary = match.Groups[1].Value.Trim();
            summary = Regex.Replace(summary, @"\s*\r?\n\s*", " ").Trim();
            return string.IsNullOrWhiteSpace(summary) ? null : summary;
        }

        return null;
    }

    private static string CleanJSDocComment(string jsDoc)
    {
        // Remove leading * from each line and trim
        var lines = jsDoc.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
        var cleaned = new System.Text.StringBuilder();
        foreach (var line in lines)
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("* "))
                trimmed = trimmed.Substring(2);
            else if (trimmed == "*")
                trimmed = "";
            cleaned.AppendLine(trimmed);
        }
        return cleaned.ToString().Trim();
    }

    private static (string type, string description) ExtractReturnTypeFromJSDoc(string jsDoc)
    {
        var cleaned = CleanJSDocComment(jsDoc);
        var regex = new Regex(@"@returns?\s+\{([^}]+)\}(?:\s*-?\s*(.+))?");
        var match = regex.Match(cleaned);
        if (match.Success && match.Groups[1].Success)
        {
            var type = MapJSDocType(match.Groups[1].Value);
            var desc = match.Groups[2].Success ? match.Groups[2].Value.Trim() : null;
            return (type, desc);
        }
        return (null, null);
    }

    private static string MapJSDocType(string jsDocType)
    {
        return jsDocType.Trim().ToLowerInvariant() switch
        {
            "string" => "string",
            "number" => "number",
            "boolean" or "bool" => "boolean",
            "date" => "datetime",
            "void" => "void",
            "object" => "object",
            "array" => "object",
            "function" => "object",
            "any" => "object",
            _ => jsDocType.Trim()
        };
    }

    private static void ExtractParamNames(NodeList<Node> funcParams, InferredTypeInfo info)
    {
        foreach (var param in funcParams)
        {
            string paramName = null;
            if (param is Identifier id)
            {
                paramName = id.Name;
            }
            // For destructured params (BindingPattern), skip

            if (!string.IsNullOrEmpty(paramName) && !info.ParamTypes.ContainsKey(paramName))
            {
                info.ParamTypes[paramName] = "object"; // default when not in JSDoc
            }
        }
    }

    private static string InferReturnTypeFromBody(BlockStatement body)
    {
        if (body == null)
            return "void";

        return InferFromStatements(body.Body);
    }

    private static string InferFromStatements(IEnumerable<StatementListItem> statements)
    {
        string inferredType = "void";

        foreach (var stmt in statements)
        {
            if (stmt is ReturnStatement returnStmt)
            {
                var returnType = InferFromExpression(returnStmt.Argument);
                if (returnType != null)
                {
                    // If multiple returns with different types, fall back to object
                    if (inferredType != "void" && inferredType != returnType)
                        return "object";
                    inferredType = returnType;
                }
            }
            else if (stmt is IfStatement ifStmt)
            {
                var consequentType = InferFromStatements(GetStatementsFromBlock(ifStmt.Consequent));
                var alternateType = ifStmt.Alternate != null
                    ? InferFromStatements(GetStatementsFromBlock(ifStmt.Alternate))
                    : null;

                if (consequentType != "void" && consequentType != null)
                {
                    if (inferredType != "void" && inferredType != consequentType)
                        return "object";
                    inferredType = consequentType;
                }
                if (alternateType != "void" && alternateType != null)
                {
                    if (inferredType != "void" && inferredType != alternateType)
                        return "object";
                    inferredType = alternateType;
                }
            }
        }

        return inferredType;
    }

    private static IEnumerable<StatementListItem> GetStatementsFromBlock(StatementListItem stmt)
    {
        if (stmt is BlockStatement block)
            return block.Body;
        return new[] { stmt };
    }

    private static string InferFromExpression(Expression expr)
    {
        if (expr == null)
            return "void";

        switch (expr)
        {
            case Literal lit:
                if (lit.Value is bool)
                    return "boolean";
                if (lit.Value is string)
                    return "string";
                if (lit.Value is double)
                    return "number";
                if (lit.Value == null)
                    return "object";
                return "object";

            case UnaryExpression unary:
                if (unary.Operator == UnaryOperator.LogicalNot)
                    return "boolean";
                return InferFromExpression(unary.Argument);

            case LogicalExpression logical:
                var leftType = InferFromExpression(logical.Left);
                var rightType = InferFromExpression(logical.Right);
                if (leftType == rightType)
                    return leftType;
                return "object";

            case BinaryExpression binary:
                return InferBinaryExpression(binary);

            case ArrayExpression:
                return "object";

            case ObjectExpression:
                return "object";

            case NewExpression newExpr:
                if (newExpr.Callee is Identifier newId)
                {
                    if (newId.Name == "Date")
                        return "datetime";
                }
                return "object";

            case CallExpression callExpr:
                if (callExpr.Callee is Identifier callId)
                {
                    if (callId.Name == "Date")
                        return "datetime";
                }
                // #5: Date.now() detection
                if (callExpr.Callee is StaticMemberExpression callMember)
                {
                    var callMethodName = (callMember.Property as Identifier)?.Name;
                    if (callMember.Object is Identifier objId && objId.Name == "Date" && callMethodName == "now")
                        return "number";
                    if (callMethodName == "toString" || callMethodName == "toFixed" || callMethodName == "substring" || callMethodName == "slice")
                        return "string";
                }
                return "object";

            case ConditionalExpression cond:
                var consequentType = InferFromExpression(cond.Consequent);
                var alternateType = InferFromExpression(cond.Alternate);
                if (consequentType == alternateType)
                    return consequentType;
                return "object";

            case TemplateLiteral:
                return "string";

            default:
                return "object";
        }
    }

    private static string InferBinaryExpression(BinaryExpression binary)
    {
        if (binary.Operator == BinaryOperator.Plus)
        {
            bool leftIsString = IsStringExpression(binary.Left);
            bool rightIsString = IsStringExpression(binary.Right);

            if (leftIsString || rightIsString)
                return "string";

            return "number";
        }

        // Comparison operators return boolean
        if (binary.Operator == BinaryOperator.Equal ||
            binary.Operator == BinaryOperator.NotEqual ||
            binary.Operator == BinaryOperator.StrictlyEqual ||
            binary.Operator == BinaryOperator.StrictlyNotEqual ||
            binary.Operator == BinaryOperator.Greater ||
            binary.Operator == BinaryOperator.GreaterOrEqual ||
            binary.Operator == BinaryOperator.Less ||
            binary.Operator == BinaryOperator.LessOrEqual)
        {
            return "boolean";
        }

        // Numeric operators
        if (binary.Operator == BinaryOperator.Minus ||
            binary.Operator == BinaryOperator.Times ||
            binary.Operator == BinaryOperator.Divide ||
            binary.Operator == BinaryOperator.Modulo)
        {
            return "number";
        }

        return "object";
    }

    private static void DetectCallbackParams(BlockStatement body, InferredTypeInfo info)
    {
        // Collect all param names
        var paramNames = new HashSet<string>(info.ParamTypes.Keys);
        if (paramNames.Count == 0) return;

        // Walk the body looking for CallExpression where callee is an Identifier that matches a param
        DetectCallbackUsage(body.Body, paramNames, info.CallbackParams);
    }

    private static void DetectCallbackUsage(IEnumerable<StatementListItem> statements, HashSet<string> paramNames, HashSet<string> callbackParams)
    {
        foreach (var stmt in statements)
        {
            WalkNodeForCallbacks(stmt, paramNames, callbackParams);
        }
    }

    private static void WalkNodeForCallbacks(Esprima.Ast.Node node, HashSet<string> paramNames, HashSet<string> callbackParams)
    {
        // Stop at nested function boundaries to avoid false positives
        if (node is FunctionExpression || node is ArrowFunctionExpression)
            return;

        if (node is CallExpression callExpr)
        {
            if (callExpr.Callee is Identifier callId && paramNames.Contains(callId.Name))
            {
                callbackParams.Add(callId.Name);
            }
        }

        // Recurse into child nodes
        foreach (var child in node.ChildNodes)
        {
            WalkNodeForCallbacks(child, paramNames, callbackParams);
        }
    }

    private static bool IsStringExpression(Expression expr)
    {
        if (expr is Literal lit && lit.Value is string)
            return true;
        if (expr is TemplateLiteral)
            return true;
        if (expr is CallExpression call && call.Callee is StaticMemberExpression member)
        {
            var methodName = (member.Property as Identifier)?.Name;
            if (methodName == "toString" || methodName == "toFixed" || methodName == "substring" || methodName == "slice" || methodName == "join")
                return true;
        }
        if (expr is ConditionalExpression cond)
        {
            return IsStringExpression(cond.Consequent) || IsStringExpression(cond.Alternate);
        }
        if (expr is BinaryExpression binary && binary.Operator == BinaryOperator.Plus)
        {
            return IsStringExpression(binary.Left) || IsStringExpression(binary.Right);
        }
        if (expr is LogicalExpression logical)
        {
            return IsStringExpression(logical.Left) || IsStringExpression(logical.Right);
        }
        return false;
    }
}
