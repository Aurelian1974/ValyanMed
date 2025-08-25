using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Shared.Enums.Medical;

public enum StatusProgramare
{
    [Display(Name = "Programata")]
    [Description("Programata")]
    Programata = 1,

    [Display(Name = "Confirmata")]
    [Description("Confirmata")]
    Confirmata = 2,

    [Display(Name = "In Asteptare")]
    [Description("In Asteptare")]
    InAsteptare = 3,

    [Display(Name = "In Consultatie")]
    [Description("In Consultatie")]
    InConsultatie = 4,

    [Display(Name = "Finalizata")]
    [Description("Finalizata")]
    Finalizata = 5,

    [Display(Name = "Anulata")]
    [Description("Anulata")]
    Anulata = 6,

    [Display(Name = "Nu s-a prezentat")]
    [Description("Nu s-a prezentat")]
    NuSaPrezentat = 7,

    [Display(Name = "Amanata")]
    [Description("Amanata")]
    Amanata = 8
}

public enum TipProgramare
{
    [Display(Name = "Consultatie")]
    [Description("Consultatie")]
    Consultatie = 1,

    [Display(Name = "Control")]
    [Description("Control")]
    Control = 2,

    [Display(Name = "Investigatie")]
    [Description("Investigatie")]
    Investigatie = 3,

    [Display(Name = "Interventie")]
    [Description("Interventie")]
    Interventie = 4,

    [Display(Name = "Urgenta")]
    [Description("Urgenta")]
    Urgenta = 5,

    [Display(Name = "Preventie")]
    [Description("Preventie")]
    Preventie = 6
}

public enum NivelTriaj
{
    [Display(Name = "Critic")]
    [Description("Critic - Risc vital imediat")]
    Critic = 1,

    [Display(Name = "Urgent")]
    [Description("Urgent - Risc potential vital")]
    Urgent = 2,

    [Display(Name = "Moderat")]
    [Description("Moderat - Necesita evaluare rapida")]
    Moderat = 3,

    [Display(Name = "Scazut")]
    [Description("Scazut - Poate astepta")]
    Scazut = 4,

    [Display(Name = "Neurgent")]
    [Description("Neurgent - Fara risc")]
    Neurgent = 5
}

public enum StatusTest
{
    [Display(Name = "Comandat")]
    [Description("Comandat")]
    Comandat = 1,

    [Display(Name = "Programat")]
    [Description("Programat")]
    Programat = 2,

    [Display(Name = "In Progres")]
    [Description("In Progres")]
    InProgres = 3,

    [Display(Name = "Finalizat")]
    [Description("Finalizat")]
    Finalizat = 4,

    [Display(Name = "Anulat")]
    [Description("Anulat")]
    Anulat = 5,

    [Display(Name = "Retestat")]
    [Description("Retestat")]
    Retestat = 6
}

public enum PrioritateTest
{
    [Display(Name = "STAT")]
    [Description("STAT - Urgent imediat")]
    STAT = 1,

    [Display(Name = "Urgent")]
    [Description("Urgent - In maxim 2 ore")]
    Urgent = 2,

    [Display(Name = "Rutina")]
    [Description("Rutina - Program normal")]
    Rutina = 3,

    [Display(Name = "Planificat")]
    [Description("Planificat - La data specificata")]
    Planificat = 4
}

public enum StatusRezultat
{
    [Display(Name = "Normal")]
    [Description("Normal")]
    Normal = 1,

    [Display(Name = "Anormal")]
    [Description("Anormal")]
    Anormal = 2,

    [Display(Name = "Critic")]
    [Description("Critic")]
    Critic = 3,

    [Display(Name = "Indeterminat")]
    [Description("Indeterminat")]
    Indeterminat = 4
}

public enum CategorieTest
{
    [Display(Name = "Laborator")]
    [Description("Laborator")]
    Laborator = 1,

    [Display(Name = "Imagistica")]
    [Description("Imagistica")]
    Imagistica = 2,

    [Display(Name = "Functional")]
    [Description("Functional")]
    Functional = 3,

    [Display(Name = "Microbiologie")]
    [Description("Microbiologie")]
    Microbiologie = 4,

    [Display(Name = "Anatomie Patologica")]
    [Description("Anatomie Patologica")]
    AnatomiePatologica = 5
}

public enum TipDiagnostic
{
    [Display(Name = "Principal")]
    [Description("Principal")]
    Principal = 1,

    [Display(Name = "Secundar")]
    [Description("Secundar")]
    Secundar = 2,

    [Display(Name = "Diferential")]
    [Description("Diferential")]
    Diferential = 3,

    [Display(Name = "De lucru")]
    [Description("De lucru")]
    DeLucru = 4,

    [Display(Name = "Final")]
    [Description("Final")]
    Final = 5
}

public enum StatusDiagnostic
{
    [Display(Name = "Activ")]
    [Description("Activ")]
    Activ = 1,

    [Display(Name = "Rezolvat")]
    [Description("Rezolvat")]
    Rezolvat = 2,

    [Display(Name = "Cronic")]
    [Description("Cronic")]
    Cronic = 3,

    [Display(Name = "In remisie")]
    [Description("In remisie")]
    InRemisie = 4,

    [Display(Name = "Progresie")]
    [Description("Progresie")]
    Progresie = 5
}

public enum SeveritateDiagnostic
{
    [Display(Name = "Usoara")]
    [Description("Usoara")]
    Usoara = 1,

    [Display(Name = "Moderata")]
    [Description("Moderata")]
    Moderata = 2,

    [Display(Name = "Severa")]
    [Description("Severa")]
    Severa = 3,

    [Display(Name = "Critica")]
    [Description("Critica")]
    Critica = 4
}

public enum FormaMedicament
{
    [Display(Name = "Tableta")]
    [Description("Tableta")]
    Tableta = 1,

    [Display(Name = "Capsula")]
    [Description("Capsula")]
    Capsula = 2,

    [Display(Name = "Sirop")]
    [Description("Sirop")]
    Sirop = 3,

    [Display(Name = "Injectabil")]
    [Description("Injectabil")]
    Injectabil = 4,

    [Display(Name = "Crema")]
    [Description("Crema")]
    Crema = 5,

    [Display(Name = "Gel")]
    [Description("Gel")]
    Gel = 6,

    [Display(Name = "Spray")]
    [Description("Spray")]
    Spray = 7,

    [Display(Name = "Picaturi")]
    [Description("Picaturi")]
    Picaturi = 8,

    [Display(Name = "Supozitor")]
    [Description("Supozitor")]
    Supozitor = 9,

    [Display(Name = "Plasture")]
    [Description("Plasture")]
    Plasture = 10
}

public enum RolSistemMedical
{
    [Display(Name = "Administrator")]
    [Description("Administrator")]
    Administrator = 1,

    [Display(Name = "Doctor Primar")]
    [Description("Doctor Primar")]
    DoctorPrimar = 2,

    [Display(Name = "Doctor Specialist")]
    [Description("Doctor Specialist")]
    DoctorSpecialist = 3,

    [Display(Name = "Asistent Medical")]
    [Description("Asistent Medical")]
    AsistentMedical = 4,

    [Display(Name = "Tehnician")]
    [Description("Tehnician")]
    Tehnician = 5,

    [Display(Name = "Receptioner")]
    [Description("Receptioner")]
    Receptioner = 6,

    [Display(Name = "Manager Clinic")]
    [Description("Manager Clinic")]
    ManagerClinic = 7,

    [Display(Name = "Farmacist")]
    [Description("Farmacist")]
    Farmacist = 8
}