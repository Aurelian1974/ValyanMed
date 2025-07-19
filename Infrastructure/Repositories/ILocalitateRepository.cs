using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.DTOs;

namespace Infrastructure.Repositories
{
    public interface ILocalitateRepository
    {
        Task<IEnumerable<LocalitateDto>> GetByJudetAsync(int idJudet);
    }
}
