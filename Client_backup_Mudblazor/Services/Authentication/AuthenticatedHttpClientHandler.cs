using System.Net.Http.Headers;
using Client.Services.Authentication;

namespace Client.Services.Authentication;

public class AuthenticatedHttpClientHandler : DelegatingHandler
{
    private readonly ITokenStorageService _tokenStorage;

    public AuthenticatedHttpClientHandler(ITokenStorageService tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            // Încarc? token-ul din storage
            var token = await _tokenStorage.GetTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't prevent request
            Console.WriteLine($"Error adding auth header: {ex.Message}");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}