using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Medical;

// Request DTOs pentru opera?iuni CRUD - folosim clase normale pentru data binding

public class CreatePacientRequest
{
    [Required(ErrorMessage = "Numele este obligatoriu")]
    [StringLength(100)] 
    public string Nume { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Prenumele este obligatoriu")]
    [StringLength(100)] 
    public string Prenume { get; set; } = string.Empty;
    
    [StringLength(13, MinimumLength = 13)] 
    public string? CNP { get; set; }
    
    [Required(ErrorMessage = "Data na?terii este obligatoriu")]
    public DateTime DataNasterii { get; set; } = DateTime.Today.AddYears(-30);
    
    [Required(ErrorMessage = "Genul este obligatoriu")] 
    public string Gen { get; set; } = string.Empty;
    
    [Phone] 
    public string? Telefon { get; set; }
    
    [EmailAddress] 
    public string? Email { get; set; }
    
    public string? Adresa { get; set; }
    public string? Oras { get; set; }
    public string? Judet { get; set; }
    public string? CodPostal { get; set; }
    public string? NumeContactUrgenta { get; set; }
    
    [Phone] 
    public string? TelefonContactUrgenta { get; set; }
    
    public string? FurnizorAsigurare { get; set; }
    public string? NumarAsigurare { get; set; }
}

public record UpdatePacientRequest(
    [Required] Guid PacientID,
    
    [Required(ErrorMessage = "Numele este obligatoriu")]
    [StringLength(100)] string Nume,
    
    [Required(ErrorMessage = "Prenumele este obligatoriu")]
    [StringLength(100)] string Prenume,
    
    [StringLength(13, MinimumLength = 13)] string? CNP,
    
    [Required(ErrorMessage = "Data na?terii este obligatorie")]
    DateTime DataNasterii,
    
    [Required(ErrorMessage = "Genul este obligatoriu")] 
    string Gen,
    
    [Phone] string? Telefon,
    
    [EmailAddress] string? Email,
    
    string? Adresa,
    string? Oras,
    string? Judet,
    string? CodPostal,
    string? NumeContactUrgenta,
    
    [Phone] string? TelefonContactUrgenta,
    
    string? FurnizorAsigurare,
    string? NumarAsigurare
);

public record CreateProgramareRequest(
    [Required(ErrorMessage = "Pacientul este obligatoriu")]
    Guid PacientID,
    
    [Required(ErrorMessage = "Doctorul este obligatoriu")]
    Guid DoctorID,
    
    [Required(ErrorMessage = "Data program?rii este obligatorie")]
    DateTime DataProgramare,
    
    string? TipProgramare,
    string? Observatii,
    
    [Required(ErrorMessage = "Utilizatorul care creeaz? programarea este obligatoriu")]
    Guid CreatDe
);

public record CreateTriajRequest(
    [Required(ErrorMessage = "Programarea este obligatorie")]
    Guid ProgramareID,
    
    [Required(ErrorMessage = "Nivelul de triaj este obligatoriu")]
    [Range(1, 5)] int NivelTriaj,
    
    [Required(ErrorMessage = "Plângerea principal? este obligatorie")]
    [StringLength(1000)] string PlangereaPrincipala,
    
    Guid? AsistentTriajID,
    string? Observatii
);

public class CreateSemneVitaleRequest
{
    [Required(ErrorMessage = "Pacientul este obligatoriu")]
    public Guid PacientID { get; set; }
    
    [Range(60, 250)] 
    public int? TensiuneArterialaMax { get; set; }
    
    [Range(40, 150)] 
    public int? TensiuneArterialaMin { get; set; }
    
    [Range(30, 200)] 
    public int? FrecariaCardiaca { get; set; }
    
    [Range(30.0, 45.0)] 
    public decimal? Temperatura { get; set; }
    
    [Range(10.0, 300.0)] 
    public decimal? Greutate { get; set; }
    
    [Range(50, 250)] 
    public int? Inaltime { get; set; }
    
    [Range(5, 60)] 
    public int? FrecariaRespiratorie { get; set; }
    
