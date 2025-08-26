using Shared.Common;
using Shared.Models.Authentication;

namespace Application.Services.Authentication;

public interface IPersoanaRepository
{
    Task<Result<int>> CreateAsync(Persoana persoana);
    Task<Result<Persoana?>> GetByIdAsync(int id);
    Task<Result<IEnumerable<Persoana>>> GetAllAsync();
    Task<Result> UpdateAsync(Persoana persoana);
    Task<Result> DeleteAsync(int id);
    Task<Result<bool>> ExistsByCNPAsync(string cnp, int? excludeId = null);
}
