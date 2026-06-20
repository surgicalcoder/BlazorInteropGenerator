# BlazorInteropGenerator

Generates Blazor ↔ JavaScript strongly typed interop methods by parsing the JavaScript source and generating extension methods for `IJSRuntime`.

## Features

- **Automatic type inference** via Esprima AST parsing + JSDoc `@param {type}` / `@returns {type}` comments
- **Arrow function & async function** support
- **Callback parameter** detection → generates `Action` delegate types
- **Date.now()** → `number` detection
- **Nested class generation** for JS object hierarchies (e.g. `window.blazorInterop.modal.show` → nested `Modal` static class)
- **JSDoc descriptions** extracted (`@param` descriptions, `@returns` descriptions, summary text)
- **Config validation** with diagnostics on errors (missing source, empty ObjectToInterop)
- **JSON Schema** for `BlazorInterop.json` editor autocomplete
- **Simplified output** — max 3 methods per JS function (Void, Generic, Typed) instead of 18

## Usage

Install via NuGet: [GoLive.Generator.BlazorInterop](https://www.nuget.org/packages/GoLive.Generator.BlazorInterop/)

Add an AdditionalFile in your `.csproj`:

```xml
<ItemGroup>
     <AdditionalFiles Include="BlazorInterop.json" />
</ItemGroup>
```

Add the settings file and configure:

```json
{
  "$schema": "./blazorinterop.schema.json",
  "Files": [
    {
      "Output": "JSInterop.cs",
      "Source": "wwwroot/blazorinterop.js",
      "Namespace": "MyApp.Client",
      "ObjectToInterop": "window.blazorInterop",
      "Init": ["window={}"]
    }
  ],
  "InvokeVoidString": "await JSRuntime.InvokeVoidAsync(\"{0}\", {1});",
  "InvokeString": "return await JSRuntime.InvokeAsync<T>(\"{0}\",{1});"
}
```

### Config Options

| Key | Description |
|-----|-------------|
| `Files` | Array of file objects for interop generation |
| `Files[].Output` | Generated C# filename |
| `Files[].Source` | Path to JavaScript source file |
| `Files[].Namespace` | C# namespace for generated code |
| `Files[].ObjectToInterop` | JS object to interop (e.g. `window.blazorInterop`) |
| `Files[].Init` | Init scripts executed before interop (e.g. `["window={}"]`) |
| `InvokeVoidString` | Template for void calls. `{0}` = function name, `{1}` = args |
| `InvokeString` | Template for returning calls. `{0}` = function name, `{1}` = args |

### Type Inference

Types are inferred automatically from JavaScript source:

1. **JSDoc annotations** — `@param {string} name`, `@returns {boolean}`
2. **AST analysis** — return statement expression types (literals, `Date.now()`, template literals, comparisons, etc.)
3. **Callback detection** — params called as functions → `Action` type
4. **Fallback** — `object` when ambiguous

Supported JSDoc types: `string`, `number`, `boolean`, `datetime`, `object`, `array`, `void`.

### Generated Output Example

For JS:
```js
window.blazorInterop = {
    /** @param {string} dialogId @returns {boolean} */
    showModal: function(dialogId) { return true; },
    modal: {
        /** @param {string} text */
        setText: function(text) { ... }
    }
}
```

Generates:
```csharp
public static class JSInterop
{
    public static string _showModal => "showModal";
    public static async Task<bool> showModalAsync(this IJSRuntime JSRuntime, string dialogId, CancellationToken ct = default)
        => await JSRuntime.InvokeAsync<bool>("showModal", new object[] { dialogId }, ct);

    public static class modal
    {
        public static string _setText => "modal.setText";
        public static async Task setTextVoidAsync(this IJSRuntime JSRuntime, string text, CancellationToken ct = default)
            => await JSRuntime.InvokeVoidAsync("modal.setText", new object[] { text }, ct);
    }
}
```

### Diagnostics

| ID | Severity | Description |
|----|----------|-------------|
| BI001 | Error | JS parse error (Esprima/Jint) |
| BI002 | Error | Config error (missing file, invalid JSON, empty ObjectToInterop) |
| BI003 | Warning | Type inference fallback (unknown type → `object`) |