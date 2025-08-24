namespace Shared.DTOs.Authentication;

public record CreateUtilizatorRequest(
    string NumeUtilizator,
    string Parola,
    string Email,
    string? Telefon,
    int PersoanaId
);

public record UpdateUtilizatorRequest(
    int Id,
    string NumeUtilizator,
    string Email,
    string? Telefon,
    int PersoanaId,
    string? NovaParola = null
);

public record LoginRequest(
    string NumeUtilizatorSauEmail,
    string Parola
);

public record AuthenticationResponse(
    int Id,
    string NumeUtilizator,
    string Email,
    string NumeComplet,
    string Token,
    DateTime Expiration
);