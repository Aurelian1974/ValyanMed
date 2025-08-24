using System.ComponentModel;

namespace Shared.Enums;

public enum TipActIdentitate
{
    [Description("Carte de identitate")]
    CI = 1, // Valoare scurt? pentru compatibilitate

    [Description("Carte de identitate")]
    CarteIdentitate = 1, // Alias pentru denumirea complet?

    [Description("Pa?aport")]
    Pasaport = 2,

    [Description("Permis de conducere")]
    Permis = 3, // Valoare scurt? pentru compatibilitate

    [Description("Permis de conducere")]
    PermisConducere = 3, // Alias pentru denumirea complet?

    [Description("Certificat na?tere")]
    Certificat = 4, // Valoare scurt? pentru compatibilitate

    [Description("Certificat na?tere")]
    CertificatNastere = 4, // Alias pentru denumirea complet?

    [Description("Altul")]
    Altul = 5
}