using System.ComponentModel.DataAnnotations;

namespace Shared.Enums.Medical;

public enum PersonalStatus
{
    [Display(Name = "Activ")] Activ = 1,
    [Display(Name = "Inactiv")] Inactiv = 2
}
