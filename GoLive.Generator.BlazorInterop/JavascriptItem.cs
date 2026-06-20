using System.Collections.Generic;

namespace GoLive.Generator.BlazorInterop;

public class MethodParameter
{
    public string Name { get; set; }
    public string Type { get; set; } = "object";
}

public class JavascriptItem
{
    public JavascriptItem()
    {
        Params = new();
    }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string ReturnType { get; set; }
    public List<MethodParameter> Params { get; set; }
}
