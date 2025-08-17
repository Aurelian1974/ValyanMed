using Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IJudetRepository
    {
        Task<IEnumerable<JudetDto>> GetAllAsync();
    }
}