    [Range(70.0, 100.0)] 
    public decimal? SaturatieOxigen { get; set; }
    
    public string? Observatii { get; set; }
}

public record CreateConsultatieRequest(
    [Required(ErrorMessage = "Programarea este obligatorie")]
    Guid ProgramareID,
    
    string? PlangereaPrincipala,
    string? IstoricBoalaActuala,
    string? ExamenFizic,
    string? Evaluare,
    string? Plan,
    int? Durata
);

// Response DTOs pentru afi?are date

public class PacientListDto
{
    public Guid PacientID { get; set; }
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string? CNP { get; set; }
    public DateTime DataNasterii { get; set; }
    public string Gen { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Oras { get; set; }
    public string? Judet { get; set; }
    public DateTime DataCreare { get; set; }
    
    // Computed properties
    public string NumeComplet => $"{Nume} {Prenume}";
    public int Varsta => DateTime.Now.Year - DataNasterii.Year - (DateTime.Now.DayOfYear < DataNasterii.DayOfYear ? 1 : 0);
}

public class PersonalMedicalListDto
{
    public Guid PersonalID { get; set; }
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string? Specializare { get; set; }
    public string? NumarLicenta { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Departament { get; set; }
    public string Pozitie { get; set; } = string.Empty;
    public bool EsteActiv { get; set; }
    public DateTime DataCreare { get; set; }
    
    // Computed properties
    public string NumeComplet => $"{Nume} {Prenume}";
    public string StatusText => EsteActiv ? "Activ" : "Inactiv";
    public string SpecializareCompleta => !string.IsNullOrEmpty(Specializare) && !string.IsNullOrEmpty(Pozitie) 
        ? $"{Pozitie} - {Specializare}" 
        : Pozitie ?? Specializare ?? "Nu este specificata";
}

public class ProgramareListDto
{
    public ProgramareListDto() { }
    
    public Guid ProgramareID { get; set; }
    public DateTime DataProgramare { get; set; }
    public string? TipProgramare { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Observatii { get; set; }
    public DateTime DataCreare { get; set; }
    
    // Navigation properties
    public string? NumePacient { get; set; }
    public string? CNPPacient { get; set; }
    public string? TelefonPacient { get; set; }
    public string? NumeDoctor { get; set; }
    public string? SpecializareDoctor { get; set; }
}

public class ConsultatieListDto
{
    public ConsultatieListDto() { }
    
    public Guid ConsultatieID { get; set; }
    public string? PlangereaPrincipala { get; set; }
    public DateTime DataConsultatie { get; set; }
    public int? Durata { get; set; }
    
    // Navigation properties
    public DateTime? DataProgramare { get; set; }
    public string? TipProgramare { get; set; }
    public string? NumeDoctor { get; set; }
    public string? SpecializareDoctor { get; set; }
}

public class TriajPacientDto
{
    public Guid TriajID { get; set; }
    public Guid ProgramareID { get; set; }
    public int NivelTriaj { get; set; }
    public string PlangereaPrincipala { get; set; } = string.Empty;
    public DateTime DataTriaj { get; set; }
    public string? Observatii { get; set; }
    
    // Computed properties
    public string NivelTriajText => NivelTriaj switch
    {
        1 => "CRITIC",
        2 => "URGENT", 
        3 => "MODERAT",
        4 => "SC?ZUT",
        5 => "NEURGENT",
        _ => "NECUNOSCUT"
    };
}

public class SemneVitaleDto
{
    public Guid SemneVitaleID { get; set; }
    public Guid PacientID { get; set; }
    public DateTime DataMasurare { get; set; }
    public int? TensiuneArterialaMax { get; set; }
    public int? TensiuneArterialaMin { get; set; }
    public int? FrecariaCardiaca { get; set; }
    public decimal? Temperatura { get; set; }
    public decimal? Greutate { get; set; }
    public int? Inaltime { get; set; }
    public int? FrecariaRespiratorie { get; set; }
    public decimal? SaturatieOxigen { get; set; }
    public string? Observatii { get; set; }
    
