using System.ComponentModel.DataAnnotations;

namespace Shared.Enums.Medical;

public enum PozitiePersonal
{
    [Display(Name = "Doctor")] Doctor = 1,
    [Display(Name = "Medic specialist")] MedicSpecialist = 2,
    [Display(Name = "Asistent medical")] AsistentMedical = 3,
    [Display(Name = "Tehnician medical")] TehnicianMedical = 4,
    [Display(Name = "Kinetoterapeut")] Kinetoterapeut = 5,
    [Display(Name = "Psiholog")] Psiholog = 6,
    [Display(Name = "Nutritionist")] Nutritionist = 7,
    [Display(Name = "Doctor Primar")] DoctorPrimar = 8,
    [Display(Name = "Doctor Specialist")] DoctorSpecialist = 9,
    [Display(Name = "Asistent Medical")] Asistent_Medical = 10,
    [Display(Name = "Receptioner Principal")] ReceptionerPrincipal = 11
}
