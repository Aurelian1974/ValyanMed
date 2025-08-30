using System.ComponentModel.DataAnnotations;

namespace Shared.Enums.Medical;

public enum Departament
{
    [Display(Name = "Cardiologie")] Cardiologie = 1,
    [Display(Name = "Neurologie")] Neurologie = 2,
    [Display(Name = "Pediatrie")] Pediatrie = 3,
    [Display(Name = "Chirurgie")] Chirurgie = 4,
    [Display(Name = "Medicina interna")] MedicinaInterna = 5,
    [Display(Name = "Radiologie")] Radiologie = 6,
    [Display(Name = "Laborator")] Laborator = 7,
    [Display(Name = "Recuperare")] Recuperare = 8,
    [Display(Name = "Receptie")] Receptie = 9
}
