namespace Shared.DTOs.Authentication;

public record CreateUtilizatorRequest(
    string NumeUtilizator,
    string Parola,
    string Email,
    string? Telefon,
    int PersoanaId
);

public record UpdateUtilizatorRequest(
    int Id,
    string NumeUtilizator,
    string Email,
    string? Telefon,
    int PersoanaId,
    string? ParolaNoua = null
);

public record LoginRequest(
    string NumeUtilizatorSauEmail,
    string Parola
);

public record AuthenticationResponse(
    int Id,
    string NumeUtilizator,
    string Email,
    string NumeComplet,
    string Token,
    DateTime Expiration
);

public class UtilizatorDto
{
    public int Id { get; set; }
    public int PersoanaId { get; set; }
    public string NumeUtilizator { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public string NumeComplet { get; set; } = string.Empty;
    public DateTime? DataCreare { get; set; }
    public DateTime? DataModificare { get; set; }
}

public class PersoanaDto
{
    public int Id { get; set; }
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string NumeComplet { get; set; } = string.Empty;
    public string? Judet { get; set; }
    public string? Localitate { get; set; }
    public string? Strada { get; set; }
    public string? NumarStrada { get; set; }
    public string? CodPostal { get; set; }
    public string AdresaCompleta { get; set; } = string.Empty;
    public string? PozitieOrganizatie { get; set; }
    public DateTime? DataNasterii { get; set; }
    public DateTime? DataCreare { get; set; }
    public DateTime? DataModificare { get; set; }
    public string? CNP { get; set; }
    public Shared.Enums.TipActIdentitate? TipActIdentitate { get; set; }
    public string? SerieActIdentitate { get; set; }
    public string? NumarActIdentitate { get; set; }
    public Shared.Enums.StareCivila? StareCivila { get; set; }
    public Shared.Enums.Gen? Gen { get; set; }
}