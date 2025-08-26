using Shared.DTOs.Medical;
using Shared.Common;

namespace Application.Services.Medical;

public interface IPacientRepository
{
    Task<PagedResult<PacientListDto>> GetPagedAsync(PacientiSearchQuery searchQuery);
    Task<PacientListDto?> GetByIdAsync(Guid pacientId);
    Task<PacientListDto?> GetByCNPAsync(string cnp);
    Task<PacientListDto?> GetByEmailAsync(string email);
    Task<Guid> CreateAsync(CreatePacientRequest request);
    Task<bool> UpdateAsync(UpdatePacientRequest request);
    Task<bool> DeleteAsync(Guid pacientId);
}
