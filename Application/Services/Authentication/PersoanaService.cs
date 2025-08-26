using Shared.Common;
using Shared.Models.Authentication;
using Shared.DTOs.Authentication;
using Application.Services.Authentication;
using FluentValidation;

namespace Application.Services.Authentication;

public class PersoanaService : IPersoanaService
{
    private readonly IPersoanaRepository _repository;
    private readonly IValidator<CreatePersoanaRequest> _createValidator;
    private readonly IValidator<UpdatePersoanaRequest> _updateValidator;

    public PersoanaService(
        IPersoanaRepository repository,
        IValidator<CreatePersoanaRequest> createValidator,
        IValidator<UpdatePersoanaRequest> updateValidator)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<int>> CreateAsync(CreatePersoanaRequest request)
    {
        // Validare
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<int>.Failure(errors);
        }

        // Verificare CNP duplicat
        if (!string.IsNullOrEmpty(request.CNP))
        {
            var existsResult = await _repository.ExistsByCNPAsync(request.CNP);
            if (!existsResult.IsSuccess)
            {
                return Result<int>.Failure(existsResult.Errors);
            }
            if (existsResult.Value)
            {
                return Result<int>.Failure("CNP-ul exist? deja în sistem");
            }
        }

        // Mapare la domain model
        var persoana = new Persoana
        {
            Nume = request.Nume,
            Prenume = request.Prenume,
            Judet = request.Judet,
            Localitate = request.Localitate,
            Strada = request.Strada,
            NumarStrada = request.NumarStrada,
            CodPostal = request.CodPostal,
            PozitieOrganizatie = request.PozitieOrganizatie,
            DataNasterii = request.DataNasterii,
            CNP = request.CNP,
            TipActIdentitate = request.TipActIdentitate,
            SerieActIdentitate = request.SerieActIdentitate,
            NumarActIdentitate = request.NumarActIdentitate,
            StareCivila = request.StareCivila,
            Gen = request.Gen
        };

        var result = await _repository.CreateAsync(persoana);
        if (!result.IsSuccess)
        {
            return Result<int>.Failure(result.Errors);
        }

        return Result<int>.Success(result.Value, "Persoana a fost creat? cu succes");
    }

    public async Task<Result<PersoanaDto?>> GetByIdAsync(int id)
    {
        var result = await _repository.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            return Result<PersoanaDto?>.Failure(result.Errors);
        }

        if (result.Value == null)
        {
            return Result<PersoanaDto?>.Success(null);
        }

        var dto = MapToDto(result.Value);
        return Result<PersoanaDto?>.Success(dto);
    }

    public async Task<Result<IEnumerable<PersoanaDto>>> GetAllAsync()
    {
        var result = await _repository.GetAllAsync();
        if (!result.IsSuccess)
        {
            return Result<IEnumerable<PersoanaDto>>.Failure(result.Errors);
        }

        var dtos = result.Value?.Select(MapToDto) ?? Enumerable.Empty<PersoanaDto>();
        return Result<IEnumerable<PersoanaDto>>.Success(dtos);
    }

    public async Task<Result> UpdateAsync(UpdatePersoanaRequest request)
    {
        // Validare
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result.Failure(errors);
        }

        // Verificare existen??
        var existingResult = await _repository.GetByIdAsync(request.Id);
        if (!existingResult.IsSuccess)
        {
            return Result.Failure(existingResult.Errors);
        }
        if (existingResult.Value == null)
        {
            return Result.Failure("Persoana nu a fost g?sit?");
        }

        var existingPersoana = existingResult.Value;

        // Verificare CNP duplicat (exclusiv current)
        if (!string.IsNullOrEmpty(request.CNP))
        {
            var existsResult = await _repository.ExistsByCNPAsync(request.CNP, request.Id);
            if (!existsResult.IsSuccess)
            {
                return Result.Failure(existsResult.Errors);
            }
            if (existsResult.Value)
            {
                return Result.Failure("CNP-ul exist? deja în sistem");
            }
        }

        // Actualizare propriet??i
        existingPersoana.Nume = request.Nume;
        existingPersoana.Prenume = request.Prenume;
        existingPersoana.Judet = request.Judet;
        existingPersoana.Localitate = request.Localitate;
        existingPersoana.Strada = request.Strada;
        existingPersoana.NumarStrada = request.NumarStrada;
        existingPersoana.CodPostal = request.CodPostal;
        existingPersoana.PozitieOrganizatie = request.PozitieOrganizatie;
        existingPersoana.DataNasterii = request.DataNasterii;
        existingPersoana.CNP = request.CNP;
        existingPersoana.TipActIdentitate = request.TipActIdentitate;
        existingPersoana.SerieActIdentitate = request.SerieActIdentitate;
        existingPersoana.NumarActIdentitate = request.NumarActIdentitate;
        existingPersoana.StareCivila = request.StareCivila;
        existingPersoana.Gen = request.Gen;

        var updateResult = await _repository.UpdateAsync(existingPersoana);
        if (!updateResult.IsSuccess)
        {
            return Result.Failure(updateResult.Errors);
        }

        return Result.Success("Persoana a fost actualizat? cu succes");
    }

    public async Task<Result> DeleteAsync(int id)
    {
        // Verificare existen??
        var existingResult = await _repository.GetByIdAsync(id);
        if (!existingResult.IsSuccess)
        {
            return Result.Failure(existingResult.Errors);
        }
        if (existingResult.Value == null)
        {
            return Result.Failure("Persoana nu a fost g?sit?");
        }

        var deleteResult = await _repository.DeleteAsync(id);
        if (!deleteResult.IsSuccess)
        {
            return Result.Failure(deleteResult.Errors);
        }

        return Result.Success("Persoana a fost ?tears? cu succes");
    }

    private static PersoanaDto MapToDto(Persoana persoana)
    {
        var dto = new PersoanaDto
        {
            Id = persoana.Id,
            Nume = persoana.Nume,
            Prenume = persoana.Prenume,
            Judet = persoana.Judet,
            Localitate = persoana.Localitate,
            Strada = persoana.Strada,
            NumarStrada = persoana.NumarStrada,
            CodPostal = persoana.CodPostal,
            PozitieOrganizatie = persoana.PozitieOrganizatie,
            DataNasterii = persoana.DataNasterii,
            DataCreare = persoana.DataCreare,
            DataModificare = persoana.DataModificare,
            CNP = persoana.CNP,
            TipActIdentitate = persoana.TipActIdentitate,
            SerieActIdentitate = persoana.SerieActIdentitate,
            NumarActIdentitate = persoana.NumarActIdentitate,
            StareCivila = persoana.StareCivila,
            Gen = persoana.Gen
        };
        
        // Set computed properties
        dto.NumeComplet = persoana.NumeComplet;
        dto.AdresaCompleta = persoana.AdresaCompleta;
        
        return dto;
    }
}