using System;

namespace Shared.DTOs
{
    /// <summary>
    /// DTO for transferring complete person data
    /// </summary>
    public class PersoanaDTO
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
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
        
        // Additional UI properties
        public string Specialitate { get; set; }
        public string Departament { get; set; }
        public DateTime DataAngajarii { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// DTO for creating a new person record
    /// </summary>
    public class CreatePersoanaDTO
    {
        public string Nume { get; set; }
        public string Prenume { get; set; }
        public string Judet { get; set; }
        public string Localitate { get; set; }
        public string Strada { get; set; }
        public string NumarStrada { get; set; }
        public string CodPostal { get; set; }
        public string PozitieOrganizatie { get; set; }
        public DateTime? DataNasterii { get; set; }
        public string CNP { get; set; }
        public string TipActIdentitate { get; set; }
        public string SerieActIdentitate { get; set; }
        public string NumarActIdentitate { get; set; }
        public string StareCivila { get; set; }
        public string Gen { get; set; }
        
        // Additional UI properties
        public string Specialitate { get; set; }
        public string Departament { get; set; }
        public DateTime? DataAngajarii { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing person record
    /// </summary>
    public class UpdatePersoanaDTO : CreatePersoanaDTO
    {
        public int Id { get; set; }
    }
}