using Shared.Common;
using Shared.Models.Authentication;
using Shared.DTOs.Authentication;
using Application.Services.Authentication;
using FluentValidation;

namespace Application.Services.Authentication;

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

        // Verificare duplicat nume utilizator
        var existsUsernameResult = await _utilizatorRepository.ExistsByUsernameAsync(request.NumeUtilizator);
        if (!existsUsernameResult.IsSuccess)
        {
            return Result<int>.Failure(existsUsernameResult.Errors);
        }
        if (existsUsernameResult.Value)
        {
            return Result<int>.Failure("Numele de utilizator exist? deja");
        }

        // Verificare duplicat email
        var existsEmailResult = await _utilizatorRepository.ExistsByEmailAsync(request.Email);
        if (!existsEmailResult.IsSuccess)
        {
            return Result<int>.Failure(existsEmailResult.Errors);
        }
        if (existsEmailResult.Value)
        {
            return Result<int>.Failure("Email-ul exist? deja");
        }

        // Verificare existen?? persoan?
        var persoanaResult = await _persoanaRepository.GetByIdAsync(request.PersoanaId);
        if (!persoanaResult.IsSuccess)
        {
            return Result<int>.Failure(persoanaResult.Errors);
        }
        if (persoanaResult.Value == null)
        {
            return Result<int>.Failure("Persoana specificat? nu exist?");
        }

        // Creare utilizator
        var utilizator = new Utilizator
        {
            PersoanaId = request.PersoanaId,
            NumeUtilizator = request.NumeUtilizator,
            ParolaHash = _passwordService.HashPassword(request.Parola),
            Email = request.Email,
            Telefon = request.Telefon
        };

        var createResult = await _utilizatorRepository.CreateAsync(utilizator);
        if (!createResult.IsSuccess)
        {
            return Result<int>.Failure(createResult.Errors);
        }

        return Result<int>.Success(createResult.Value, "Utilizatorul a fost creat cu succes");
    }

    public async Task<Result<UtilizatorDto?>> GetByIdAsync(int id)
    {
        var result = await _utilizatorRepository.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            return Result<UtilizatorDto?>.Failure(result.Errors);
        }

        if (result.Value == null)
        {
            return Result<UtilizatorDto?>.Success(null);
        }

        var dto = MapToDto(result.Value);
        return Result<UtilizatorDto?>.Success(dto);
    }

    public async Task<Result<IEnumerable<UtilizatorDto>>> GetAllAsync()
    {
        var result = await _utilizatorRepository.GetAllAsync();
        if (!result.IsSuccess)
        {
            return Result<IEnumerable<UtilizatorDto>>.Failure(result.Errors);
        }

        var dtos = result.Value?.Select(MapToDto) ?? Enumerable.Empty<UtilizatorDto>();
        return Result<IEnumerable<UtilizatorDto>>.Success(dtos);
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
            return Result.Failure("Utilizatorul nu a fost gasit");
        }

        var existingUtilizator = existingResult.Value;

        // Verificare duplicat nume utilizator (exclusiv current)
        var existsUsernameResult = await _utilizatorRepository.ExistsByUsernameAsync(request.NumeUtilizator, request.Id);
        if (!existsUsernameResult.IsSuccess)
        {
            return Result.Failure(existsUsernameResult.Errors);
        }
        if (existsUsernameResult.Value)
        {
            return Result.Failure("Numele de utilizator existe deja");
        }

        // Verificare duplicat email (exclusiv current)
        var existsEmailResult = await _utilizatorRepository.ExistsByEmailAsync(request.Email, request.Id);
        if (!existsEmailResult.IsSuccess)
        {
            return Result.Failure(existsEmailResult.Errors);
        }
        if (existsEmailResult.Value)
        {
            return Result.Failure("Email-ul exista deja");
        }

        // Verificare existenta persoana
        var persoanaResult = await _persoanaRepository.GetByIdAsync(request.PersoanaId);
        if (!persoanaResult.IsSuccess)
        {
            return Result.Failure(persoanaResult.Errors);
        }
        if (persoanaResult.Value == null)
        {
            return Result.Failure("Persoana specificata nu exista");
        }

        // Actualizare utilizator
        existingUtilizator.PersoanaId = request.PersoanaId;
        existingUtilizator.NumeUtilizator = request.NumeUtilizator;
        existingUtilizator.Email = request.Email;
        existingUtilizator.Telefon = request.Telefon;

        // Actualizare parola doar daca este specificata
        if (!string.IsNullOrWhiteSpace(request.ParolaNoua))
        {
            existingUtilizator.ParolaHash = _passwordService.HashPassword(request.ParolaNoua);
        }

        var updateResult = await _utilizatorRepository.UpdateAsync(existingUtilizator);
        if (!updateResult.IsSuccess)
        {
            return Result.Failure(updateResult.Errors);
        }

        return Result.Success("Utilizatorul a fost actualizat cu succes");
    }

    public async Task<Result> DeleteAsync(int id)
    {
        // Verificare existenta
        var existingResult = await _utilizatorRepository.GetByIdAsync(id);
        if (!existingResult.IsSuccess)
        {
            return Result.Failure(existingResult.Errors);
        }
        if (existingResult.Value == null)
        {
            return Result.Failure("Utilizatorul nu a fost gasit");
        }

        var deleteResult = await _utilizatorRepository.DeleteAsync(id);
        if (!deleteResult.IsSuccess)
        {
            return Result.Failure(deleteResult.Errors);
        }

        return Result.Success("Utilizatorul a fost ?ters cu succes");
    }

    private static UtilizatorDto MapToDto(Utilizator utilizator)
    {
        return new UtilizatorDto
        {
            Id = utilizator.Id,
            PersoanaId = utilizator.PersoanaId,
            NumeUtilizator = utilizator.NumeUtilizator,
            Email = utilizator.Email,
            Telefon = utilizator.Telefon,
            NumeComplet = utilizator.NumeComplet,
            DataCreare = utilizator.DataCreare,
            DataModificare = utilizator.DataModificare
        };
    }
}