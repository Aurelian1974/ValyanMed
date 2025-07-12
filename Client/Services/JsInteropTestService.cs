using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Client.Services
{
    public class JsInteropTestService
    {
        private readonly IJSRuntime _jsRuntime;

        public JsInteropTestService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string> TestInteropAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("appFunctions.test");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JS interop error: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }
    }
}