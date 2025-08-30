namespace Client.Shared.Dialogs;

public class UserFormModel
{
    public string NumeUtilizator { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public int? SelectedPersoanaId { get; set; }
    public string Parola { get; set; } = string.Empty;
    public string? NovaParola { get; set; }
}