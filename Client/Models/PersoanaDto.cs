namespace Client.Models
{
    public class PersoanaDto
    {
        public int Id { get; set; }
        public string Guid { get; set; }
        public string Nume { get; set; }
        public string Prenume { get; set; }
        public string Judet { get; set; }
        public string Localitate { get; set; }
        public string Strada { get; set; }
        public string Numar { get; set; }
        public string CodPostal { get; set; }

        public override string ToString()
        {
            return $"{Nume} {Prenume}";
        }
    }
}
