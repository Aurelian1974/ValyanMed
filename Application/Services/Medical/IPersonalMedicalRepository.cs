using Shared.Common;
using Shared.DTOs.Medical;

namespace Application.Services.Medical;

public interface IPersonalMedicalRepository
{
    Task<Result<PagedResult<PersonalMedicalListDto>>> GetPagedAsync(PersonalMedicalSearchQuery query);
    Task<Result<PersonalMedicalDataGridResult>> GetDataGridAsync(PersonalMedicalDataGridRequest request);
    Task<Result<IEnumerable<PersonalMedicalGroupAggregateDto>>> GetGroupAggregatesAsync(PersonalMedicalGroupAggregateQuery query);
    Task<Result<PersonalMedicalDetailDto?>> GetByIdAsync(Guid personalId);
    Task<Result<Guid>> CreateAsync(CreatePersonalMedicalRequest request);
    Task<Result> UpdateAsync(UpdatePersonalMedicalRequest request);
    Task<Result> DeleteAsync(Guid personalId);
    Task<Result<IEnumerable<PersonalMedicalListDto>>> GetActiveDoctorsAsync();
}