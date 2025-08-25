using Shared.DTOs.Medical;
using Shared.Common;

namespace Application.Services.Medical;

public interface IPacientService
{
    Task<Result<PagedResult<PacientListDto>>> GetPagedAsync(PacientiSearchQuery searchQuery);
    Task<Result<PacientListDto?>> GetByIdAsync(Guid pacientId);
    Task<Result<Guid>> CreateAsync(CreatePacientRequest request);
    Task<Result<bool>> UpdateAsync(UpdatePacientRequest request);
    Task<Result<bool>> DeleteAsync(Guid pacientId);
}