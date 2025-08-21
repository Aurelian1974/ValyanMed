namespace Shared.DTOs
{
    public class DispozitivMedicalDTO
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Denumire { get; set; } = string.Empty;
        public string? Categorie { get; set; }
        public string? ClasaRisc { get; set; }
        public string? Producator { get; set; }
        public string? ModelTip { get; set; }
        public string? NumarSerie { get; set; }
        public bool CertificareCE { get; set; }
        public DateTime? DataExpirare { get; set; }
        public string? Specificatii { get; set; }
        public DateTime DataCreare { get; set; }
        public DateTime DataModificare { get; set; }
    }

    public class CreateDispozitivMedicalDTO
    {
        public string Denumire { get; set; } = string.Empty;
        public string? Categorie { get; set; }
        public string? ClasaRisc { get; set; }
        public string? Producator { get; set; }
        public string? ModelTip { get; set; }
        public string? NumarSerie { get; set; }
        public bool CertificareCE { get; set; } = false;
        public DateTime? DataExpirare { get; set; }
        public string? Specificatii { get; set; }
    }

    public class UpdateDispozitivMedicalDTO
    {
        public int Id { get; set; }
        public string Denumire { get; set; } = string.Empty;
        public string? Categorie { get; set; }
        public string? ClasaRisc { get; set; }
        public string? Producator { get; set; }
        public string? ModelTip { get; set; }
        public string? NumarSerie { get; set; }
        public bool CertificareCE { get; set; }
        public DateTime? DataExpirare { get; set; }
        public string? Specificatii { get; set; }
    }
}