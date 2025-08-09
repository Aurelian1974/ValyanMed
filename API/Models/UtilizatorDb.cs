using System;

namespace API.Models
{
    public class UtilizatorDb
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public int PersoanaId { get; set; }
        public string NumeUtilizator { get; set; }
        public string ParolaHash { get; set; }
        public string Email { get; set; }
        public string Telefon { get; set; }
        public DateTime? DataCreare { get; set; }
        public DateTime? DataModificare { get; set; }
        
        // Properties from join with Persoana table
        public string Nume { get; set; }
        public string Prenume { get; set; }
    }
}