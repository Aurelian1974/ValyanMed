using System;

namespace Shared.DTOs
{
    public class UtilizatorDTO
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public int PersoanaId { get; set; }
        public string NumeUtilizator { get; set; }
        public string Email { get; set; }
        public string Telefon { get; set; }
        public DateTime? DataCreare { get; set; }
        public DateTime? DataModificare { get; set; }
        
        // Navigation property for display purposes
        public string NumeComplet { get; set; }
    }

    public class CreateUtilizatorDTO : IUtilizatorModel
    {
        public int PersoanaId { get; set; }
        public string NumeUtilizator { get; set; }
        public string Parola { get; set; }
        public string Email { get; set; }
        public string Telefon { get; set; }
    }

    public class UpdateUtilizatorDTO : IUtilizatorModel
    {
        public int Id { get; set; }
        public int PersoanaId { get; set; }
        public string NumeUtilizator { get; set; }
        public string Parola { get; set; }
        public string Email { get; set; }
        public string Telefon { get; set; }
    }
}
