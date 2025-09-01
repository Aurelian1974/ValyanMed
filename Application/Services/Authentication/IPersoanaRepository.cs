using Shared.Common;
using Shared.Models.Authentication;
using Shared.DTOs.Authentication;

namespace Application.Services.Authentication;

public interface IPersoanaRepository
{
    Task<Result<int>> CreateAsync(Persoana persoana);
    Task<Result<Persoana?>> GetByIdAsync(int id);
    Task<Result<IEnumerable<Persoana>>> GetAllAsync();
    Task<Result<PagedResult<PersoanaListDto>>> GetPagedAsync(PersoanaSearchQuery query);
    Task<Result> UpdateAsync(Persoana persoana);
    Task<Result> DeleteAsync(int id);
    Task<Result<bool>> ExistsByCNPAsync(string cnp, int? excludeId = null);
}
