namespace Shared.DTOs.Common;

public class JudetDto
{
    public int IdJudet { get; set; }
    public Guid JudetGuid { get; set; }
    public string CodJudet { get; set; } = string.Empty;
    public string Nume { get; set; } = string.Empty;
    public string? Siruta { get; set; }
    public string? CodAuto { get; set; }
    public int? Ordine { get; set; }
}

public class LocalitateDto
{
    public int IdOras { get; set; }
    public Guid LocalitateGuid { get; set; }
    public int IdJudet { get; set; }
    public string Nume { get; set; } = string.Empty;
    public string? Siruta { get; set; }
    public string? CodPostal { get; set; }
    
    // For UI compatibility
    public string JudetNume { get; set; } = string.Empty;
}

public class JudetWithLocalitatiDto
{
    public int IdJudet { get; set; }
    public Guid JudetGuid { get; set; }
    public string CodJudet { get; set; } = string.Empty;
    public string Nume { get; set; } = string.Empty;
    public string? Siruta { get; set; }
    public string? CodAuto { get; set; }
    public int? Ordine { get; set; }
    public List<LocalitateDto> Localitati { get; set; } = new();
}