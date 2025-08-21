using System.Security.Claims;

namespace API.Services
{
    public interface ICurrentUserService
    {
        string GetCurrentUserName();
        string GetCurrentUserFullName();
        int? GetCurrentUserId();
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentUserName()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            Console.WriteLine($"=== CurrentUserService.GetCurrentUserName ===");
            Console.WriteLine($"HttpContext exists: {_httpContextAccessor.HttpContext != null}");
            Console.WriteLine($"User exists: {user != null}");
            Console.WriteLine($"User authenticated: {user?.Identity?.IsAuthenticated}");
            Console.WriteLine($"User identity name: {user?.Identity?.Name}");
            
            if (user?.Identity?.IsAuthenticated == true)
            {
                // Look for JWT standard claim names that match our token generation
                var userName = user.FindFirst("unique_name")?.Value ?? 
                               user.FindFirst(ClaimTypes.Name)?.Value ?? 
                               "Utilizator Necunoscut";
                Console.WriteLine($"Returning user name: {userName}");
                return userName;
            }
            Console.WriteLine("Returning fallback: Sistem");
            return "Sistem"; // Fallback for system operations
        }

        public string GetCurrentUserFullName()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            Console.WriteLine($"=== CurrentUserService.GetCurrentUserFullName ===");
            
            if (user?.Identity?.IsAuthenticated == true)
            {
                // Print all claims for debugging
                Console.WriteLine($"Total claims: {user.Claims.Count()}");
                foreach (var claim in user.Claims)
                {
                    Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
                }
                
                // Look for the fullName claim that we set in the JWT token
                var fullName = user.FindFirst("fullName")?.Value;
                if (!string.IsNullOrEmpty(fullName))
                {
                    Console.WriteLine($"Returning full name from claim: {fullName}");
                    return fullName;
                }
                    
                var userName = GetCurrentUserName();
                Console.WriteLine($"Returning user name as fallback: {userName}");
                return userName;
            }
            Console.WriteLine("Returning fallback: Sistem");
            return "Sistem"; // Fallback for system operations
        }

        public int? GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                // Look for JWT standard "sub" claim that matches our token generation
                var userIdClaim = user.FindFirst("sub") ?? user.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            return null;
        }
    }
}