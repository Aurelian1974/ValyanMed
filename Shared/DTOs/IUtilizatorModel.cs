namespace Shared.DTOs
{
    public interface IUtilizatorModel
    {
        int PersoanaId { get; set; }
        string NumeUtilizator { get; set; }
        string Email { get; set; }
        string Telefon { get; set; }
        string Parola { get; set; }
    }
}