    // Computed properties
    public string? TensiuneArteriala => 
        TensiuneArterialaMax.HasValue && TensiuneArterialaMin.HasValue 
            ? $"{TensiuneArterialaMax}/{TensiuneArterialaMin} mmHg" 
            : null;
            
    public decimal? BMI
    {
        get
        {
            if (Greutate.HasValue && Inaltime.HasValue && Inaltime > 0)
            {
                var heightInMeters = Inaltime.Value / 100.0m;
                return Greutate.Value / (heightInMeters * heightInMeters);
            }
            return null;
        }
    }
}

// Query DTOs pentru c?utare ?i filtrare

public class PacientiSearchQuery
{
    public string? Search { get; set; }
    public string? Judet { get; set; }
    public string? Gen { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Sort { get; set; }
}

public class PersonalMedicalSearchQuery
{
    public string? Search { get; set; }
    public string? Departament { get; set; }
    public string? Pozitie { get; set; }
    public bool? EsteActiv { get; set; }

    // Per-column text filters
    public string? Nume { get; set; }
    public string? Prenume { get; set; }
    public string? Specializare { get; set; }
    public string? NumarLicenta { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }

    // Multi-select filters for Advanced filtering (Radzen DropDown Multiple)
    public IEnumerable<string>? Specializari { get; set; }
    public IEnumerable<string>? Departamente { get; set; }
    public IEnumerable<string>? Pozitii { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Sort { get; set; }
}

public class ProgramariSearchQuery
{
    public DateTime? DataStart { get; set; }
    public DateTime? DataEnd { get; set; }
    public Guid? DoctorID { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Sort { get; set; }
}

// DTOs pentru agregari/grupari PersonalMedical
public class PersonalMedicalGroupAggregateQuery
{
    [Required]
    public string GroupBy { get; set; } = nameof(PersonalMedicalListDto.Departament);

    // Aceleasi filtre ca PersonalMedicalSearchQuery (fara paging)
    public string? Search { get; set; }
    public string? Departament { get; set; }
    public string? Pozitie { get; set; }
    public bool? EsteActiv { get; set; }

    public string? Nume { get; set; }
    public string? Prenume { get; set; }
    public string? Specializare { get; set; }
    public string? NumarLicenta { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }

    public IEnumerable<string>? Specializari { get; set; }
    public IEnumerable<string>? Departamente { get; set; }
    public IEnumerable<string>? Pozitii { get; set; }

    public string? Sort { get; set; }
}

public class PersonalMedicalGroupAggregateDto
{
    public string Key { get; set; } = string.Empty; // valoarea grupului (string normalizat)
    public int Count { get; set; }
    public DateTime? LastDataCreare { get; set; }
}

// Dashboard DTOs

public class DashboardStatisticiDto
{
    public int PacientiTotal { get; set; }
    public int ProgramariAzi { get; set; }
    public int ConsultatiiLunaAceasta { get; set; }
    public int PersonalActiv { get; set; }
    public List<ProgramareListDto> ProgramariAstazi { get; set; } = new();
}

public class PersonalMedicalDetailDto : PersonalMedicalListDto
{
    public PersonalMedicalDetailDto()
    {
        ProgramariRecente = new List<ProgramareListDto>();
        ConsultatiiRecente = new List<ConsultatieListDto>();
    }
    
    public List<ProgramareListDto> ProgramariRecente { get; set; } = new();
    public List<ConsultatieListDto> ConsultatiiRecente { get; set; } = new();
    public int TotalProgramari { get; set; }
    public int TotalConsultatii { get; set; }
}

public class CreatePersonalMedicalRequest
{
    [Required(ErrorMessage = "Numele este obligatoriu")]
    [StringLength(100, ErrorMessage = "Numele nu poate avea mai mult de 100 de caractere")]
    public string Nume { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Prenumele este obligatoriu")]
    [StringLength(100, ErrorMessage = "Prenumele nu poate avea mai mult de 100 de caractere")]
    public string Prenume { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "Specializarea nu poate avea mai mult de 100 de caractere")]
    public string? Specializare { get; set; }
    
