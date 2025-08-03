using System.Threading.Tasks;

namespace Client.Services
{
    /// <summary>
    /// Interface for authentication providers that support logout functionality
    /// </summary>
    public interface IClientLogoutService
    {
        /// <summary>
        /// Logs out the current user asynchronously
        /// </summary>
        Task LogoutAsync();
    }
}