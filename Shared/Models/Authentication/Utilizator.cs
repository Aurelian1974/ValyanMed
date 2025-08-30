namespace Shared.Models.Authentication;

public class Utilizator
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public int PersoanaId { get; set; } // P?strez int pentru compatibilitate cu sistemul existent
    public string NumeUtilizator { get; set; } = string.Empty;
    public string ParolaHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public DateTime? DataCreare { get; set; }
    public DateTime? DataModificare { get; set; }

    // Navigation properties
    public Persoana? Persoana { get; set; }

    // Proprietati pentru personalul medical din UtilizatoriSistem
    public string? Specializare { get; set; }
    public string? CodPersonal { get; set; } // Mapata din NumarLicenta
    public bool EsteActiv { get; set; } = true;

    public string NumeComplet => Persoana?.NumeComplet ?? $"{Persoana?.Nume} {Persoana?.Prenume}".Trim();
    
    // Pentru afisare in UI - specializare completa
    public string SpecializareCompleta => !string.IsNullOrEmpty(Specializare) 
        ? $"{Specializare} - {Persoana?.PozitieOrganizatie}" 
        : Persoana?.PozitieOrganizatie ?? string.Empty;
}