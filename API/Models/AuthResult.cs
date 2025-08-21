namespace API.Models
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UtilizatorDb User { get; set; }
        public string NumeComplet { get; set; }
        public string Token { get; set; }
    }
}
