using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Medical;

public class Pacient
{
    public Guid PacientID { get; set; }
    
    [Required(ErrorMessage = "Numele este obligatoriu")]
    [StringLength(100, ErrorMessage = "Numele nu poate avea mai mult de 100 de caractere")]
    public string Nume { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Prenumele este obligatoriu")]
    [StringLength(100, ErrorMessage = "Prenumele nu poate avea mai mult de 100 de caractere")]
    public string Prenume { get; set; } = string.Empty;
    
    [StringLength(13, MinimumLength = 13, ErrorMessage = "CNP-ul trebuie s? aib? exact 13 caractere")]
    public string? CNP { get; set; }
    
    [Required(ErrorMessage = "Data na?terii este obligatorie")]
    public DateTime DataNasterii { get; set; }
    
    [Required(ErrorMessage = "Genul este obligatoriu")]
    public string Gen { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Num?rul de telefon nu este valid")]
    public string? Telefon { get; set; }
    
    [EmailAddress(ErrorMessage = "Adresa de email nu este valid?")]
    public string? Email { get; set; }
    
    public string? Adresa { get; set; }
    public string? Oras { get; set; }
    public string? Judet { get; set; }
    public string? CodPostal { get; set; }
    
    public string? NumeContactUrgenta { get; set; }
    
    [Phone(ErrorMessage = "Num?rul de telefon pentru contactul de urgen?? nu este valid")]
    public string? TelefonContactUrgenta { get; set; }
    
    public string? FurnizorAsigurare { get; set; }
    public string? NumarAsigurare { get; set; }
    
    public DateTime DataCreare { get; set; }
    public DateTime DataUltimeiModificari { get; set; }
    public bool EsteActiv { get; set; } = true;

    // Computed properties
    public string NumeComplet => $"{Nume} {Prenume}";
    
    public string AdresaCompleta
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(Adresa)) parts.Add(Adresa);
            if (!string.IsNullOrEmpty(Oras)) parts.Add(Oras);
            if (!string.IsNullOrEmpty(Judet)) parts.Add($"jud. {Judet}");
            return string.Join(", ", parts);
        }
    }
    
    public int Varsta => DateTime.Now.Year - DataNasterii.Year - (DateTime.Now.DayOfYear < DataNasterii.DayOfYear ? 1 : 0);
}