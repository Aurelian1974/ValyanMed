using System.ComponentModel;

namespace Shared.Enums;

public enum StareCivila
{
    [Description("Nec?s?torit/?")]
    Necasatorit = 1,

    [Description("C?s?torit/?")]
    Casatorit = 2,

    [Description("Divor?at/?")]
    Divortit = 3,

    [Description("V?duv/?")]
    Vaduv = 4,

    [Description("Concubinaj")]
    Concubinaj = 5
}