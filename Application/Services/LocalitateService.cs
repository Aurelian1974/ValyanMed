using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.DTOs;
using Application.Services;
using Infrastructure.Repositories;

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
