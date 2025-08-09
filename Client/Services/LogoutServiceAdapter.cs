using System.Threading.Tasks;

namespace Client.Services
{
    public class LogoutServiceAdapter : ILogoutService
    {
        private readonly ILogoutService _innerService;

        public LogoutServiceAdapter(ILogoutService innerService)
        {
            _innerService = innerService;
        }

        public async Task LogoutAsync()
        {
            await _innerService.LogoutAsync();
        }
    }
}