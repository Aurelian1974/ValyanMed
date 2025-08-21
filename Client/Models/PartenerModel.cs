using System.ComponentModel.DataAnnotations;

namespace Client.Models
{
    public class PartenerModel
    {
        public int PartenerId { get; set; }
        public Guid PartenerGuid { get; set; } = Guid.NewGuid();
        
        [Required(ErrorMessage = "Codul intern este obligatoriu")]
        [StringLength(50, ErrorMessage = "Codul intern nu poate avea mai mult de 50 de caractere")]
        public string CodIntern { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Denumirea este obligatorie")]
        [StringLength(200, ErrorMessage = "Denumirea nu poate avea mai mult de 200 de caractere")]
        public string Denumire { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "Codul fiscal nu poate avea mai mult de 50 de caractere")]
        public string? CodFiscal { get; set; }
        
        [StringLength(100, ErrorMessage = "Jude?ul nu poate avea mai mult de 100 de caractere")]
        public string? Judet { get; set; }
        
        [StringLength(100, ErrorMessage = "Localitatea nu poate avea mai mult de 100 de caractere")]
        public string? Localitate { get; set; }
        
        [StringLength(500, ErrorMessage = "Adresa nu poate avea mai mult de 500 de caractere")]
        public string? Adresa { get; set; }
        
        public DateTime DataCreare { get; set; } = DateTime.Now;
        public DateTime DataActualizare { get; set; } = DateTime.Now;
        public string? UtilizatorCreare { get; set; }
        public string? UtilizatorActualizare { get; set; }
        public bool Activ { get; set; } = true;
    }
}