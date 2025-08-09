using Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Services
{
    public interface IUtilizatorService
    {
        Task<List<UtilizatorDTO>> GetAllUtilizatoriAsync();
        Task<UtilizatorDTO> GetUtilizatorByIdAsync(int id);
        Task<int> CreateUtilizatorAsync(CreateUtilizatorDTO utilizator);
        Task<bool> UpdateUtilizatorAsync(UpdateUtilizatorDTO utilizator);
        Task<DeleteResult> DeleteUtilizatorAsync(int id);
    }

    public class DeleteResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
