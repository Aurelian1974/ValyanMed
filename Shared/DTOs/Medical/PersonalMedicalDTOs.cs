using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Medical;

// =============================================
// PERSONAL MEDICAL DTOs - ACTUALIZAT FARA DIACRITICE
// =============================================

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
    
    // Noi proprietati pentru ierarhie
    public Guid? CategorieID { get; set; }
    public Guid? SpecializareID { get; set; }
    public Guid? SubspecializareID { get; set; }
    public string? CategorieNume { get; set; }
    public string? SpecializareNume { get; set; }
    public string? SubspecializareNume { get; set; }

    // Computed properties
    public string NumeComplet => $"{Nume} {Prenume}";
    public string StatusText => EsteActiv ? "Activ" : "Inactiv";
    public string SpecializareCompleta => SpecializareNume ?? Specializare ?? "Nu este specificata";
    public string DepartamentAfisare => CategorieNume ?? Departament ?? "Nu este specificat";
}

public class PersonalMedicalDetailDto
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
    
    // Noi proprietati pentru ierarhie
    public Guid? CategorieID { get; set; }
    public Guid? SpecializareID { get; set; }
    public Guid? SubspecializareID { get; set; }
    public string? CategorieNume { get; set; }
    public string? SpecializareNume { get; set; }
    public string? SubspecializareNume { get; set; }

    // Data aditionale pentru pagina de detalii
    public IEnumerable<ProgramareListDto> ProgramariRecente { get; set; } = new List<ProgramareListDto>();
    public IEnumerable<ConsultatieListDto> ConsultatiiRecente { get; set; } = new List<ConsultatieListDto>();
    public int TotalProgramari { get; set; }
    public int TotalConsultatii { get; set; }

    // Computed properties
    public string NumeComplet => $"{Nume} {Prenume}";
    public string StatusText => EsteActiv ? "Activ" : "Inactiv";
    public string CalieCompletaDepartament
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(CategorieNume)) parts.Add(CategorieNume);
            if (!string.IsNullOrEmpty(SpecializareNume)) parts.Add(SpecializareNume);
            if (!string.IsNullOrEmpty(SubspecializareNume)) parts.Add(SubspecializareNume);
            return parts.Any() ? string.Join(" > ", parts) : (Departament ?? "Nu este specificat");
        }
    }
    public string DepartamentAfisare => CategorieNume ?? Departament ?? "Nu este specificat";
}

