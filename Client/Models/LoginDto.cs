namespace ValyanMed.Client.Models
{
    public class LoginDto
    {
        public string NumeUtilizatorSauEmail { get; set; }
        public string Parola { get; set; }
    }
    
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string NumeUtilizator { get; set; }
        public string NumeComplet { get; set; }
        public string Email { get; set; }
    }
}