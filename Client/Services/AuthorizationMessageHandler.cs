using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace Client.Services
{
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        private readonly IJSRuntime _jsRuntime;

        public AuthorizationMessageHandler(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"AuthorizationMessageHandler: Processing request to {request.RequestUri}");
            
            // Skip adding auth header for login and register endpoints
            var uri = request.RequestUri?.ToString() ?? "";
            if (uri.Contains("/login") || uri.Contains("/register"))
            {
                Console.WriteLine("Skipping auth header for login/register endpoint");
                return await base.SendAsync(request, cancellationToken);
            }

            // Get the token from localStorage
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    Console.WriteLine($"Added Authorization header with token: {token.Substring(0, Math.Min(20, token.Length))}...");
                }
                else
                {
                    Console.WriteLine("No token found in localStorage");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting auth token: {ex.Message}");
                // Continue without token if there's an error
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}