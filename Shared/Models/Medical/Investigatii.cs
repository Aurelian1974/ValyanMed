using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Medical;

public class TriajPacient
{
    public Guid TriajID { get; set; }
    
    [Required(ErrorMessage = "Programarea este obligatorie")]
    public Guid ProgramareID { get; set; }
    
    [Required(ErrorMessage = "Nivelul de triaj este obligatoriu")]
    [Range(1, 5, ErrorMessage = "Nivelul de triaj trebuie s? fie între 1 (critic) ?i 5 (neurgent)")]
    public int NivelTriaj { get; set; }
    
    [Required(ErrorMessage = "Plângerea principal? este obligatorie")]
    [StringLength(1000, ErrorMessage = "Plângerea principal? nu poate avea mai mult de 1000 de caractere")]
    public string PlangereaPrincipala { get; set; } = string.Empty;
    
    public Guid? AsistentTriajID { get; set; }
    public DateTime DataTriaj { get; set; }
    public string? Observatii { get; set; }

    // Computed properties
    public string NivelTriajText => NivelTriaj switch
    {
        1 => "Critic",
        2 => "Urgent",
        3 => "Moderat",
        4 => "Sc?zut",
        5 => "Neurgent",
        _ => "Necunoscut"
    };
    
    public string NivelTriajCuloare => NivelTriaj switch
    {
        1 => "error",
        2 => "warning", 
        3 => "info",
        4 => "primary",
        5 => "success",
        _ => "default"
    };
}

public class IstoricMedical
{
    public Guid IstoricID { get; set; }
    
    [Required(ErrorMessage = "Pacientul este obligatoriu")]
    public Guid PacientID { get; set; }
    
    [Required(ErrorMessage = "Afec?iunea este obligatorie")]
    [StringLength(200, ErrorMessage = "Afec?iunea nu poate avea mai mult de 200 de caractere")]
    public string Afectiune { get; set; } = string.Empty;
    
    public DateTime? DataDiagnostic { get; set; }
    public string? Status { get; set; }
    public string? Severitate { get; set; }
    public string? Observatii { get; set; }
    
    public Guid? InregistratDe { get; set; }
    public DateTime DataInregistrare { get; set; }
}

public class TipTest
{
    public Guid TipTestID { get; set; }
    
    [Required(ErrorMessage = "Numele testului este obligatoriu")]
    [StringLength(200, ErrorMessage = "Numele testului nu poate avea mai mult de 200 de caractere")]
    public string NumeTest { get; set; } = string.Empty;
    
    public string? Categorie { get; set; }
    public string? Departament { get; set; }
    public string? IntervalNormal { get; set; }
    public string? UnitateaMasura { get; set; }
    public bool EsteActiv { get; set; } = true;
}

public class ComandaTest
{
    public Guid ComandaID { get; set; }
    
    [Required(ErrorMessage = "Consulta?ia este obligatorie")]
    public Guid ConsultatieID { get; set; }
    
    [Required(ErrorMessage = "Tipul testului este obligatoriu")]
    public Guid TipTestID { get; set; }
    
    public DateTime DataComanda { get; set; }
    public string Status { get; set; } = "Comandat";
    public string Prioritate { get; set; } = "Rutina";
    
    [Required(ErrorMessage = "Comanda trebuie s? fie f?cut? de un membru al personalului medical")]
    public Guid ComantatDe { get; set; }
    
    public string? Observatii { get; set; }

    // Navigation properties (for display)
    public string? NumeTest { get; set; }
    public string? CategorieTest { get; set; }
}

public class RezultatTest
{
    public Guid RezultatID { get; set; }
    
    [Required(ErrorMessage = "Comanda este obligatorie")]
    public Guid ComandaID { get; set; }
    
    public string? Rezultat { get; set; }
    public decimal? ValoareNumerica { get; set; }
    public string? IntervalReferinta { get; set; }
    public string? Status { get; set; }
    
    public DateTime? DataRezultat { get; set; }
    public Guid? EfectuatDe { get; set; }
    public Guid? RevizuitDe { get; set; }
    public DateTime? DataRevizuire { get; set; }
    public string? Observatii { get; set; }

    // Computed properties
    public string StatusCuloare => Status switch
    {
        "Normal" => "success",
        "Anormal" => "warning",
        "Critic" => "error",
        _ => "default"
    };
}