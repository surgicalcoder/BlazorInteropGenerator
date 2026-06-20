using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
namespace GoLive.Generator.BlazorInterop.Playground.Client
{
    public static class JSInterop
    {
        public static string _window_blazorInterop_showModal => "window.blazorInterop.showModal";
        public static async Task showModalVoidAsync (this IJSRuntime JSRuntime )
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.showModal", null);
        }
        public static async Task showModalVoidAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.showModal", cancellationToken);
        }
        public static async Task<T> showModalAsync<T> (this IJSRuntime JSRuntime )
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.showModal",null);
        }
        public static async Task<T> showModalAsync<T> (this IJSRuntime JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.showModal",cancellationToken);
        }
        public static async Task<bool> showModalAsync (this IJSRuntime JSRuntime )
        {
            return await JSRuntime.InvokeAsync<bool>("window.blazorInterop.showModal",null);
        }
        public static async Task<bool> showModalAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<bool>("window.blazorInterop.showModal",cancellationToken);
        }
        public static async Task showModalVoidAsyncModule (this IJSObjectReference JSRuntime )
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.showModal", null);
        }
        public static async Task showModalVoidAsyncModule (this IJSObjectReference JSRuntime , CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.showModal", cancellationToken);
        }
        public static async Task<T> showModalAsyncModule<T> (this IJSObjectReference JSRuntime )
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.showModal",null);
        }
        public static async Task<T> showModalAsyncModule<T> (this IJSObjectReference JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.showModal",cancellationToken);
        }
        public static async Task<bool> showModalAsyncModule (this IJSObjectReference JSRuntime )
        {
            return await JSRuntime.InvokeAsync<bool>("window.blazorInterop.showModal",null);
        }
        public static async Task<bool> showModalAsyncModule (this IJSObjectReference JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<bool>("window.blazorInterop.showModal",cancellationToken);
        }
        public static string _window_blazorInterop_hideModal => "window.blazorInterop.hideModal";
        public static async Task hideModalVoidAsync (this IJSRuntime JSRuntime )
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.hideModal", null);
        }
        public static async Task hideModalVoidAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.hideModal", cancellationToken);
        }
        public static async Task<T> hideModalAsync<T> (this IJSRuntime JSRuntime )
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.hideModal",null);
        }
        public static async Task<T> hideModalAsync<T> (this IJSRuntime JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.hideModal",cancellationToken);
        }
        public static async Task<bool> hideModalAsync (this IJSRuntime JSRuntime )
        {
            return await JSRuntime.InvokeAsync<bool>("window.blazorInterop.hideModal",null);
        }
        public static async Task<bool> hideModalAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<bool>("window.blazorInterop.hideModal",cancellationToken);
        }
        public static async Task hideModalVoidAsyncModule (this IJSObjectReference JSRuntime )
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.hideModal", null);
        }
        public static async Task hideModalVoidAsyncModule (this IJSObjectReference JSRuntime , CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.hideModal", cancellationToken);
        }
        public static async Task<T> hideModalAsyncModule<T> (this IJSObjectReference JSRuntime )
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.hideModal",null);
        }
        public static async Task<T> hideModalAsyncModule<T> (this IJSObjectReference JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.hideModal",cancellationToken);
        }
        public static async Task<bool> hideModalAsyncModule (this IJSObjectReference JSRuntime )
        {
            return await JSRuntime.InvokeAsync<bool>("window.blazorInterop.hideModal",null);
        }
        public static async Task<bool> hideModalAsyncModule (this IJSObjectReference JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<bool>("window.blazorInterop.hideModal",cancellationToken);
        }
        public static string _window_blazorInterop_setPageTitle => "window.blazorInterop.setPageTitle";
        public static async Task setPageTitleVoidAsync (this IJSRuntime JSRuntime )
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.setPageTitle", null);
        }
        public static async Task setPageTitleVoidAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.setPageTitle", cancellationToken);
        }
        public static async Task setPageTitleVoidAsyncModule (this IJSObjectReference JSRuntime )
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.setPageTitle", null);
        }
        public static async Task setPageTitleVoidAsyncModule (this IJSObjectReference JSRuntime , CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.setPageTitle", cancellationToken);
        }
        public static string _window_blazorInterop_datePicker => "window.blazorInterop.datePicker";
        public static async Task datePickerVoidAsync (this IJSRuntime JSRuntime )
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.datePicker", null);
        }
        public static async Task datePickerVoidAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.datePicker", cancellationToken);
        }
        public static async Task datePickerVoidAsyncModule (this IJSObjectReference JSRuntime )
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.datePicker", null);
        }
        public static async Task datePickerVoidAsyncModule (this IJSObjectReference JSRuntime , CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.datePicker", cancellationToken);
        }
        public static string _window_blazorInterop_addNumbers => "window.blazorInterop.addNumbers";
        public static async Task addNumbersVoidAsync (this IJSRuntime JSRuntime )
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.addNumbers", null);
        }
        public static async Task addNumbersVoidAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.addNumbers", cancellationToken);
        }
        public static async Task<T> addNumbersAsync<T> (this IJSRuntime JSRuntime )
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.addNumbers",null);
        }
        public static async Task<T> addNumbersAsync<T> (this IJSRuntime JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.addNumbers",cancellationToken);
        }
        public static async Task<int> addNumbersAsync (this IJSRuntime JSRuntime )
        {
            return await JSRuntime.InvokeAsync<int>("window.blazorInterop.addNumbers",null);
        }
        public static async Task<int> addNumbersAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<int>("window.blazorInterop.addNumbers",cancellationToken);
        }
        public static async Task addNumbersVoidAsyncModule (this IJSObjectReference JSRuntime )
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.addNumbers", null);
        }
        public static async Task addNumbersVoidAsyncModule (this IJSObjectReference JSRuntime , CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.addNumbers", cancellationToken);
        }
        public static async Task<T> addNumbersAsyncModule<T> (this IJSObjectReference JSRuntime )
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.addNumbers",null);
        }
        public static async Task<T> addNumbersAsyncModule<T> (this IJSObjectReference JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.addNumbers",cancellationToken);
        }
        public static async Task<int> addNumbersAsyncModule (this IJSObjectReference JSRuntime )
        {
            return await JSRuntime.InvokeAsync<int>("window.blazorInterop.addNumbers",null);
        }
        public static async Task<int> addNumbersAsyncModule (this IJSObjectReference JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<int>("window.blazorInterop.addNumbers",cancellationToken);
        }
        public static string _window_blazorInterop_formatDate => "window.blazorInterop.formatDate";
        public static async Task formatDateVoidAsync (this IJSRuntime JSRuntime )
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.formatDate", null);
        }
        public static async Task formatDateVoidAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.formatDate", cancellationToken);
        }
        public static async Task<T> formatDateAsync<T> (this IJSRuntime JSRuntime )
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.formatDate",null);
        }
        public static async Task<T> formatDateAsync<T> (this IJSRuntime JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.formatDate",cancellationToken);
        }
        public static async Task<DateTime> formatDateAsync (this IJSRuntime JSRuntime )
        {
            return await JSRuntime.InvokeAsync<DateTime>("window.blazorInterop.formatDate",null);
        }
        public static async Task<DateTime> formatDateAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<DateTime>("window.blazorInterop.formatDate",cancellationToken);
        }
        public static async Task formatDateVoidAsyncModule (this IJSObjectReference JSRuntime )
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.formatDate", null);
        }
        public static async Task formatDateVoidAsyncModule (this IJSObjectReference JSRuntime , CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.formatDate", cancellationToken);
        }
        public static async Task<T> formatDateAsyncModule<T> (this IJSObjectReference JSRuntime )
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.formatDate",null);
        }
        public static async Task<T> formatDateAsyncModule<T> (this IJSObjectReference JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.formatDate",cancellationToken);
        }
        public static async Task<DateTime> formatDateAsyncModule (this IJSObjectReference JSRuntime )
        {
            return await JSRuntime.InvokeAsync<DateTime>("window.blazorInterop.formatDate",null);
        }
        public static async Task<DateTime> formatDateAsyncModule (this IJSObjectReference JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<DateTime>("window.blazorInterop.formatDate",cancellationToken);
        }
        public static string _window_blazorInterop_greetUser => "window.blazorInterop.greetUser";
        public static async Task greetUserVoidAsync (this IJSRuntime JSRuntime )
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.greetUser", null);
        }
        public static async Task greetUserVoidAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.greetUser", cancellationToken);
        }
        public static async Task<T> greetUserAsync<T> (this IJSRuntime JSRuntime )
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.greetUser",null);
        }
        public static async Task<T> greetUserAsync<T> (this IJSRuntime JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.greetUser",cancellationToken);
        }
        public static async Task<int> greetUserAsync (this IJSRuntime JSRuntime )
        {
            return await JSRuntime.InvokeAsync<int>("window.blazorInterop.greetUser",null);
        }
        public static async Task<int> greetUserAsync (this IJSRuntime JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<int>("window.blazorInterop.greetUser",cancellationToken);
        }
        public static async Task greetUserVoidAsyncModule (this IJSObjectReference JSRuntime )
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.greetUser", null);
        }
        public static async Task greetUserVoidAsyncModule (this IJSObjectReference JSRuntime , CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.greetUser", cancellationToken);
        }
        public static async Task<T> greetUserAsyncModule<T> (this IJSObjectReference JSRuntime )
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.greetUser",null);
        }
        public static async Task<T> greetUserAsyncModule<T> (this IJSObjectReference JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.greetUser",cancellationToken);
        }
        public static async Task<int> greetUserAsyncModule (this IJSObjectReference JSRuntime )
        {
            return await JSRuntime.InvokeAsync<int>("window.blazorInterop.greetUser",null);
        }
        public static async Task<int> greetUserAsyncModule (this IJSObjectReference JSRuntime , CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<int>("window.blazorInterop.greetUser",cancellationToken);
        }
    }
}
