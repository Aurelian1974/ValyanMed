namespace Shared.DTOs.Medical;

public class JudetDto
{
    public int IdJudet { get; set; }
    public Guid JudetGuid { get; set; }
    public string CodJudet { get; set; } = string.Empty;
    public string Nume { get; set; } = string.Empty;
}