using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace GoLive.Generator.BlazorInterop.Playground.Client
{
    public static class JSInterop
    {
        public static string _window_blazorInterop_setPageTitle => "window.blazorInterop.setPageTitle";

        public static async Task setPageTitleVoidAsync(this IJSRuntime JSRuntime, object @title, object @title2)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.setPageTitle", @title, @title2);
        }

        public static async Task<T> setPageTitleAsync<T>(this IJSRuntime JSRuntime, object @title, object @title2)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.setPageTitle", @title, @title2);
        }
    }
}