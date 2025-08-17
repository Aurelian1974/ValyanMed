using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.DTOs;
using Core.Interfaces;

namespace Application.Services
{
    public class JudetService : IJudetService
    {
        private readonly IJudetRepository _repo;
        public JudetService(IJudetRepository repo) => _repo = repo;

        public async Task<IEnumerable<JudetDto>> GetAllAsync() => await _repo.GetAllAsync();
    }
}