namespace Shared.DTOs
{
    public class MaterialSanitarDTO
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Denumire { get; set; } = string.Empty;
        public string? Categorie { get; set; }
        public string? Specificatii { get; set; }
        public string? UnitateaMasura { get; set; }
        public bool Sterile { get; set; }
        public bool UniUzinta { get; set; }
        public DateTime DataCreare { get; set; }
        public DateTime DataModificare { get; set; }
    }

    public class CreateMaterialSanitarDTO
    {
        public string Denumire { get; set; } = string.Empty;
        public string? Categorie { get; set; }
        public string? Specificatii { get; set; }
        public string? UnitateaMasura { get; set; }
        public bool Sterile { get; set; } = false;
        public bool UniUzinta { get; set; } = true;
    }

    public class UpdateMaterialSanitarDTO
    {
        public int Id { get; set; }
        public string Denumire { get; set; } = string.Empty;
        public string? Categorie { get; set; }
        public string? Specificatii { get; set; }
        public string? UnitateaMasura { get; set; }
        public bool Sterile { get; set; }
        public bool UniUzinta { get; set; }
    }
}