using System.Threading.Tasks;

namespace ValyanMed.Client.Services
{
    /// <summary>
    /// Interface for authentication providers that support logout functionality
    /// </summary>
    public interface ILogoutService
    {
        /// <summary>
        /// Logs out the current user asynchronously
        /// </summary>
        Task LogoutAsync();
    }
}