public class CreatePersonalMedicalRequest
{
    [Required(ErrorMessage = "Numele este obligatoriu")]
    [StringLength(100, ErrorMessage = "Numele nu poate avea mai mult de 100 de caractere")]
    public string Nume { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Prenumele este obligatoriu")]
    [StringLength(100, ErrorMessage = "Prenumele nu poate avea mai mult de 100 de caractere")]
    public string Prenume { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Pozitia este obligatorie")]
    public string Pozitie { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Specializarea este obligatorie")]
    [StringLength(100, ErrorMessage = "Specializarea nu poate avea mai mult de 100 de caractere")]
    public string Specializare { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Numarul de licenta este obligatoriu")]
    [StringLength(50, ErrorMessage = "Numarul de licenta nu poate avea mai mult de 50 de caractere")]
    public string NumarLicenta { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Telefonul este obligatoriu")]
    [Phone(ErrorMessage = "Numarul de telefon nu este valid")]
    public string Telefon { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email-ul este obligatoriu")]
    [EmailAddress(ErrorMessage = "Adresa de email nu este valida")]
    public string Email { get; set; } = string.Empty;
    
    public string? Departament { get; set; }
    public bool EsteActiv { get; set; } = true;
    
    // Noi proprietati pentru ierarhie
    public Guid? CategorieID { get; set; }
    public Guid? SpecializareID { get; set; }
    public Guid? SubspecializareID { get; set; }
}

public class UpdatePersonalMedicalRequest
{
    [Required]
    public Guid PersonalID { get; set; }
    
    [Required(ErrorMessage = "Numele este obligatoriu")]
    [StringLength(100, ErrorMessage = "Numele nu poate avea mai mult de 100 de caractere")]
    public string Nume { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Prenumele este obligatoriu")]
    [StringLength(100, ErrorMessage = "Prenumele nu poate avea mai mult de 100 de caractere")]
    public string Prenume { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Pozitia este obligatorie")]
    public string Pozitie { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Specializarea este obligatorie")]
    [StringLength(100, ErrorMessage = "Specializarea nu poate avea mai mult de 100 de caractere")]
    public string Specializare { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Numarul de licenta este obligatoriu")]
    [StringLength(50, ErrorMessage = "Numarul de licenta nu poate avea mai mult de 50 de caractere")]
    public string NumarLicenta { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Telefonul este obligatoriu")]
    [Phone(ErrorMessage = "Numarul de telefon nu este valid")]
    public string Telefon { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email-ul este obligatoriu")]
    [EmailAddress(ErrorMessage = "Adresa de email nu este valida")]
    public string Email { get; set; } = string.Empty;
    
    public string? Departament { get; set; }
    public bool EsteActiv { get; set; } = true;
    
    // Noi proprietati pentru ierarhie
    public Guid? CategorieID { get; set; }
    public Guid? SpecializareID { get; set; }
    public Guid? SubspecializareID { get; set; }
}

public class PersonalMedicalSearchQuery
{
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
    
    // Multi-select filters pentru Advanced filtering (din PersonalMedicalApiService)
    public IEnumerable<string>? Specializari { get; set; }
    public IEnumerable<string>? Departamente { get; set; }
    public IEnumerable<string>? Pozitii { get; set; }
    
    // Noi proprietati pentru ierarhie
    public Guid? CategorieID { get; set; }
    public Guid? SpecializareID { get; set; }
    public Guid? SubspecializareID { get; set; }
    
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Sort { get; set; }
}

// =============================================
// DEPARTAMENTE DTOs - NOI
// =============================================

public class DepartamentDto
{
    public Guid DepartamentID { get; set; }
    public string Nume { get; set; } = string.Empty;
    public string Tip { get; set; } = string.Empty; // Categorie, Specialitate, Subspecialitate
}

public class DepartamentOptionDto
{
    public Guid Value { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Tip { get; set; } = string.Empty;
    public bool EsteActiv { get; set; } = true;
}

public class DepartamentIerarhicDto
{
    public Guid DepartamentID { get; set; }
    public string Nume { get; set; } = string.Empty;
    public string Tip { get; set; } = string.Empty;
    public Guid? ParenteID { get; set; }
    public int Nivel { get; set; }
    public string CalieCompleta { get; set; } = string.Empty;
    public IEnumerable<DepartamentIerarhicDto> Copii { get; set; } = new List<DepartamentIerarhicDto>();
}

// =============================================
// PROGRAMARI DTOs
// =============================================

public class ProgramareListDto
{
    public Guid ProgramareID { get; set; }
    public Guid PacientID { get; set; }
    public Guid DoctorID { get; set; }
    public DateTime DataProgramare { get; set; }
    public string? TipProgramare { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Observatii { get; set; }
    public DateTime DataCreare { get; set; }
    public string? NumePacient { get; set; }
    public string? CNPPacient { get; set; }
    public string? TelefonPacient { get; set; }
    public string? NumeDoctor { get; set; }
    public string? SpecializareDoctor { get; set; }
}

// =============================================
// CONSULTATII DTOs
// =============================================

public class ConsultatieListDto
{
    public Guid ConsultatieID { get; set; }
    public Guid ProgramareID { get; set; }
    public string? PlangereaPrincipala { get; set; }
    public string? Evaluare { get; set; }
    public DateTime DataConsultatie { get; set; }
    public int? Durata { get; set; }
    public DateTime? DataProgramare { get; set; }
    public string? TipProgramare { get; set; }
    public string? NumeDoctor { get; set; }
    public string? NumePacient { get; set; }
}

// =============================================
// DATA GRID DTOs
// =============================================

public class PersonalMedicalDataGridRequest
{
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
    
    // Noi proprietati pentru ierarhie
    public Guid? CategorieID { get; set; }
    public Guid? SpecializareID { get; set; }
    public Guid? SubspecializareID { get; set; }
    
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 25;
    public string? Sort { get; set; }
    public IEnumerable<DataGridGroupRequest>? Groups { get; set; }
}

public class DataGridGroupRequest
{
    public string Property { get; set; } = string.Empty;
    public string SortOrder { get; set; } = "asc";
}

public class PersonalMedicalDataGridResult
{
    public IEnumerable<PersonalMedicalListDto> Data { get; set; } = new List<PersonalMedicalListDto>();
    public int Count { get; set; }
    public int TotalItems { get; set; }
    public int TotalItemsOverall { get; set; }
    public bool IsGrouped { get; set; }
    public IEnumerable<DataGridGroupResult<PersonalMedicalListDto>> Groups { get; set; } = new List<DataGridGroupResult<PersonalMedicalListDto>>();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalGroups { get; set; }
}

public class DataGridGroupResult<T>
{
    public object Key { get; set; } = null!;
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int Count { get; set; }
    public bool IsExpanded { get; set; } = true;
}

public class PersonalMedicalGroupAggregateQuery
{
    public string GroupBy { get; set; } = "Departament";
    public string? Search { get; set; }
    public string? Departament { get; set; }
    public string? Pozitie { get; set; }
    public bool? EsteActiv { get; set; }
    
    // Proprietati din PersonalMedicalApiService care lipseau
    public string? Nume { get; set; }
    public string? Prenume { get; set; }
    public string? Specializare { get; set; }
    public string? NumarLicenta { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    
    public Guid? CategorieID { get; set; }
    public Guid? SpecializareID { get; set; }
    public Guid? SubspecializareID { get; set; }
}

public class PersonalMedicalGroupAggregateDto
{
    public string Key { get; set; } = string.Empty;
    public int Count { get; set; }
    public DateTime? LastDataCreare { get; set; }
}