    [StringLength(50, ErrorMessage = "Numarul de licenta nu poate avea mai mult de 50 de caractere")]
    public string? NumarLicenta { get; set; }
    
    [Phone(ErrorMessage = "Formatul telefonului nu este valid")]
    [StringLength(20, ErrorMessage = "Telefonul nu poate avea mai mult de 20 de caractere")]
    public string? Telefon { get; set; }
    
    [EmailAddress(ErrorMessage = "Formatul email-ului nu este valid")]
    [StringLength(100, ErrorMessage = "Email-ul nu poate avea mai mult de 100 de caractere")]
    public string? Email { get; set; }
    
    [StringLength(100, ErrorMessage = "Departamentul nu poate avea mai mult de 100 de caractere")]
    public string? Departament { get; set; }
    
    [Required(ErrorMessage = "Pozitia este obligatorie")]
    [StringLength(50, ErrorMessage = "Pozitia nu poate avea mai mult de 50 de caractere")]
    public string Pozitie { get; set; } = string.Empty;
    
    public bool EsteActiv { get; set; } = true;
}

public class UpdatePersonalMedicalRequest
{
    public Guid PersonalID { get; set; }
    
    [Required(ErrorMessage = "Numele este obligatoriu")]
    [StringLength(100, ErrorMessage = "Numele nu poate avea mai mult de 100 de caractere")]
    public string Nume { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Prenumele este obligatoriu")]
    [StringLength(100, ErrorMessage = "Prenumele nu poate avea mai mult de 100 de caractere")]
    public string Prenume { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "Specializarea nu poate avea mai mult de 100 de caractere")]
    public string? Specializare { get; set; }
    
    [StringLength(50, ErrorMessage = "Numarul de licenta nu poate avea mai mult de 50 de caractere")]
    public string? NumarLicenta { get; set; }
    
    [Phone(ErrorMessage = "Formatul telefonului nu este valid")]
    [StringLength(20, ErrorMessage = "Telefonul nu poate avea mai mult de 20 de caractere")]
    public string? Telefon { get; set; }
    
    [EmailAddress(ErrorMessage = "Formatul email-ului nu este valid")]
    [StringLength(100, ErrorMessage = "Email-ul nu poate avea mai mult de 100 de caractere")]
    public string? Email { get; set; }
    
    [StringLength(100, ErrorMessage = "Departamentul nu poate avea mai mult de 100 de caractere")]
    public string? Departament { get; set; }
    
    [Required(ErrorMessage = "Pozitia este obligatorie")]
    [StringLength(50, ErrorMessage = "Pozitia nu poate avea mai mult de 50 de caractere")]
    public string Pozitie { get; set; } = string.Empty;
    
    public bool EsteActiv { get; set; }
}

// DTOs pentru DataGrid server-side grouping
public class DataGridGroupRequest
{
    public string Property { get; set; } = string.Empty;
    public string SortOrder { get; set; } = "asc";
}

public class PersonalMedicalDataGridRequest : PersonalMedicalSearchQuery
{
    public List<DataGridGroupRequest> Groups { get; set; } = new();
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 10;
}

public class DataGridGroupResult<T>
{
    public object Key { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<T> Items { get; set; } = new();
    public List<DataGridGroupResult<T>> SubGroups { get; set; } = new();
}

public class PersonalMedicalDataGridResult
{
    public IEnumerable<PersonalMedicalListDto> Data { get; set; } = new List<PersonalMedicalListDto>();
    public int Count { get; set; } // Pentru DataGrid - num?rul de înregistr?ri din grupul curent
    public List<DataGridGroupResult<PersonalMedicalListDto>> Groups { get; set; } = new();
    public bool IsGrouped { get; set; }
    public int TotalItems { get; set; } // Total înregistr?ri din grupul afi?at în pagina curent?
    public int TotalItemsOverall { get; set; } // Total general de înregistr?ri (pentru statistici)
    public int TotalGroups { get; set; } // Total num?rul de grupuri
    public int CurrentPage { get; set; } // Pagina curent? (pentru grupare)
    public int TotalPages { get; set; } // Total pagini (pentru grupare)
}