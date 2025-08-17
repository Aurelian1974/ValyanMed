using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.DTOs;

namespace Application.Services
{
    public interface IJudetService
    {
        Task<IEnumerable<JudetDto>> GetAllAsync();
    }
}