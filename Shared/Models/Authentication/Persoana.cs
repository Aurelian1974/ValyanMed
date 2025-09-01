using Shared.Enums;

namespace Shared.Models.Authentication;

public class Persoana
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string? Judet { get; set; }
    public string? Localitate { get; set; }
    public string? Strada { get; set; }
    public string? NumarStrada { get; set; }
    public string? CodPostal { get; set; }
    public string? PozitieOrganizatie { get; set; }
    public DateTime? DataNasterii { get; set; }
    public DateTime? DataCreare { get; set; }
    public DateTime? DataModificare { get; set; }
    public string? CNP { get; set; }
    public TipActIdentitate? TipActIdentitate { get; set; }
    public string? SerieActIdentitate { get; set; }
    public string? NumarActIdentitate { get; set; }
    public StareCivila? StareCivila { get; set; }
    public Gen? Gen { get; set; }

    // New fields for UI compatibility
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public bool EsteActiva { get; set; } = true;

    public string NumeComplet => $"{Nume} {Prenume}";
    
    public string AdresaCompleta
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(Strada)) parts.Add(Strada);
            if (!string.IsNullOrEmpty(NumarStrada)) parts.Add($"nr. {NumarStrada}");
            if (!string.IsNullOrEmpty(Localitate)) parts.Add(Localitate);
            if (!string.IsNullOrEmpty(Judet)) parts.Add($"jud. {Judet}");
            return string.Join(", ", parts);
        }
    }
}