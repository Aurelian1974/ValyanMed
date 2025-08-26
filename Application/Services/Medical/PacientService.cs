using Application.Services.Medical;
using Shared.DTOs.Medical;
using Shared.Common;
using Shared.Exceptions;
using Shared.Utilities;

namespace Application.Services.Medical;

public class PacientService : IPacientService
{
    private readonly IPacientRepository _pacientRepository;

    public PacientService(IPacientRepository pacientRepository)
    {
        _pacientRepository = pacientRepository;
    }

    public async Task<Result<PagedResult<PacientListDto>>> GetPagedAsync(PacientiSearchQuery searchQuery)
    {
        try
        {
            // Validare parametri
            if (searchQuery.Page < 1)
                searchQuery.Page = 1;

            if (searchQuery.PageSize < 1 || searchQuery.PageSize > 1000)
                searchQuery.PageSize = 25;

            var result = await _pacientRepository.GetPagedAsync(searchQuery);
            
            return Result<PagedResult<PacientListDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<PacientListDto>>.Failure("Eroare la incarcarea pacientilor");
        }
    }

    public async Task<Result<PacientListDto?>> GetByIdAsync(Guid pacientId)
    {
        try
        {
            if (pacientId == Guid.Empty)
            {
                return Result<PacientListDto?>.Failure("ID pacient invalid");
            }

            var patient = await _pacientRepository.GetByIdAsync(pacientId);
            
            if (patient == null)
            {
                return Result<PacientListDto?>.Failure("Pacientul nu a fost gasit");
            }

            return Result<PacientListDto?>.Success(patient);
        }
        catch (Exception ex)
        {
            return Result<PacientListDto?>.Failure("Eroare la incarcarea pacientului");
        }
    }

    public async Task<Result<Guid>> CreateAsync(CreatePacientRequest request)
    {
        try
        {
            // Validari business
            var validationErrors = ValidateCreateRequest(request);
            if (validationErrors.Any())
            {
                return Result<Guid>.Failure(validationErrors.ToArray());
            }

            // Verifica CNP duplicat daca este specificat
            if (!string.IsNullOrEmpty(request.CNP))
            {
                var existingPatient = await _pacientRepository.GetByCNPAsync(request.CNP);
                if (existingPatient != null)
                {
                    return Result<Guid>.Failure("Exista deja un pacient cu acest CNP");
                }
            }

            // Verifica email duplicat daca este specificat
            if (!string.IsNullOrEmpty(request.Email))
            {
                var existingPatientByEmail = await _pacientRepository.GetByEmailAsync(request.Email);
                if (existingPatientByEmail != null)
                {
                    return Result<Guid>.Failure("Exista deja un pacient cu aceasta adresa de email");
                }
            }

            var pacientId = await _pacientRepository.CreateAsync(request);
            
            return Result<Guid>.Success(pacientId, "Pacientul a fost creat cu succes");
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure("Eroare la crearea pacientului");
        }
    }

    public async Task<Result<bool>> UpdateAsync(UpdatePacientRequest request)
    {
        try
        {
            // Verifica daca pacientul exista
            var existingPatient = await _pacientRepository.GetByIdAsync(request.PacientID);
            if (existingPatient == null)
            {
                return Result<bool>.Failure("Pacientul nu a fost gasit");
            }

            // Validari business
            var validationErrors = ValidateUpdateRequest(request);
            if (validationErrors.Any())
            {
                return Result<bool>.Failure(validationErrors.ToArray());
            }

            // Verifica CNP duplicat daca este specificat si diferit de cel curent
            if (!string.IsNullOrEmpty(request.CNP) && request.CNP != existingPatient.CNP)
            {
                var duplicatePatient = await _pacientRepository.GetByCNPAsync(request.CNP);
                if (duplicatePatient != null)
                {
                    return Result<bool>.Failure("Exista deja un pacient cu acest CNP");
                }
            }

            // Verifica email duplicat daca este specificat si diferit de cel curent
            if (!string.IsNullOrEmpty(request.Email) && request.Email != existingPatient.Email)
            {
                var duplicatePatientByEmail = await _pacientRepository.GetByEmailAsync(request.Email);
                if (duplicatePatientByEmail != null)
                {
                    return Result<bool>.Failure("Exista deja un pacient cu aceasta adresa de email");
                }
            }

            var success = await _pacientRepository.UpdateAsync(request);
            
            if (success)
            {
                return Result<bool>.Success(true, "Pacientul a fost actualizat cu succes");
            }
            else
            {
                return Result<bool>.Failure("Nu s-a putut actualiza pacientul");
            }
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure("Eroare la actualizarea pacientului");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid pacientId)
    {
        try
        {
            if (pacientId == Guid.Empty)
            {
                return Result<bool>.Failure("ID pacient invalid");
            }

            // Verifica daca pacientul exista
            var existingPatient = await _pacientRepository.GetByIdAsync(pacientId);
            if (existingPatient == null)
            {
                return Result<bool>.Failure("Pacientul nu a fost gasit");
            }

            // Verificari business pentru stergere
            // TODO: Verifica daca pacientul are programari active, consultatii in desfasurare, etc.

            var success = await _pacientRepository.DeleteAsync(pacientId);
            
            if (success)
            {
                return Result<bool>.Success(true, "Pacientul a fost sters cu succes");
            }
            else
            {
                return Result<bool>.Failure("Nu s-a putut sterge pacientul");
            }
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure("Eroare la stergerea pacientului");
        }
    }

    private List<string> ValidateCreateRequest(CreatePacientRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Nume))
            errors.Add("Numele este obligatoriu");

        if (string.IsNullOrWhiteSpace(request.Prenume))
            errors.Add("Prenumele este obligatoriu");

        if (!string.IsNullOrEmpty(request.CNP) && request.CNP.Length != 13)
            errors.Add("CNP-ul trebuie sa aiba 13 cifre");

        if (!string.IsNullOrEmpty(request.Email) && !IsValidEmail(request.Email))
            errors.Add("Adresa de email nu este valida");

        if (request.DataNasterii > DateTime.Today)
            errors.Add("Data nasterii nu poate fi in viitor");

        if (request.DataNasterii < DateTime.Today.AddYears(-150))
            errors.Add("Data nasterii nu este realista");

        return errors;
    }

    private List<string> ValidateUpdateRequest(UpdatePacientRequest request)
    {
        var errors = new List<string>();

        if (request.PacientID == Guid.Empty)
            errors.Add("ID pacient invalid");

        if (string.IsNullOrWhiteSpace(request.Nume))
            errors.Add("Numele este obligatoriu");

        if (string.IsNullOrWhiteSpace(request.Prenume))
            errors.Add("Prenumele este obligatoriu");

        if (!string.IsNullOrEmpty(request.CNP) && request.CNP.Length != 13)
            errors.Add("CNP-ul trebuie sa aiba 13 cifre");

        if (!string.IsNullOrEmpty(request.Email) && !IsValidEmail(request.Email))
            errors.Add("Adresa de email nu este valida");

        return errors;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}