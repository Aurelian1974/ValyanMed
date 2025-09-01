using Shared.Enums;

namespace Shared.DTOs.Authentication;

public class CreatePersoanaRequest
{
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string? Judet { get; set; }
    public string? Localitate { get; set; }
    public string? Strada { get; set; }
    public string? NumarStrada { get; set; }
    public string? CodPostal { get; set; }
    public string? PozitieOrganizatie { get; set; }
    public DateTime? DataNasterii { get; set; }
    public string? CNP { get; set; }
    public TipActIdentitate? TipActIdentitate { get; set; }
    public string? SerieActIdentitate { get; set; }
    public string? NumarActIdentitate { get; set; }
    public StareCivila? StareCivila { get; set; }
    public Gen? Gen { get; set; }
    
    // Noile propriet??i pentru UI
    public string? Adresa { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public bool EsteActiva { get; set; } = true;
}

public class UpdatePersoanaRequest
{
    public int Id { get; set; }
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string? Judet { get; set; }
    public string? Localitate { get; set; }
    public string? Strada { get; set; }
    public string? NumarStrada { get; set; }
    public string? CodPostal { get; set; }
    public string? PozitieOrganizatie { get; set; }
    public DateTime? DataNasterii { get; set; }
    public string? CNP { get; set; }
    public TipActIdentitate? TipActIdentitate { get; set; }
    public string? SerieActIdentitate { get; set; }
    public string? NumarActIdentitate { get; set; }
    public StareCivila? StareCivila { get; set; }
    public Gen? Gen { get; set; }
    
    // Noile propriet??i pentru UI
    public string? Adresa { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public bool EsteActiva { get; set; } = true;
}

public record PersoanaListDto
{
    public int Id { get; init; }
    public string Nume { get; init; } = string.Empty;
    public string Prenume { get; init; } = string.Empty;
    public string NumeComplet { get; init; } = string.Empty;
    public string? CNP { get; init; }
    public DateTime? DataNasterii { get; init; }
    public int Varsta { get; init; }
    public string? Gen { get; init; }
    public string? Telefon { get; init; }
    public string? Email { get; init; }
    public string? Judet { get; init; }
    public string? Localitate { get; init; }
    public string? Adresa { get; init; }
    public bool EsteActiva { get; init; }
    public string StatusText => EsteActiva ? "Activa" : "Inactiva";
    public DateTime? DataCreare { get; init; }
}

public record PersoanaSearchQuery
{
    public string? Search { get; set; }
    public string? Judet { get; set; }
    public string? Localitate { get; set; }
    public bool? EsteActiv { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Sort { get; set; }
}