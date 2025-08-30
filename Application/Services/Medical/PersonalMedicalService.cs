using FluentValidation;
using Shared.Common;
using Shared.DTOs.Medical;

namespace Application.Services.Medical;

public interface IPersonalMedicalService
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

public class PersonalMedicalService : IPersonalMedicalService
{
    private readonly IPersonalMedicalRepository _repository;
    private readonly IValidator<CreatePersonalMedicalRequest> _createValidator;
    private readonly IValidator<UpdatePersonalMedicalRequest> _updateValidator;

    public PersonalMedicalService(
        IPersonalMedicalRepository repository,
        IValidator<CreatePersonalMedicalRequest> createValidator,
        IValidator<UpdatePersonalMedicalRequest> updateValidator)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<PersonalMedicalDataGridResult>> GetDataGridAsync(PersonalMedicalDataGridRequest request)
    {
        // Validation pentru paginare ?i grupare
        if (request.Skip < 0) request.Skip = 0;
        if (request.Take < 1) request.Take = 10;
        if (request.Take > 500) request.Take = 500; // Limit?m pentru performan??

        return await _repository.GetDataGridAsync(request);
    }

    public async Task<Result<PagedResult<PersonalMedicalListDto>>> GetPagedAsync(PersonalMedicalSearchQuery query)
    {
        // Validation for paging - FIXED: Allow larger page sizes for dropdown population
        if (query.Page < 1) query.Page = 1;
        
        // Allow larger page sizes for loading dropdown data, but with reasonable upper limit
        if (query.PageSize < 1) 
        {
            query.PageSize = 10;
        }
        else if (query.PageSize > 2000) // Increased from 100 to 2000 for dropdown loading
        {
            query.PageSize = 2000; // Cap at 2000 to prevent excessive memory usage
        }

        return await _repository.GetPagedAsync(query);
    }

    public async Task<Result<IEnumerable<PersonalMedicalGroupAggregateDto>>> GetGroupAggregatesAsync(PersonalMedicalGroupAggregateQuery query)
    {
        // Whitelist group by handled in repository; here we can add light validation if needed
        return await _repository.GetGroupAggregatesAsync(query);
    }

    public async Task<Result<PersonalMedicalDetailDto?>> GetByIdAsync(Guid personalId)
    {
        if (personalId == Guid.Empty)
        {
            return Result<PersonalMedicalDetailDto?>.Failure("ID personal medical invalid");
        }

        return await _repository.GetByIdAsync(personalId);
    }

    public async Task<Result<Guid>> CreateAsync(CreatePersonalMedicalRequest request)
    {
        // Validation
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<Guid>.Failure(errors);
        }

        // Business rules
        var businessValidation = await ValidateBusinessRulesForCreate(request);
        if (!businessValidation.IsSuccess)
        {
            return Result<Guid>.Failure(businessValidation.Errors);
        }

        return await _repository.CreateAsync(request);
    }

    public async Task<Result> UpdateAsync(UpdatePersonalMedicalRequest request)
    {
        // Validation
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result.Failure(errors);
        }

        // Check if exists
        var existingResult = await _repository.GetByIdAsync(request.PersonalID);
        if (!existingResult.IsSuccess)
        {
            return Result.Failure(existingResult.Errors);
        }
        if (existingResult.Value == null)
        {
            return Result.Failure("Personal medical nu a fost gasit");
        }

        // Business rules
        var businessValidation = await ValidateBusinessRulesForUpdate(request);
        if (!businessValidation.IsSuccess)
        {
            return Result.Failure(businessValidation.Errors);
        }

        return await _repository.UpdateAsync(request);
    }

    public async Task<Result> DeleteAsync(Guid personalId)
    {
        if (personalId == Guid.Empty)
        {
            return Result.Failure("ID personal medical invalid");
        }

        // Check if exists
        var existingResult = await _repository.GetByIdAsync(personalId);
        if (!existingResult.IsSuccess)
        {
            return Result.Failure(existingResult.Errors);
        }
        if (existingResult.Value == null)
        {
            return Result.Failure("Personal medical nu a fost gasit");
        }

        // Business rule: Check if has active appointments
        if (existingResult.Value.TotalProgramari > 0)
        {
            return Result.Failure("Nu se poate sterge personal medical cu programari active. Dezactivati mai intai.");
        }

        return await _repository.DeleteAsync(personalId);
    }

    public async Task<Result<IEnumerable<PersonalMedicalListDto>>> GetActiveDoctorsAsync()
    {
        return await _repository.GetActiveDoctorsAsync();
    }

    private async Task<Result> ValidateBusinessRulesForCreate(CreatePersonalMedicalRequest request)
    {
        var errors = new List<string>();

        // Check for duplicate license number if provided
        if (!string.IsNullOrWhiteSpace(request.NumarLicenta))
        {
            // This would need a method to check duplicates
            // For now, we'll skip this validation
        }

        // Position-specific validations
        if (request.Pozitie.Contains("Doctor") || request.Pozitie.Contains("Medic"))
        {
            if (string.IsNullOrWhiteSpace(request.NumarLicenta))
            {
                errors.Add("Doctorul trebuie sa aibe numar de licenta");
            }
            if (string.IsNullOrWhiteSpace(request.Specializare))
            {
                errors.Add("Doctorul trebuie sa aiba specializare");
            }
        }

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    private async Task<Result> ValidateBusinessRulesForUpdate(UpdatePersonalMedicalRequest request)
    {
        var errors = new List<string>();

        // Position-specific validations
        if (request.Pozitie.Contains("Doctor") || request.Pozitie.Contains("Medic"))
        {
            if (string.IsNullOrWhiteSpace(request.NumarLicenta))
            {
                errors.Add("Doctorul trebuie sa aibe numar de licenta");
            }
            if (string.IsNullOrWhiteSpace(request.Specializare))
            {
                errors.Add("Doctorul trebuie sa aiba specializare");
            }
        }

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }
}