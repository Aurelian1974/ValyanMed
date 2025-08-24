using System.ComponentModel;

namespace Shared.Enums;

public enum TipActIdentitate
{
    [Description("Carte de identitate")]
    CarteIdentitate = 1,

    [Description("Pa?aport")]
    Pasaport = 2,

    [Description("Permis de conducere")]
    PermisConducere = 3,

    [Description("Certificat na?tere")]
    CertificatNastere = 4,

    [Description("Altul")]
    Altul = 5
}