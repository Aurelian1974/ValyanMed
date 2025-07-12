using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Client.Services
{
    public class BrowserEventService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogoutService _logoutService;
        private DotNetObjectReference<BrowserEventService> _dotNetObjectReference;
        private bool _initialized = false;

        public BrowserEventService(IJSRuntime jsRuntime, ILogoutService logoutService)
        {
            _jsRuntime = jsRuntime;
            _logoutService = logoutService;
            _dotNetObjectReference = DotNetObjectReference.Create(this);
        }

        public async Task InitializeAsync()
        {
            if (_initialized)
                return;

            try
            {
                await _jsRuntime.InvokeVoidAsync("browserInterop.registerBeforeUnload", _dotNetObjectReference);
                _initialized = true;
                Console.WriteLine("Browser close detection initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing browser close detection: {ex.Message}");
            }
        }

        [JSInvokable]
        public async Task OnBeforeUnload()
        {
            try
            {
                await _logoutService.LogoutAsync();
                Console.WriteLine("Logout executed during browser close");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during logout: {ex.Message}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            _dotNetObjectReference?.Dispose();
            await ValueTask.CompletedTask;
        }
    }

    // Interface for auth providers that support logout
    public interface ILogoutService
    {
        Task LogoutAsync();
    }
}