using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Medical;

public class Diagnostic
{
    public Guid DiagnosticID { get; set; }
    
    [Required(ErrorMessage = "Consulta?ia este obligatorie")]
    public Guid ConsultatieID { get; set; }
    
    [StringLength(10, ErrorMessage = "Codul ICD nu poate avea mai mult de 10 caractere")]
    public string? CodICD { get; set; }
    
    [Required(ErrorMessage = "Descrierea diagnosticului este obligatorie")]
    [StringLength(500, ErrorMessage = "Descrierea diagnosticului nu poate avea mai mult de 500 de caractere")]
    public string DescriereaDiagnosticului { get; set; } = string.Empty;
    
    public string? TipDiagnostic { get; set; }
    public string? Severitate { get; set; }
    public string? Status { get; set; }
    
    public DateTime DataDiagnostic { get; set; }
}

public class MedicamentNou
{
    public Guid MedicamentID { get; set; }
    
    [Required(ErrorMessage = "Numele medicamentului este obligatoriu")]
    [StringLength(200, ErrorMessage = "Numele medicamentului nu poate avea mai mult de 200 de caractere")]
    public string NumeMedicament { get; set; } = string.Empty;
    
    public string? NumeGeneric { get; set; }
    public string? Concentratie { get; set; }
    public string? Forma { get; set; }
    public string? Producator { get; set; }
    public bool EsteActiv { get; set; } = true;

    // Computed property
    public string NumeComplet => !string.IsNullOrEmpty(Concentratie) 
        ? $"{NumeMedicament} {Concentratie}" 
        : NumeMedicament;
}

public class Prescriptie
{
    public Guid PrescriptieID { get; set; }
    
    [Required(ErrorMessage = "Consulta?ia este obligatorie")]
    public Guid ConsultatieID { get; set; }
    
    [Required(ErrorMessage = "Medicamentul este obligatoriu")]
    public Guid MedicamentID { get; set; }
    
    [Required(ErrorMessage = "Doza este obligatorie")]
    [StringLength(100, ErrorMessage = "Doza nu poate avea mai mult de 100 de caractere")]
    public string Doza { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Frecven?a este obligatorie")]
    [StringLength(100, ErrorMessage = "Frecven?a nu poate avea mai mult de 100 de caractere")]
    public string Frecventa { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "Durata nu poate avea mai mult de 100 de caractere")]
    public string? Durata { get; set; }
    
    [Range(1, 1000, ErrorMessage = "Cantitatea trebuie s? fie între 1 ?i 1000")]
    public int? Cantitate { get; set; }
    
    [Range(0, 10, ErrorMessage = "Num?rul de reînnoire trebuie s? fie între 0 ?i 10")]
    public int Reinnoire { get; set; } = 0;
    
    public string? Instructiuni { get; set; }
    
    public DateTime DataPrescriptie { get; set; }
    
    [Required(ErrorMessage = "Prescrip?ia trebuie s? fie f?cut? de un doctor")]
    public Guid PrescrisDe { get; set; }

    // Navigation properties (for display)
    public string? NumeMedicament { get; set; }
    public string? ConcentratieMedicament { get; set; }
    public string? FormaMedicament { get; set; }
}

public class UtilizatorSistem
{
    public Guid UtilizatorID { get; set; }
    
    [Required(ErrorMessage = "Numele de utilizator este obligatoriu")]
    [StringLength(50, ErrorMessage = "Numele de utilizator nu poate avea mai mult de 50 de caractere")]
    public string NumeUtilizator { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Adresa de email este obligatorie")]
    [EmailAddress(ErrorMessage = "Adresa de email nu este valid?")]
    [StringLength(100, ErrorMessage = "Adresa de email nu poate avea mai mult de 100 de caractere")]
    public string Email { get; set; } = string.Empty;
    
    public string HashParola { get; set; } = string.Empty; // Nu va fi expus în UI
    
    public Guid? PersonalID { get; set; }
    public bool EsteActiv { get; set; } = true;
    public DateTime? DataUltimeiAutentificari { get; set; }
    public int IncercariEsuateAutentificare { get; set; } = 0;
    public DateTime? BlocatPanaLa { get; set; }
    public DateTime DataCreare { get; set; }
    public Guid? CreatDe { get; set; }

    // Navigation properties (for display)
    public string? NumeComplet { get; set; }
    public string? Pozitie { get; set; }
    public string? Departament { get; set; }
    public string? Roluri { get; set; }

    // Computed properties
    public bool EsteBlocat => BlocatPanaLa.HasValue && BlocatPanaLa.Value > DateTime.Now;
}

public class RolSistem
{
    public Guid RolID { get; set; }
    
    [Required(ErrorMessage = "Numele rolului este obligatoriu")]
    [StringLength(50, ErrorMessage = "Numele rolului nu poate avea mai mult de 50 de caractere")]
    public string NumeRol { get; set; } = string.Empty;
    
    [StringLength(200, ErrorMessage = "Descrierea nu poate avea mai mult de 200 de caractere")]
    public string? Descriere { get; set; }
    
    public bool EsteActiv { get; set; } = true;
    public DateTime DataCreare { get; set; }
}