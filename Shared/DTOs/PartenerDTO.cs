namespace Shared.DTOs
{
    public class PartenerDTO
    {
        public int PartenerId { get; set; }
        public Guid PartenerGuid { get; set; }
        public string CodIntern { get; set; } = string.Empty;
        public string Denumire { get; set; } = string.Empty;
        public string? CodFiscal { get; set; }
        public string? Judet { get; set; }
        public string? Localitate { get; set; }
        public string? Adresa { get; set; }
        public DateTime DataCreare { get; set; }
        public DateTime DataActualizare { get; set; }
        public string? UtilizatorCreare { get; set; }
        public string? UtilizatorActualizare { get; set; }
        public bool Activ { get; set; }
    }

    public class CreatePartenerDTO
    {
        public string CodIntern { get; set; } = string.Empty;
        public string Denumire { get; set; } = string.Empty;
        public string? CodFiscal { get; set; }
        public string? Judet { get; set; }
        public string? Localitate { get; set; }
        public string? Adresa { get; set; }
        public string? UtilizatorCreare { get; set; }
    }

    public class UpdatePartenerDTO
    {
        public int PartenerId { get; set; }
        public string CodIntern { get; set; } = string.Empty;
        public string Denumire { get; set; } = string.Empty;
        public string? CodFiscal { get; set; }
        public string? Judet { get; set; }
        public string? Localitate { get; set; }
        public string? Adresa { get; set; }
        public string? UtilizatorActualizare { get; set; }
    }
}