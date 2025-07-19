using System;

namespace Client.Models
{
    public class PersoanaModel
    {
        public int Id { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public string Nume { get; set; }
        public string Prenume { get; set; }
        public string Judet { get; set; }
        public string Localitate { get; set; }
        public string Strada { get; set; }
        public string NumarStrada { get; set; }
        public string CodPostal { get; set; }
        public string PozitieOrganizatie { get; set; }
        public DateTime? DataNasterii { get; set; }
        public DateTime? DataCreare { get; set; }
        public DateTime? DataModificare { get; set; }
        public string CNP { get; set; }
        public string TipActIdentitate { get; set; }
        public string SerieActIdentitate { get; set; }
        public string NumarActIdentitate { get; set; }
        public string StareCivila { get; set; }
        public string Gen { get; set; }
        public string Specialitate { get; set; }
        public string Departament { get; set; }
        public DateTime? DataAngajarii { get; set; } = DateTime.Today;
        public string Status { get; set; } = "Activ";
    }
}