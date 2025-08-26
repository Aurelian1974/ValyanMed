using Shared.Common;
using Shared.DTOs.Authentication;

namespace Application.Services.Authentication;

public interface IPersoanaService
{
    Task<Result<int>> CreateAsync(CreatePersoanaRequest request);
    Task<Result<PersoanaDto?>> GetByIdAsync(int id);
    Task<Result<IEnumerable<PersoanaDto>>> GetAllAsync();
    Task<Result> UpdateAsync(UpdatePersoanaRequest request);
    Task<Result> DeleteAsync(int id);
}