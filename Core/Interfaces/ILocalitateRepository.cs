using Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ILocalitateRepository
    {
        Task<IEnumerable<LocalitateDto>> GetByJudetAsync(int idJudet);
    }
}
