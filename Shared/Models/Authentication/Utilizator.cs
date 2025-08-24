namespace Shared.Models.Authentication;

public class Utilizator
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public int PersoanaId { get; set; }
    public string NumeUtilizator { get; set; } = string.Empty;
    public string ParolaHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public DateTime? DataCreare { get; set; }
    public DateTime? DataModificare { get; set; }

    // Navigation properties
    public Persoana? Persoana { get; set; }

    public string NumeComplet => Persoana?.NumeComplet ?? string.Empty;
}