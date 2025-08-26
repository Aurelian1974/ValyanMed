using Shared.Common;
using Shared.DTOs.Authentication;

namespace Application.Services.Authentication;

public interface IUtilizatorService
{
    Task<Result<int>> CreateAsync(CreateUtilizatorRequest request);
    Task<Result<UtilizatorDto?>> GetByIdAsync(int id);
    Task<Result<IEnumerable<UtilizatorDto>>> GetAllAsync();
    Task<Result> UpdateAsync(UpdateUtilizatorRequest request);
    Task<Result> DeleteAsync(int id);
}