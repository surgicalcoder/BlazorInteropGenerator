using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace GoLive.Generator.BlazorInterop.Playground.Client
{
    public static class JSInterop
    {
        public static string _window_blazorInterop_showModal => "window.blazorInterop.showModal";

        public static async Task showModalVoidAsync(this IJSRuntime JSRuntime, object @dialogId)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.showModal", @dialogId);
        }

        public static async Task<T> showModalAsync<T>(this IJSRuntime JSRuntime, object @dialogId)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.showModal", @dialogId);
        }

        public static string _window_blazorInterop_hideModal => "window.blazorInterop.hideModal";

        public static async Task hideModalVoidAsync(this IJSRuntime JSRuntime, object @dialogId)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.hideModal", @dialogId);
        }

        public static async Task<T> hideModalAsync<T>(this IJSRuntime JSRuntime, object @dialogId)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.hideModal", @dialogId);
        }

        public static string _window_blazorInterop_setPageTitle => "window.blazorInterop.setPageTitle";

        public static async Task setPageTitleVoidAsync(this IJSRuntime JSRuntime, object @title)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.setPageTitle", @title);
        }

        public static async Task<T> setPageTitleAsync<T>(this IJSRuntime JSRuntime, object @title)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.setPageTitle", @title);
        }

        public static string _window_blazorInterop_datePicker => "window.blazorInterop.datePicker";

        public static async Task datePickerVoidAsync(this IJSRuntime JSRuntime, object @id)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.datePicker", @id);
        }

        public static async Task<T> datePickerAsync<T>(this IJSRuntime JSRuntime, object @id)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.datePicker", @id);
        }
    }
}