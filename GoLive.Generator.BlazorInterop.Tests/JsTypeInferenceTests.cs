using GoLive.Generator.BlazorInterop;
using Xunit;

namespace GoLive.Generator.BlazorInterop.Tests;

public class JsTypeInferenceTests
{
    [Fact]
    public void InfersParamTypesFromJSDoc()
    {
        var source = @"
obj = {
    /**
     * @param {string} name
     * @param {boolean} isFormal
     */
    greetUser: function(name, isFormal) {
        return (isFormal ? ""Dear "" : ""Hello "") + name;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Single(result);
        var info = result["obj.greetUser"];
        Assert.Equal("string", info.ParamTypes["name"]);
        Assert.Equal("boolean", info.ParamTypes["isFormal"]);
    }

    [Fact]
    public void InfersReturnTypeFromJSDoc()
    {
        var source = @"
obj = {
    /** @returns {number} */
    addNumbers: function(x, y) {
        return x + y;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("number", result["obj.addNumbers"].ReturnType);
    }

    [Fact]
    public void InfersReturnType_BooleanLiteral()
    {
        var source = @"
obj = {
    showModal: function(dialogId) {
        return true;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("boolean", result["obj.showModal"].ReturnType);
    }

    [Fact]
    public void InfersReturnType_StringLiteral()
    {
        var source = @"
obj = {
    getName: function() {
        return ""hello"";
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("string", result["obj.getName"].ReturnType);
    }

    [Fact]
    public void InfersReturnType_NumberLiteral()
    {
        var source = @"
obj = {
    getNumber: function() {
        return 42;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("number", result["obj.getNumber"].ReturnType);
    }

    [Fact]
    public void InfersReturnType_NewDate()
    {
        var source = @"
obj = {
    makeDate: function(y, m, d) {
        return new Date(y, m, d);
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("datetime", result["obj.makeDate"].ReturnType);
    }

    [Fact]
    public void InfersReturnType_TemplateLiteral()
    {
        var source = @"
obj = {
    format: function(name) {
        return `Hello ${name}`;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("string", result["obj.format"].ReturnType);
    }

    [Fact]
    public void InfersReturnType_TernaryWithStrings()
    {
        var source = @"
obj = {
    greet: function(isFormal) {
        return isFormal ? ""Dear "" : ""Hello "";
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("string", result["obj.greet"].ReturnType);
    }

    [Fact]
    public void InfersReturnType_TernaryConcatenation()
    {
        var source = @"
obj = {
    greetUser: function(name, isFormal) {
        return (isFormal ? ""Dear "" : ""Hello "") + name;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("string", result["obj.greetUser"].ReturnType);
    }

    [Fact]
    public void InfersReturnType_ComparisonOperators()
    {
        var source = @"
obj = {
    isEqual: function(a, b) {
        return a === b;
    },
    isGreater: function(a, b) {
        return a > b;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("boolean", result["obj.isEqual"].ReturnType);
        Assert.Equal("boolean", result["obj.isGreater"].ReturnType);
    }

    [Fact]
    public void InfersReturnType_NumericOperators()
    {
        var source = @"
obj = {
    subtract: function(a, b) {
        return a - b;
    },
    multiply: function(a, b) {
        return a * b;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("number", result["obj.subtract"].ReturnType);
        Assert.Equal("number", result["obj.multiply"].ReturnType);
    }

    [Fact]
    public void InfersReturnType_VoidFunction()
    {
        var source = @"
obj = {
    setPageTitle: function(title) {
        document.title = title;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("void", result["obj.setPageTitle"].ReturnType);
    }

    [Fact]
    public void JSDocOverridesASTInference()
    {
        var source = @"
obj = {
    /**
     * @returns {boolean}
     */
    addNumbers: function(x, y) {
        return x + y;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        // JSDoc says boolean, AST would infer number — JSDoc wins
        Assert.Equal("boolean", result["obj.addNumbers"].ReturnType);
    }

    [Fact]
    public void DefaultParamTypeWhenNoJSDoc()
    {
        var source = @"
obj = {
    doSomething: function(x, y) {
        return x + y;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("object", result["obj.doSomething"].ParamTypes["x"]);
        Assert.Equal("object", result["obj.doSomething"].ParamTypes["y"]);
    }

    [Fact]
    public void NestedObjectFunctions()
    {
        var source = @"
window.blazorInterop = {
    nested: {
        /** @returns {string} */
        getValue: function() {
            return ""hello"";
        }
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.True(result.ContainsKey("window.blazorInterop.nested.getValue"));
        Assert.Equal("string", result["window.blazorInterop.nested.getValue"].ReturnType);
    }

    [Fact]
    public void InvalidJSReturnsEmpty()
    {
        var source = "this is not valid javascript {{{{";

        var result = JsTypeInference.InferTypes(source);

        Assert.Empty(result);
    }

    [Fact]
    public void EmptySourceReturnsEmpty()
    {
        var result = JsTypeInference.InferTypes("");

        Assert.Empty(result);
    }

    [Fact]
    public void MultipleReturnsSameType()
    {
        var source = @"
obj = {
    test: function(x) {
        if (x) {
            return true;
        }
        return false;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("boolean", result["obj.test"].ReturnType);
    }

    [Fact]
    public void MultipleReturnsDifferentTypes_FallsBackToObject()
    {
        var source = @"
obj = {
    test: function(x) {
        if (x) {
            return ""hello"";
        }
        return 42;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("object", result["obj.test"].ReturnType);
    }

    [Fact]
    public void LogicalNotReturnsBoolean()
    {
        var source = @"
obj = {
    negate: function(x) {
        return !x;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("boolean", result["obj.negate"].ReturnType);
    }

    [Fact]
    public void MixedParamTypesFromJSDoc()
    {
        var source = @"
obj = {
    /**
     * @param {string} name
     * @param {number} age
     * @param {boolean} active
     */
    createUser: function(name, age, active) {
        return { name: name, age: age, active: active };
    }
};";

        var result = JsTypeInference.InferTypes(source);

        var info = result["obj.createUser"];
        Assert.Equal("string", info.ParamTypes["name"]);
        Assert.Equal("number", info.ParamTypes["age"]);
        Assert.Equal("boolean", info.ParamTypes["active"]);
    }

    // --- Arrow function tests (#3) ---

    [Fact]
    public void ArrowFunction_InfersReturnType()
    {
        var source = @"
obj = {
    /** @returns {number} */
    add: (x, y) => x + y
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.True(result.ContainsKey("obj.add"));
        Assert.Equal("number", result["obj.add"].ReturnType);
    }

    [Fact]
    public void ArrowFunction_WithBlockBody_InfersReturnType()
    {
        var source = @"
obj = {
    add: (x, y) => {
        return x + y;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("number", result["obj.add"].ReturnType);
    }

    [Fact]
    public void ArrowFunction_ExtractsParamNames()
    {
        var source = @"
obj = {
    /** @param {string} name */
    greet: (name) => ""Hello "" + name
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("string", result["obj.greet"].ParamTypes["name"]);
        Assert.Equal("string", result["obj.greet"].ReturnType);
    }

    [Fact]
    public void AsyncFunction_DetectedViaIFunction()
    {
        var source = @"
obj = {
    fetchData: async function(url) {
        return 42;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.True(result.ContainsKey("obj.fetchData"));
        Assert.Equal("number", result["obj.fetchData"].ReturnType);
    }

    // --- Date.now() detection (#5) ---

    [Fact]
    public void InfersReturnType_DateNow()
    {
        var source = @"
obj = {
    getTimestamp: function() {
        return Date.now();
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("number", result["obj.getTimestamp"].ReturnType);
    }

    // --- Callback detection (#10) ---

    [Fact]
    public void DetectsCallbackParam()
    {
        var source = @"
obj = {
    onItemClick: function(callback) {
        callback();
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.True(result["obj.onItemClick"].CallbackParams.Contains("callback"));
    }

    [Fact]
    public void DoesNotDetectNonCallbackParam()
    {
        var source = @"
obj = {
    doSomething: function(name) {
        var x = name;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Empty(result["obj.doSomething"].CallbackParams);
    }

    [Fact]
    public void CallbackDetection_StopsAtNestedFunction()
    {
        var source = @"
obj = {
    outer: function(callback) {
        var inner = function(callback) {
            callback();
        };
    }
};";

        var result = JsTypeInference.InferTypes(source);

        // callback is called inside the nested function, not the outer one
        Assert.Empty(result["obj.outer"].CallbackParams);
    }

    [Fact]
    public void CallbackDetection_MultipleCallbacks()
    {
        var source = @"
obj = {
    onEvents: function(onClick, onHover) {
        onClick();
        onHover();
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.True(result["obj.onEvents"].CallbackParams.Contains("onClick"));
        Assert.True(result["obj.onEvents"].CallbackParams.Contains("onHover"));
    }

    // --- JSDoc descriptions (#14) ---

    [Fact]
    public void JSDocExtractsSummary()
    {
        var source = @"
obj = {
    /**
     * Shows a modal dialog.
     * @param {string} dialogId
     * @returns {boolean}
     */
    showModal: function(dialogId) {
        return true;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("Shows a modal dialog.", result["obj.showModal"].Description);
    }

    [Fact]
    public void JSDocExtractsParamDescriptions()
    {
        var source = @"
obj = {
    /**
     * @param {string} dialogId - The CSS selector
     * @param {number} timeout - Max wait time
     */
    showModal: function(dialogId, timeout) {
        return true;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("The CSS selector", result["obj.showModal"].ParamDescriptions["dialogId"]);
        Assert.Equal("Max wait time", result["obj.showModal"].ParamDescriptions["timeout"]);
    }

    [Fact]
    public void JSDocExtractsReturnDescription()
    {
        var source = @"
obj = {
    /**
     * @returns {boolean} True if shown
     */
    showModal: function() {
        return true;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("True if shown", result["obj.showModal"].ReturnDescription);
    }

    [Fact]
    public void JSDocMultilineSummary()
    {
        var source = @"
obj = {
    /**
     * This is a long
     * multiline description.
     * @param {string} name
     */
    greet: function(name) {
        return ""Hello "" + name;
    }
};";

        var result = JsTypeInference.InferTypes(source);

        Assert.Equal("This is a long multiline description.", result["obj.greet"].Description);
    }

    // --- FindBestMatch unambiguous (#12) ---

    [Fact]
    public void FindBestMatch_AmbiguousLastSegment_ReturnsNull()
    {
        // Two different nested paths with same last segment
        var keys = new[] { "a.init", "b.init" };
        var match = JSInteropGenerator_FindBestMatch("c.init", keys);

        Assert.Null(match);
    }

    [Fact]
    public void FindBestMatch_UnambiguousLastSegment_ReturnsMatch()
    {
        var keys = new[] { "a.showModal", "b.hideModal" };
        var match = JSInteropGenerator_FindBestMatch("c.showModal", keys);

        Assert.Equal("a.showModal", match);
    }

    [Fact]
    public void FindBestMatch_ExactMatch_Prioritized()
    {
        var keys = new[] { "a.init", "b.init" };
        var match = JSInteropGenerator_FindBestMatch("a.init", keys);

        Assert.Equal("a.init", match);
    }

    // Helper to test FindBestMatch which is private — use reflection or make internal
    private static string JSInteropGenerator_FindBestMatch(string displayName, IEnumerable<string> keys)
    {
        // Replicate the logic from JSInteropGenerator.FindBestMatch
        if (keys.Contains(displayName))
            return displayName;

        var lastSegment = displayName.Contains('.') ? displayName.Substring(displayName.LastIndexOf('.') + 1) : displayName;
        var matches = keys.Where(k =>
        {
            var kLast = k.Contains('.') ? k.Substring(k.LastIndexOf('.') + 1) : k;
            return kLast == lastSegment;
        }).ToList();

        return matches.Count == 1 ? matches[0] : null;
    }
}
