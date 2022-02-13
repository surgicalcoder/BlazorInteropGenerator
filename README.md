# BlazorInteropGenerator
Generates Blazor -> Javascript strongly typed interop methods, by parsing the Javascript it self and generating extension methods for IJSRuntime.

## Usage

Firstly, add the project from Nuget - [GoLive.Generator.BlazorInterop](https://www.nuget.org/packages/GoLive.Generator.BlazorInterop/), then add an AdditionalFile in your .csproj named "ApiClientGenerator.json", like so:

```
<ItemGroup>
     <AdditionalFiles Include="BlazorInterop.json" />
</ItemGroup>
```

Once that's done, add the settings file and change as required:


```
{
  "JavascriptFile": "wwwroot\\assets\\js\\blazorinterop.js",
  "JSInit": [
    "window={}"
  ],
  "MainJsObject": "window.blazor",
  "InvokeVoidString": "await JSRuntime.InvokeVoidAsync(\"{0}\", {1});",
  "InvokeString": "return await JSRuntime.InvokeAsync<T>(\"{0}\",{1});"
}

```

The `JSInt` option is there if you need to init a value, such as if you have all of your methods under `window.blazor`, you need to init `window`.
