// Client/Services/ILocalitateService.cs
using Shared.DTOs;

namespace Client.Services
{
    public interface ILocalitateService
    {
        Task<List<LocalitateDto>> GetByJudetAsync(int idJudet);
    }
}
