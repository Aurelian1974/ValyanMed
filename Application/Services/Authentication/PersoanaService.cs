using FluentValidation;
using Infrastructure.Repositories.Authentication;
using Infrastructure.Services.Authentication;
using Shared.Common;
using Shared.DTOs.Authentication;
using Shared.Exceptions;
using Shared.Models.Authentication;
using Shared.Validators.Authentication;

namespace Application.Services.Authentication;

public interface IPersoanaService
{
    Task<Result<int>> CreateAsync(CreatePersoanaRequest request);
    Task<Result<Persoana?>> GetByIdAsync(int id);
    Task<Result<IEnumerable<Persoana>>> GetAllAsync();
    Task<Result> UpdateAsync(UpdatePersoanaRequest request);
    Task<Result> DeleteAsync(int id);
}

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
            var cnpExistsResult = await _repository.ExistsByCNPAsync(request.CNP);
            if (!cnpExistsResult.IsSuccess)
            {
                return Result<int>.Failure(cnpExistsResult.Errors);
            }

            if (cnpExistsResult.Value)
            {
                return Result<int>.Failure("O persoan? cu acest CNP exist? deja");
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

        return await _repository.CreateAsync(persoana);
    }

    public async Task<Result<Persoana?>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Result<Persoana?>.Failure("ID-ul persoanei trebuie s? fie valid");
        }

        return await _repository.GetByIdAsync(id);
    }

    public async Task<Result<IEnumerable<Persoana>>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
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

        // Verificare CNP duplicat (exclusiv pentru aceast? persoan?)
        if (!string.IsNullOrEmpty(request.CNP))
        {
            var cnpExistsResult = await _repository.ExistsByCNPAsync(request.CNP, request.Id);
            if (!cnpExistsResult.IsSuccess)
            {
                return Result.Failure(cnpExistsResult.Errors);
            }

            if (cnpExistsResult.Value)
            {
                return Result.Failure("O alt? persoan? cu acest CNP exist? deja");
            }
        }

        // Actualizare domain model
        var persoana = existingResult.Value;
        persoana.Nume = request.Nume;
        persoana.Prenume = request.Prenume;
        persoana.Judet = request.Judet;
        persoana.Localitate = request.Localitate;
        persoana.Strada = request.Strada;
        persoana.NumarStrada = request.NumarStrada;
        persoana.CodPostal = request.CodPostal;
        persoana.PozitieOrganizatie = request.PozitieOrganizatie;
        persoana.DataNasterii = request.DataNasterii;
        persoana.CNP = request.CNP;
        persoana.TipActIdentitate = request.TipActIdentitate;
        persoana.SerieActIdentitate = request.SerieActIdentitate;
        persoana.NumarActIdentitate = request.NumarActIdentitate;
        persoana.StareCivila = request.StareCivila;
        persoana.Gen = request.Gen;

        return await _repository.UpdateAsync(persoana);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure("ID-ul persoanei trebuie s? fie valid");
        }

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

        // TODO: Verificare dependen?e (utilizatori, etc.)
        // Aceast? verificare va fi implementat? când vom avea alte entit??i conectate

        return await _repository.DeleteAsync(id);
    }
}