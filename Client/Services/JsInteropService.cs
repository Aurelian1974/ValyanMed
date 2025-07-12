using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Client.Services
{
    public class JsInteropService
    {
        private readonly IJSRuntime _jsRuntime;

        public JsInteropService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string> SayHelloAsync(string name)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("appInterop.sayHello", name);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling JavaScript: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        public async Task<bool> LogoutOnCloseAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<bool>("appInterop.logoutOnClose");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling JavaScript logout: {ex.Message}");
                return false;
            }
        }
    }
}