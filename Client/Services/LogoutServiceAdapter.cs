using System.Threading.Tasks;
using ValyanMed.Client.Services;

namespace Client.Services
{
    public class LogoutServiceAdapter : ILogoutService
    {
        private readonly ValyanMed.Client.Services.ILogoutService _innerService;

        public LogoutServiceAdapter(ValyanMed.Client.Services.ILogoutService innerService)
        {
            _innerService = innerService;
        }

        public async Task LogoutAsync()
        {
            await _innerService.LogoutAsync();
        }
    }
}