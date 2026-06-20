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
}
