using FluentValidation;
using Infrastructure.Repositories.Authentication;
using Infrastructure.Services.Authentication;
using Shared.Common;
using Shared.DTOs.Authentication;
using Shared.Models.Authentication;
using Shared.Validators.Authentication;

namespace Application.Services.Authentication;

public interface IUtilizatorService
{
    Task<Result<int>> CreateAsync(CreateUtilizatorRequest request);
    Task<Result<Utilizator?>> GetByIdAsync(int id);
    Task<Result<IEnumerable<Utilizator>>> GetAllAsync();
    Task<Result> UpdateAsync(UpdateUtilizatorRequest request);
    Task<Result> DeleteAsync(int id);
}

public class UtilizatorService : IUtilizatorService
{
    private readonly IUtilizatorRepository _utilizatorRepository;
    private readonly IPersoanaRepository _persoanaRepository;
    private readonly IPasswordService _passwordService;
    private readonly IValidator<CreateUtilizatorRequest> _createValidator;
    private readonly IValidator<UpdateUtilizatorRequest> _updateValidator;

    public UtilizatorService(
        IUtilizatorRepository utilizatorRepository,
        IPersoanaRepository persoanaRepository,
        IPasswordService passwordService,
        IValidator<CreateUtilizatorRequest> createValidator,
        IValidator<UpdateUtilizatorRequest> updateValidator)
    {
        _utilizatorRepository = utilizatorRepository;
        _persoanaRepository = persoanaRepository;
        _passwordService = passwordService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<int>> CreateAsync(CreateUtilizatorRequest request)
    {
        // Validare
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<int>.Failure(errors);
        }

        // Verificare persoan? existent?
        var persoanaResult = await _persoanaRepository.GetByIdAsync(request.PersoanaId);
        if (!persoanaResult.IsSuccess)
        {
            return Result<int>.Failure(persoanaResult.Errors);
        }

        if (persoanaResult.Value == null)
        {
            return Result<int>.Failure("Persoana selectat? nu exist?");
        }

        // Verificare nume utilizator duplicat
        var usernameExistsResult = await _utilizatorRepository.ExistsByUsernameAsync(request.NumeUtilizator);
        if (!usernameExistsResult.IsSuccess)
        {
            return Result<int>.Failure(usernameExistsResult.Errors);
        }

        if (usernameExistsResult.Value)
        {
            return Result<int>.Failure("Un utilizator cu acest nume exist? deja");
        }

        // Verificare email duplicat
        var emailExistsResult = await _utilizatorRepository.ExistsByEmailAsync(request.Email);
        if (!emailExistsResult.IsSuccess)
        {
            return Result<int>.Failure(emailExistsResult.Errors);
        }

        if (emailExistsResult.Value)
        {
            return Result<int>.Failure("Un utilizator cu acest email exist? deja");
        }

        // Creare utilizator
        var utilizator = new Utilizator
        {
            Guid = Guid.NewGuid(),
            NumeUtilizator = request.NumeUtilizator,
            ParolaHash = _passwordService.HashPassword(request.Parola),
            Email = request.Email,
            Telefon = request.Telefon,
            PersoanaId = request.PersoanaId,
            Persoana = persoanaResult.Value
        };

        return await _utilizatorRepository.CreateAsync(utilizator);
    }

    public async Task<Result<Utilizator?>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Result<Utilizator?>.Failure("ID-ul utilizatorului trebuie s? fie valid");
        }

        return await _utilizatorRepository.GetByIdAsync(id);
    }

    public async Task<Result<IEnumerable<Utilizator>>> GetAllAsync()
    {
        return await _utilizatorRepository.GetAllAsync();
    }

    public async Task<Result> UpdateAsync(UpdateUtilizatorRequest request)
    {
        // Validare
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result.Failure(errors);
        }

        // Verificare existen?? utilizator
        var existingResult = await _utilizatorRepository.GetByIdAsync(request.Id);
        if (!existingResult.IsSuccess)
        {
            return Result.Failure(existingResult.Errors);
        }

        if (existingResult.Value == null)
        {
            return Result.Failure("Utilizatorul nu a fost g?sit");
        }

        // Verificare persoan? existent?
        var persoanaResult = await _persoanaRepository.GetByIdAsync(request.PersoanaId);
        if (!persoanaResult.IsSuccess)
        {
            return Result.Failure(persoanaResult.Errors);
        }

        if (persoanaResult.Value == null)
        {
            return Result.Failure("Persoana selectat? nu exist?");
        }

        // Verificare nume utilizator duplicat (exclusiv pentru acest utilizator)
        var usernameExistsResult = await _utilizatorRepository.ExistsByUsernameAsync(request.NumeUtilizator, request.Id);
        if (!usernameExistsResult.IsSuccess)
        {
            return Result.Failure(usernameExistsResult.Errors);
        }

        if (usernameExistsResult.Value)
        {
            return Result.Failure("Un alt utilizator cu acest nume exist? deja");
        }

        // Verificare email duplicat (exclusiv pentru acest utilizator)
        var emailExistsResult = await _utilizatorRepository.ExistsByEmailAsync(request.Email, request.Id);
        if (!emailExistsResult.IsSuccess)
        {
            return Result.Failure(emailExistsResult.Errors);
        }

        if (emailExistsResult.Value)
        {
            return Result.Failure("Un alt utilizator cu acest email exist? deja");
        }

        // Actualizare utilizator
        var utilizator = existingResult.Value;
        utilizator.NumeUtilizator = request.NumeUtilizator;
        utilizator.Email = request.Email;
        utilizator.Telefon = request.Telefon;
        utilizator.PersoanaId = request.PersoanaId;

        // Actualizare parol? doar dac? este furnizat?
        if (!string.IsNullOrEmpty(request.NovaParola))
        {
            utilizator.ParolaHash = _passwordService.HashPassword(request.NovaParola);
        }

        return await _utilizatorRepository.UpdateAsync(utilizator);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure("ID-ul utilizatorului trebuie s? fie valid");
        }

        // Verificare existen??
        var existingResult = await _utilizatorRepository.GetByIdAsync(id);
        if (!existingResult.IsSuccess)
        {
            return Result.Failure(existingResult.Errors);
        }

        if (existingResult.Value == null)
        {
            return Result.Failure("Utilizatorul nu a fost g?sit");
        }

        // TODO: Verificare dependen?e (sesiuni active, etc.)
        // Aceast? verificare va fi implementat? când vom avea alte entit??i conectate

        return await _utilizatorRepository.DeleteAsync(id);
    }
}