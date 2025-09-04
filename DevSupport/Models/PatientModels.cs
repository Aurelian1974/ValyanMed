namespace DevSupport.Models;

/// <summary>
/// Model simplu pentru pacient - folosit doar pentru teste
/// </summary>
public class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CNP { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO pentru transfer de date pacient
/// </summary>
public class PatientDto
{
    public Guid Id { get; set; }
    public string CNP { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}

/// <summary>
/// Request pentru crearea unui pacient nou
/// </summary>
public class CreatePatientRequest
{
    public string CNP { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}

/// <summary>
/// Request pentru actualizarea unui pacient
/// </summary>
public class UpdatePatientRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}