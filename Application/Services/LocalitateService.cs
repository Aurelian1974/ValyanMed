using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.DTOs;
using Core.Interfaces;

namespace Application.Services
{
    public class LocalitateService : ILocalitateService
    {
        private readonly ILocalitateRepository _repo;

        public LocalitateService(ILocalitateRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<LocalitateDto>> GetByJudetAsync(int idJudet)
        {
            return _repo.GetByJudetAsync(idJudet);
        }
    }
}
