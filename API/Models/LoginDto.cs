namespace API.Models
{
    public class LoginDto
    {
        public string NumeUtilizatorSauEmail { get; set; }
        public string Parola { get; set; }
    }
    
    //public class UtilizatorDb
    //{
    //    public int Id { get; set; }
    //    public string NumeUtilizator { get; set; }
    //    public string ParolaHash { get; set; }
    //    public string Email { get; set; }
    //    public string Nume { get; set; }
    //    public string Prenume { get; set; }
    //}
    
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string NumeUtilizator { get; set; }
        public string NumeComplet { get; set; }
        public string Email { get; set; }
    }
}