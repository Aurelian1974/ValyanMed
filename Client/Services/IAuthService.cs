using System.Threading.Tasks;

namespace ValyanMed.Client.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> Register(UtilizatorRegisterDto model);
        Task<(bool Success, string Message, string Token)> Login(string numeUtilizatorSauEmail, string parola);
        Task Logout();
    }
}