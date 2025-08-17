using System;

namespace Shared.DTOs
{
    public class MedicamentDTO
    {
        public int MedicamentID { get; set; }
        public Guid MedicamentGUID { get; set; }
        public string Nume { get; set; }
        public string DenumireComunaInternationala { get; set; }
        public string Concentratie { get; set; }
        public string FormaFarmaceutica { get; set; }
        public string Producator { get; set; }
        public string CodATC { get; set; }
        public string Status { get; set; }
        public DateTime DataInregistrare { get; set; }
        public string NumarAutorizatie { get; set; }
        public DateTime? DataAutorizatie { get; set; }
        public DateTime DataExpirare { get; set; }
        public string Ambalaj { get; set; }
        public string Prospect { get; set; }
        public string Contraindicatii { get; set; }
        public string Interactiuni { get; set; }
        public decimal? Pret { get; set; }
        public decimal? PretProducator { get; set; }
        public decimal? TVA { get; set; }
        public bool? Compensat { get; set; }
        public bool? PrescriptieMedicala { get; set; }
        public int Stoc { get; set; }
        public int StocSiguranta { get; set; }
        public DateTime? DataActualizare { get; set; }
        public string UtilizatorActualizare { get; set; }
        public string Observatii { get; set; }
        public bool Activ { get; set; }
    }

    public class CreateMedicamentDTO
    {
        public string Nume { get; set; }
        public string DenumireComunaInternationala { get; set; }
        public string Concentratie { get; set; }
        public string FormaFarmaceutica { get; set; }
        public string Producator { get; set; }
        public string CodATC { get; set; }
        public string Status { get; set; }
        public string NumarAutorizatie { get; set; }
        public DateTime? DataAutorizatie { get; set; }
        public DateTime DataExpirare { get; set; }
        public string Ambalaj { get; set; }
        public string Prospect { get; set; }
        public string Contraindicatii { get; set; }
        public string Interactiuni { get; set; }
        public decimal? Pret { get; set; }
        public decimal? PretProducator { get; set; }
        public decimal? TVA { get; set; } = 19m;
        public bool? Compensat { get; set; } = false;
        public bool? PrescriptieMedicala { get; set; } = false;
        public int Stoc { get; set; } = 0;
        public int StocSiguranta { get; set; } = 0;
        public string UtilizatorActualizare { get; set; }
        public string Observatii { get; set; }
        public bool Activ { get; set; } = true;
    }

    public class UpdateMedicamentDTO : CreateMedicamentDTO
    {
        public int MedicamentID { get; set; }
    }
}
