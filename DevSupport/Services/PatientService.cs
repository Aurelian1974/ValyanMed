using DevSupport.Interfaces;
using DevSupport.Models;

namespace DevSupport.Services;

/// <summary>
/// Serviciu simplu pentru pacien?i - doar pentru demonstra?ie ?i teste
/// </summary>
public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;

    public PatientService(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
    }

    public async Task<Patient> CreateAsync(Patient patient)
    {
        // Valid?ri simple
        ValidatePatient(patient);
        
        // Verific? dac? CNP-ul exist? deja
        if (await _patientRepository.ExistsAsync(patient.CNP))
        {
            throw new InvalidOperationException($"Un pacient cu CNP-ul {patient.CNP} exist? deja.");
        }

        return await _patientRepository.CreateAsync(patient);
    }

    public async Task<Patient?> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID-ul nu poate fi gol.", nameof(id));

        return await _patientRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Patient>> GetAllAsync()
    {
        return await _patientRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Patient>> SearchAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<Patient>();

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        return await _patientRepository.SearchAsync(searchTerm, page, pageSize);
    }

    public async Task<Patient> UpdateAsync(Guid id, UpdatePatientRequest request)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID-ul nu poate fi gol.", nameof(id));

        var existingPatient = await _patientRepository.GetByIdAsync(id);
        if (existingPatient == null)
            throw new InvalidOperationException($"Pacientul cu ID-ul {id} nu a fost g?sit.");

        // Actualizeaz? doar câmpurile specificate
        if (!string.IsNullOrWhiteSpace(request.FirstName))
            existingPatient.FirstName = request.FirstName;
        
        if (!string.IsNullOrWhiteSpace(request.LastName))
            existingPatient.LastName = request.LastName;
        
        if (!string.IsNullOrWhiteSpace(request.Phone))
            existingPatient.Phone = request.Phone;
        
        if (!string.IsNullOrWhiteSpace(request.Email))
            existingPatient.Email = request.Email;
        
        if (!string.IsNullOrWhiteSpace(request.Address))
            existingPatient.Address = request.Address;

        existingPatient.UpdatedAt = DateTime.UtcNow;

        return await _patientRepository.UpdateAsync(existingPatient);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID-ul nu poate fi gol.", nameof(id));

        return await _patientRepository.DeleteAsync(id);
    }

    private static void ValidatePatient(Patient patient)
    {
        if (patient == null)
            throw new ArgumentNullException(nameof(patient));

        if (string.IsNullOrWhiteSpace(patient.CNP))
            throw new ArgumentException("CNP-ul este obligatoriu.", nameof(patient.CNP));

        if (patient.CNP.Length != 13)
            throw new ArgumentException("CNP-ul trebuie s? aib? exact 13 cifre.", nameof(patient.CNP));

        if (!patient.CNP.All(char.IsDigit))
            throw new ArgumentException("CNP-ul trebuie s? con?in? doar cifre.", nameof(patient.CNP));

        if (string.IsNullOrWhiteSpace(patient.FirstName))
            throw new ArgumentException("Prenumele este obligatoriu.", nameof(patient.FirstName));

        if (string.IsNullOrWhiteSpace(patient.LastName))
            throw new ArgumentException("Numele este obligatoriu.", nameof(patient.LastName));

        if (patient.DateOfBirth == default)
            throw new ArgumentException("Data na?terii este obligatorie.", nameof(patient.DateOfBirth));

        if (patient.DateOfBirth > DateTime.Today)
            throw new ArgumentException("Data na?terii nu poate fi în viitor.", nameof(patient.DateOfBirth));

        if (string.IsNullOrWhiteSpace(patient.Gender))
            throw new ArgumentException("Sexul este obligatoriu.", nameof(patient.Gender));
    }
}