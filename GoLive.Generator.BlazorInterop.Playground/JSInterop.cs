using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace GoLive.Generator.BlazorInterop.Playground.Client
{
    public static class JSInterop
    {
        public static string _window_blazorInterop_showModal => "window.blazorInterop.showModal";

        public static async Task showModalVoidAsync(this IJSRuntime JSRuntime, string @dialogId)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.showModal", @dialogId);
        }

        public static async Task showModalVoidAsync(this IJSRuntime JSRuntime, string @dialogId, CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.showModal", cancellationToken, @dialogId);
        }

        public static async Task<T> showModalAsync<T>(this IJSRuntime JSRuntime, string @dialogId)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.showModal", @dialogId);
        }

        public static async Task<T> showModalAsync<T>(this IJSRuntime JSRuntime, string @dialogId, CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.showModal", cancellationToken, @dialogId);
        }

        public static async Task<bool> showModalAsync(this IJSRuntime JSRuntime, string @dialogId)
        {
            return await JSRuntime.InvokeAsync<bool>("window.blazorInterop.showModal", @dialogId);
        }

        public static async Task<bool> showModalAsync(this IJSRuntime JSRuntime, string @dialogId, CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<bool>("window.blazorInterop.showModal", cancellationToken, @dialogId);
        }

        public static string _window_blazorInterop_hideModal => "window.blazorInterop.hideModal";

        public static async Task hideModalVoidAsync(this IJSRuntime JSRuntime, string @dialogId)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.hideModal", @dialogId);
        }

        public static async Task hideModalVoidAsync(this IJSRuntime JSRuntime, string @dialogId, CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.hideModal", cancellationToken, @dialogId);
        }

        public static async Task<T> hideModalAsync<T>(this IJSRuntime JSRuntime, string @dialogId)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.hideModal", @dialogId);
        }

        public static async Task<T> hideModalAsync<T>(this IJSRuntime JSRuntime, string @dialogId, CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.hideModal", cancellationToken, @dialogId);
        }

        public static async Task<bool> hideModalAsync(this IJSRuntime JSRuntime, string @dialogId)
        {
            return await JSRuntime.InvokeAsync<bool>("window.blazorInterop.hideModal", @dialogId);
        }

        public static async Task<bool> hideModalAsync(this IJSRuntime JSRuntime, string @dialogId, CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<bool>("window.blazorInterop.hideModal", cancellationToken, @dialogId);
        }

        public static string _window_blazorInterop_setPageTitle => "window.blazorInterop.setPageTitle";

        public static async Task setPageTitleVoidAsync(this IJSRuntime JSRuntime, string @title)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.setPageTitle", @title);
        }

        public static async Task setPageTitleVoidAsync(this IJSRuntime JSRuntime, string @title, CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.setPageTitle", cancellationToken, @title);
        }

        public static string _window_blazorInterop_datePicker => "window.blazorInterop.datePicker";

        public static async Task datePickerVoidAsync(this IJSRuntime JSRuntime, string @id)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.datePicker", @id);
        }

        public static async Task datePickerVoidAsync(this IJSRuntime JSRuntime, string @id, CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.datePicker", cancellationToken, @id);
        }

        public static string _window_blazorInterop_addNumbers => "window.blazorInterop.addNumbers";

        public static async Task addNumbersVoidAsync(this IJSRuntime JSRuntime, int @x, int @y)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.addNumbers", @x, @y);
        }

        public static async Task addNumbersVoidAsync(this IJSRuntime JSRuntime, int @x, int @y, CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.addNumbers", cancellationToken, @x, @y);
        }

        public static async Task<T> addNumbersAsync<T>(this IJSRuntime JSRuntime, int @x, int @y)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.addNumbers", @x, @y);
        }

        public static async Task<T> addNumbersAsync<T>(this IJSRuntime JSRuntime, int @x, int @y, CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.addNumbers", cancellationToken, @x, @y);
        }

        public static async Task<int> addNumbersAsync(this IJSRuntime JSRuntime, int @x, int @y)
        {
            return await JSRuntime.InvokeAsync<int>("window.blazorInterop.addNumbers", @x, @y);
        }

        public static async Task<int> addNumbersAsync(this IJSRuntime JSRuntime, int @x, int @y, CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<int>("window.blazorInterop.addNumbers", cancellationToken, @x, @y);
        }

        public static string _window_blazorInterop_formatDate => "window.blazorInterop.formatDate";

        public static async Task formatDateVoidAsync(this IJSRuntime JSRuntime, int @day, int @month, int @year)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.formatDate", @day, @month, @year);
        }

        public static async Task formatDateVoidAsync(this IJSRuntime JSRuntime, int @day, int @month, int @year, CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.formatDate", cancellationToken, @day, @month, @year);
        }

        public static async Task<T> formatDateAsync<T>(this IJSRuntime JSRuntime, int @day, int @month, int @year)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.formatDate", @day, @month, @year);
        }

        public static async Task<T> formatDateAsync<T>(this IJSRuntime JSRuntime, int @day, int @month, int @year, CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.formatDate", cancellationToken, @day, @month, @year);
        }

        public static async Task<DateTime> formatDateAsync(this IJSRuntime JSRuntime, int @day, int @month, int @year)
        {
            return await JSRuntime.InvokeAsync<DateTime>("window.blazorInterop.formatDate", @day, @month, @year);
        }

        public static async Task<DateTime> formatDateAsync(this IJSRuntime JSRuntime, int @day, int @month, int @year, CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<DateTime>("window.blazorInterop.formatDate", cancellationToken, @day, @month, @year);
        }

        public static string _window_blazorInterop_greetUser => "window.blazorInterop.greetUser";

        public static async Task greetUserVoidAsync(this IJSRuntime JSRuntime, string @name, bool @isFormal)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.greetUser", @name, @isFormal);
        }

        public static async Task greetUserVoidAsync(this IJSRuntime JSRuntime, string @name, bool @isFormal, CancellationToken cancellationToken)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorInterop.greetUser", cancellationToken, @name, @isFormal);
        }

        public static async Task<T> greetUserAsync<T>(this IJSRuntime JSRuntime, string @name, bool @isFormal)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.greetUser", @name, @isFormal);
        }

        public static async Task<T> greetUserAsync<T>(this IJSRuntime JSRuntime, string @name, bool @isFormal, CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<T>("window.blazorInterop.greetUser", cancellationToken, @name, @isFormal);
        }

        public static async Task<string> greetUserAsync(this IJSRuntime JSRuntime, string @name, bool @isFormal)
        {
            return await JSRuntime.InvokeAsync<string>("window.blazorInterop.greetUser", @name, @isFormal);
        }

        public static async Task<string> greetUserAsync(this IJSRuntime JSRuntime, string @name, bool @isFormal, CancellationToken cancellationToken)
        {
            return await JSRuntime.InvokeAsync<string>("window.blazorInterop.greetUser", cancellationToken, @name, @isFormal);
        }
    }
}