using Shared.Enums;

namespace Shared.DTOs.Authentication;

public record CreatePersoanaRequest(
    string Nume,
    string Prenume,
    string? Judet = null,
    string? Localitate = null,
    string? Strada = null,
    string? NumarStrada = null,
    string? CodPostal = null,
    string? PozitieOrganizatie = null,
    DateTime? DataNasterii = null,
    string? CNP = null,
    TipActIdentitate? TipActIdentitate = null,
    string? SerieActIdentitate = null,
    string? NumarActIdentitate = null,
    StareCivila? StareCivila = null,
    Gen? Gen = null
);

public record UpdatePersoanaRequest(
    int Id,
    string Nume,
    string Prenume,
    string? Judet = null,
    string? Localitate = null,
    string? Strada = null,
    string? NumarStrada = null,
    string? CodPostal = null,
    string? PozitieOrganizatie = null,
    DateTime? DataNasterii = null,
    string? CNP = null,
    TipActIdentitate? TipActIdentitate = null,
    string? SerieActIdentitate = null,
    string? NumarActIdentitate = null,
    StareCivila? StareCivila = null,
    Gen? Gen = null
);