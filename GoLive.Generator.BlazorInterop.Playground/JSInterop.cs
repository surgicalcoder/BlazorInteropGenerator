using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
namespace GoLive.Generator.BlazorInterop.Playground.Client
{
    public static class JSInterop
    {
        public static string _window_blazorInterop_showModal => "window.blazorInterop.showModal";
        public static async Task showModalVoidAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken = default)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.showModal", cancellationToken);
        }
        public static async Task<T> showModalAsync<T> (this IJSRuntime JSRuntime , CancellationToken cancellationToken = default)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.showModal",cancellationToken);
        }
        public static async Task<bool> showModalAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken = default)
        {
            return await JSRuntime.InvokeAsync<bool>("window.blazorInterop.showModal",cancellationToken);
        }
        public static string _window_blazorInterop_hideModal => "window.blazorInterop.hideModal";
        public static async Task hideModalVoidAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken = default)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.hideModal", cancellationToken);
        }
        public static async Task<T> hideModalAsync<T> (this IJSRuntime JSRuntime , CancellationToken cancellationToken = default)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.hideModal",cancellationToken);
        }
        public static async Task<bool> hideModalAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken = default)
        {
            return await JSRuntime.InvokeAsync<bool>("window.blazorInterop.hideModal",cancellationToken);
        }
        public static string _window_blazorInterop_setPageTitle => "window.blazorInterop.setPageTitle";
        public static async Task setPageTitleVoidAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken = default)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.setPageTitle", cancellationToken);
        }
        public static string _window_blazorInterop_datePicker => "window.blazorInterop.datePicker";
        public static async Task datePickerVoidAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken = default)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.datePicker", cancellationToken);
        }
        public static string _window_blazorInterop_addNumbers => "window.blazorInterop.addNumbers";
        public static async Task addNumbersVoidAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken = default)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.addNumbers", cancellationToken);
        }
        public static async Task<T> addNumbersAsync<T> (this IJSRuntime JSRuntime , CancellationToken cancellationToken = default)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.addNumbers",cancellationToken);
        }
        public static async Task<int> addNumbersAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken = default)
        {
            return await JSRuntime.InvokeAsync<int>("window.blazorInterop.addNumbers",cancellationToken);
        }
        public static string _window_blazorInterop_formatDate => "window.blazorInterop.formatDate";
        public static async Task formatDateVoidAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken = default)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.formatDate", cancellationToken);
        }
        public static async Task<T> formatDateAsync<T> (this IJSRuntime JSRuntime , CancellationToken cancellationToken = default)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.formatDate",cancellationToken);
        }
        public static async Task<DateTime> formatDateAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken = default)
        {
            return await JSRuntime.InvokeAsync<DateTime>("window.blazorInterop.formatDate",cancellationToken);
        }
        public static string _window_blazorInterop_greetUser => "window.blazorInterop.greetUser";
        public static async Task greetUserVoidAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken = default)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.greetUser", cancellationToken);
        }
        public static async Task<T> greetUserAsync<T> (this IJSRuntime JSRuntime , CancellationToken cancellationToken = default)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.greetUser",cancellationToken);
        }
        public static async Task<string> greetUserAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken = default)
        {
            return await JSRuntime.InvokeAsync<string>("window.blazorInterop.greetUser",cancellationToken);
        }
    }
}
