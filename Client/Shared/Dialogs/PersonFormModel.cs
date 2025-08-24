using Shared.Enums;

namespace Client.Shared.Dialogs;

public class PersonFormModel
{
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string? Judet { get; set; }
    public string? Localitate { get; set; }
    public string? Strada { get; set; }
    public string? NumarStrada { get; set; }
    public string? CodPostal { get; set; }
    public string? PozitieOrganizatie { get; set; }
    public DateTime? DataNasterii { get; set; }
    public string? CNP { get; set; }
    public TipActIdentitate? TipActIdentitate { get; set; }
    public string? SerieActIdentitate { get; set; }
    public string? NumarActIdentitate { get; set; }
    public StareCivila? StareCivila { get; set; }
    public Gen? Gen { get; set; }
}