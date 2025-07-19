using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.DTOs;

namespace Application.Services
{
    public interface ILocalitateService
    {
        Task<IEnumerable<LocalitateDto>> GetByJudetAsync(int idJudet);
    }
}
