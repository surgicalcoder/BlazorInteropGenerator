using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace GoLive.Generator.BlazorInterop.Playground.Client
{
    public static class JSInterop
    {
        public static string _window_blazorInterop_showModal => "window.blazorInterop.showModal";

        public static async Task showModalVoidAsync(this IJSRuntime JSRuntime)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.showModal", null);
        }

        public static async Task<T> showModalAsync<T>(this IJSRuntime JSRuntime)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.showModal", null);
        }

        public static string _window_blazorInterop_hideModal => "window.blazorInterop.hideModal";

        public static async Task hideModalVoidAsync(this IJSRuntime JSRuntime)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.hideModal", null);
        }

        public static async Task<T> hideModalAsync<T>(this IJSRuntime JSRuntime)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.hideModal", null);
        }

        public static string _window_blazorInterop_setPageTitle => "window.blazorInterop.setPageTitle";

        public static async Task setPageTitleVoidAsync(this IJSRuntime JSRuntime)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.setPageTitle", null);
        }

        public static async Task<T> setPageTitleAsync<T>(this IJSRuntime JSRuntime)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.setPageTitle", null);
        }
    }
}