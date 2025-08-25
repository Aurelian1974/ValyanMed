using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Medical;

public class PersonalMedical
{
    public Guid PersonalID { get; set; }
    
    [Required(ErrorMessage = "Numele este obligatoriu")]
    [StringLength(100, ErrorMessage = "Numele nu poate avea mai mult de 100 de caractere")]
    public string Nume { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Prenumele este obligatoriu")]
    [StringLength(100, ErrorMessage = "Prenumele nu poate avea mai mult de 100 de caractere")]
    public string Prenume { get; set; } = string.Empty;
    
    public string? Specializare { get; set; }
    
    [StringLength(50, ErrorMessage = "Num?rul de licen?? nu poate avea mai mult de 50 de caractere")]
    public string? NumarLicenta { get; set; }
    
    [Phone(ErrorMessage = "Num?rul de telefon nu este valid")]
    public string? Telefon { get; set; }
    
    [EmailAddress(ErrorMessage = "Adresa de email nu este valid?")]
    public string? Email { get; set; }
    
    public string? Departament { get; set; }
    
    [Required(ErrorMessage = "Pozi?ia este obligatorie")]
    public string Pozitie { get; set; } = string.Empty;
    
    public bool EsteActiv { get; set; } = true;
    public DateTime DataCreare { get; set; }

    // Computed properties
    public string NumeComplet => $"{Nume} {Prenume}";
    public string SpecializareCompleta => !string.IsNullOrEmpty(Specializare) ? $"{Specializare}" : Pozitie;
}

public class Programare
{
    public Guid ProgramareID { get; set; }
    
    [Required(ErrorMessage = "Pacientul este obligatoriu")]
    public Guid PacientID { get; set; }
    
    [Required(ErrorMessage = "Doctorul este obligatoriu")]
    public Guid DoctorID { get; set; }
    
    [Required(ErrorMessage = "Data program?rii este obligatorie")]
    public DateTime DataProgramare { get; set; }
    
    public string? TipProgramare { get; set; }
    public string Status { get; set; } = "Programata";
    public string? Observatii { get; set; }
    
    public DateTime DataCreare { get; set; }
    public Guid? CreatDe { get; set; }

    // Navigation properties (for display)
    public string? NumePacient { get; set; }
    public string? CNPPacient { get; set; }
    public string? TelefonPacient { get; set; }
    public string? NumeDoctor { get; set; }
    public string? SpecializareDoctor { get; set; }
}

public class Consultatie
{
    public Guid ConsultatieID { get; set; }
    
    [Required(ErrorMessage = "Programarea este obligatorie")]
    public Guid ProgramareID { get; set; }
    
    public string? PlangereaPrincipala { get; set; }
    public string? IstoricBoalaActuala { get; set; }
    public string? ExamenFizic { get; set; }
    public string? Evaluare { get; set; }
    public string? Plan { get; set; }
    
    public DateTime DataConsultatie { get; set; }
    public int? Durata { get; set; } // în minute

    // Navigation properties (for display)
    public DateTime? DataProgramare { get; set; }
    public string? TipProgramare { get; set; }
    public string? NumeDoctor { get; set; }
    public string? SpecializareDoctor { get; set; }
}

public class SemneVitale
{
    public Guid SemneVitaleID { get; set; }
    
    [Required(ErrorMessage = "Programarea este obligatorie")]
    public Guid ProgramareID { get; set; }
    
    [Range(60, 250, ErrorMessage = "Tensiunea arterial? maxim? trebuie s? fie între 60 ?i 250 mmHg")]
    public int? TensiuneArterialaMax { get; set; }
    
    [Range(40, 150, ErrorMessage = "Tensiunea arterial? minim? trebuie s? fie între 40 ?i 150 mmHg")]
    public int? TensiuneArterialaMin { get; set; }
    
    [Range(30, 200, ErrorMessage = "Frecven?a cardiac? trebuie s? fie între 30 ?i 200 bpm")]
    public int? FrecariaCardiaca { get; set; }
    
    [Range(30.0, 45.0, ErrorMessage = "Temperatura trebuie s? fie între 30.0 ?i 45.0 °C")]
    public decimal? Temperatura { get; set; }
    
    [Range(10.0, 300.0, ErrorMessage = "Greutatea trebuie s? fie între 10.0 ?i 300.0 kg")]
    public decimal? Greutate { get; set; }
    
    [Range(50, 250, ErrorMessage = "În?l?imea trebuie s? fie între 50 ?i 250 cm")]
    public int? Inaltime { get; set; }
    
    [Range(5, 60, ErrorMessage = "Frecven?a respiratorie trebuie s? fie între 5 ?i 60/min")]
    public int? FrecariaRespiratorie { get; set; }
    
    [Range(70.0, 100.0, ErrorMessage = "Satura?ia de oxigen trebuie s? fie între 70.0 ?i 100.0%")]
    public decimal? SaturatieOxigen { get; set; }
    
    public Guid? MasuratDe { get; set; }
    public DateTime DataMasurare { get; set; }

    // Computed properties
    public string TensiuneArteriala => TensiuneArterialaMax.HasValue && TensiuneArterialaMin.HasValue 
        ? $"{TensiuneArterialaMax}/{TensiuneArterialaMin} mmHg" 
        : "";
        
    public decimal? BMI => Greutate.HasValue && Inaltime.HasValue && Inaltime > 0 
        ? Math.Round(Greutate.Value / ((decimal)Inaltime.Value / 100 * (decimal)Inaltime.Value / 100), 2) 
        : null;
}