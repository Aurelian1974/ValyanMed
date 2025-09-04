using DevSupport.Models;

namespace DevSupport.Interfaces;

/// <summary>
/// Interface pentru repository-ul de pacien?i - doar pentru teste
/// </summary>
public interface IPatientRepository
{
    Task<Patient> CreateAsync(Patient patient);
    Task<Patient?> GetByIdAsync(Guid id);
    Task<Patient?> GetByCNPAsync(string cnp);
    Task<IEnumerable<Patient>> GetAllAsync();
    Task<IEnumerable<Patient>> SearchAsync(string searchTerm, int page = 1, int pageSize = 10);
    Task<Patient> UpdateAsync(Patient patient);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string cnp);
}

/// <summary>
/// Interface pentru serviciul de pacien?i - doar pentru teste
/// </summary>
public interface IPatientService
{
    Task<Patient> CreateAsync(Patient patient);
    Task<Patient?> GetByIdAsync(Guid id);
    Task<IEnumerable<Patient>> GetAllAsync();
    Task<IEnumerable<Patient>> SearchAsync(string searchTerm, int page = 1, int pageSize = 10);
    Task<Patient> UpdateAsync(Guid id, UpdatePatientRequest request);
    Task<bool> DeleteAsync(Guid id);
}