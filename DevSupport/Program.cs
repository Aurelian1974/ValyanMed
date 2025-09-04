using Microsoft.AspNetCore.Mvc;
using DevSupport.Models;
using DevSupport.Interfaces;

namespace DevSupport;

/// <summary>
/// Clas? de startup pentru testele de integrare
/// </summary>
public class TestStartup
{
    public static WebApplication CreateApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services
        builder.Services.AddControllers();
        builder.Services.AddScoped<IPatientService, Services.PatientService>();
        builder.Services.AddScoped<IPatientRepository, MockPatientRepository>();

        var app = builder.Build();

        // Configure pipeline
        app.UseRouting();
        app.MapControllers();

        return app;
    }
}

/// <summary>
/// Controller simplu pentru API-ul de pacien?i - doar pentru teste
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetAll()
    {
        var patients = await _patientService.GetAllAsync();
        var dtos = patients.Select(p => new PatientDto
        {
            Id = p.Id,
            CNP = p.CNP,
            FirstName = p.FirstName,
            LastName = p.LastName,
            DateOfBirth = p.DateOfBirth,
            Gender = p.Gender,
            Phone = p.Phone,
            Email = p.Email,
            Address = p.Address
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PatientDto>> GetById(Guid id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient == null)
            return NotFound();

        var dto = new PatientDto
        {
            Id = patient.Id,
            CNP = patient.CNP,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            Phone = patient.Phone,
            Email = patient.Email,
            Address = patient.Address
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<PatientDto>> Create(CreatePatientRequest request)
    {
        try
        {
            var patient = new Patient
            {
                CNP = request.CNP,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Phone = request.Phone,
                Email = request.Email,
                Address = request.Address
            };

            var created = await _patientService.CreateAsync(patient);
            var dto = new PatientDto
            {
                Id = created.Id,
                CNP = created.CNP,
                FirstName = created.FirstName,
                LastName = created.LastName,
                DateOfBirth = created.DateOfBirth,
                Gender = created.Gender,
                Phone = created.Phone,
                Email = created.Email,
                Address = created.Address
            };

            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PatientDto>> Update(Guid id, UpdatePatientRequest request)
    {
        try
        {
            var updated = await _patientService.UpdateAsync(id, request);
            var dto = new PatientDto
            {
                Id = updated.Id,
                CNP = updated.CNP,
                FirstName = updated.FirstName,
                LastName = updated.LastName,
                DateOfBirth = updated.DateOfBirth,
                Gender = updated.Gender,
                Phone = updated.Phone,
                Email = updated.Email,
                Address = updated.Address
            };

            return Ok(dto);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _patientService.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<PatientDto>>> Search([FromQuery] string searchTerm)
    {
        var patients = await _patientService.SearchAsync(searchTerm);
        var dtos = patients.Select(p => new PatientDto
        {
            Id = p.Id,
            CNP = p.CNP,
            FirstName = p.FirstName,
            LastName = p.LastName,
            DateOfBirth = p.DateOfBirth,
            Gender = p.Gender,
            Phone = p.Phone,
            Email = p.Email,
            Address = p.Address
        });

        return Ok(dtos);
    }
}

/// <summary>
/// Repository mock simplu pentru teste
/// </summary>
public class MockPatientRepository : IPatientRepository
{
    private readonly List<Patient> _patients = new()
    {
        new Patient { Id = Guid.NewGuid(), CNP = "1234567890123", FirstName = "Ion", LastName = "Popescu", DateOfBirth = new DateTime(1990, 1, 1), Gender = "Masculin" },
        new Patient { Id = Guid.NewGuid(), CNP = "2234567890123", FirstName = "Maria", LastName = "Ionescu", DateOfBirth = new DateTime(1992, 5, 15), Gender = "Feminin" }
    };

    public Task<Patient> CreateAsync(Patient patient)
    {
        patient.Id = Guid.NewGuid();
        _patients.Add(patient);
        return Task.FromResult(patient);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var patient = _patients.FirstOrDefault(p => p.Id == id);
        if (patient == null) return Task.FromResult(false);
        
        _patients.Remove(patient);
        return Task.FromResult(true);
    }

    public Task<bool> ExistsAsync(string cnp)
    {
        return Task.FromResult(_patients.Any(p => p.CNP == cnp));
    }

    public Task<IEnumerable<Patient>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Patient>>(_patients);
    }

    public Task<Patient?> GetByCNPAsync(string cnp)
    {
        return Task.FromResult(_patients.FirstOrDefault(p => p.CNP == cnp));
    }

    public Task<Patient?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(_patients.FirstOrDefault(p => p.Id == id));
    }

    public Task<IEnumerable<Patient>> SearchAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        var results = _patients.Where(p => 
            p.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            p.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return Task.FromResult(results);
    }

    public Task<Patient> UpdateAsync(Patient patient)
    {
        var existing = _patients.FirstOrDefault(p => p.Id == patient.Id);
        if (existing != null)
        {
            var index = _patients.IndexOf(existing);
            _patients[index] = patient;
        }
        return Task.FromResult(patient);
    }
}