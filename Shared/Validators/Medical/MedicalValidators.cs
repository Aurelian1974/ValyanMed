using FluentValidation;
using Shared.DTOs.Medical;

namespace Shared.Validators.Medical;

public class CreatePacientValidator : AbstractValidator<CreatePacientRequest>
{
    public CreatePacientValidator()
    {
        RuleFor(x => x.Nume)
            .NotEmpty().WithMessage("Numele este obligatoriu")
            .Length(1, 100).WithMessage("Numele trebuie s? aib? între 1 ?i 100 de caractere")
            .Matches(@"^[a-zA-Z?âî???ÂÎ??\s\-']+$").WithMessage("Numele poate con?ine doar litere, spa?ii, cratimi ?i apostrofuri");

        RuleFor(x => x.Prenume)
            .NotEmpty().WithMessage("Prenumele este obligatoriu")
            .Length(1, 100).WithMessage("Prenumele trebuie s? aib? între 1 ?i 100 de caractere")
            .Matches(@"^[a-zA-Z?âî???ÂÎ??\s\-']+$").WithMessage("Prenumele poate con?ine doar litere, spa?ii, cratimi ?i apostrofuri");

        RuleFor(x => x.CNP)
            .Length(13).WithMessage("CNP-ul trebuie s? aib? exact 13 cifre")
            .Matches(@"^\d{13}$").WithMessage("CNP-ul poate con?ine doar cifre")
            .Must(BeValidCNP).WithMessage("CNP-ul nu este valid")
            .When(x => !string.IsNullOrEmpty(x.CNP));

        RuleFor(x => x.DataNasterii)
            .NotEmpty().WithMessage("Data na?terii este obligatorie")
            .LessThan(DateTime.Today).WithMessage("Data na?terii nu poate fi în viitor")
            .GreaterThan(DateTime.Today.AddYears(-150)).WithMessage("Data na?terii nu poate fi mai mult de 150 de ani în urm?");

        RuleFor(x => x.Gen)
            .NotEmpty().WithMessage("Genul este obligatoriu")
            .Must(BeValidGender).WithMessage("Genul trebuie s? fie 'Masculin', 'Feminin' sau 'Neprecizat'");

        RuleFor(x => x.Telefon)
            .Matches(@"^[0-9+\-\s\(\)]{10,15}$").WithMessage("Num?rul de telefon nu este valid")
            .When(x => !string.IsNullOrEmpty(x.Telefon));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Adresa de email nu este valid?")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.TelefonContactUrgenta)
            .Matches(@"^[0-9+\-\s\(\)]{10,15}$").WithMessage("Num?rul de telefon pentru contactul de urgen?? nu este valid")
            .When(x => !string.IsNullOrEmpty(x.TelefonContactUrgenta));
    }

    private static bool BeValidCNP(string? cnp)
    {
        if (string.IsNullOrEmpty(cnp) || cnp.Length != 13 || !cnp.All(char.IsDigit))
            return false;

        // Validare CNP românesc simplificat?
        var weights = new[] { 2, 7, 9, 1, 4, 6, 3, 5, 8, 2, 7, 9 };
        var sum = 0;

        for (int i = 0; i < 12; i++)
        {
            sum += int.Parse(cnp[i].ToString()) * weights[i];
        }

        var checkDigit = sum % 11;
        if (checkDigit == 10) checkDigit = 1;

        return checkDigit == int.Parse(cnp[12].ToString());
    }

    private static bool BeValidGender(string gen)
    {
        return gen == "Masculin" || gen == "Feminin" || gen == "Neprecizat";
    }
}

public class UpdatePacientValidator : AbstractValidator<UpdatePacientRequest>
{
    public UpdatePacientValidator()
    {
        RuleFor(x => x.PacientID)
            .NotEmpty().WithMessage("ID-ul pacientului este obligatoriu");

        RuleFor(x => x.Nume)
            .NotEmpty().WithMessage("Numele este obligatoriu")
            .Length(1, 100).WithMessage("Numele trebuie s? aib? între 1 ?i 100 de caractere")
            .Matches(@"^[a-zA-Z?âî???ÂÎ??\s\-']+$").WithMessage("Numele poate con?ine doar litere, spa?ii, cratimi ?i apostrofuri");

        RuleFor(x => x.Prenume)
            .NotEmpty().WithMessage("Prenumele este obligatoriu")
            .Length(1, 100).WithMessage("Prenumele trebuie s? aib? între 1 ?i 100 de caractere")
            .Matches(@"^[a-zA-Z?âî???ÂÎ??\s\-']+$").WithMessage("Prenumele poate con?ine doar litere, spa?ii, cratimi ?i apostrofuri");

        RuleFor(x => x.CNP)
            .Length(13).WithMessage("CNP-ul trebuie s? aib? exact 13 cifre")
            .Matches(@"^\d{13}$").WithMessage("CNP-ul poate con?ine doar cifre")
            .Must(BeValidCNP).WithMessage("CNP-ul nu este valid")
            .When(x => !string.IsNullOrEmpty(x.CNP));

        RuleFor(x => x.DataNasterii)
            .NotEmpty().WithMessage("Data na?terii este obligatorie")
            .LessThan(DateTime.Today).WithMessage("Data na?terii nu poate fi în viitor")
            .GreaterThan(DateTime.Today.AddYears(-150)).WithMessage("Data na?terii nu poate fi mai mult de 150 de ani în urm?");

        RuleFor(x => x.Gen)
            .NotEmpty().WithMessage("Genul este obligatoriu")
            .Must(BeValidGender).WithMessage("Genul trebuie s? fie 'Masculin', 'Feminin' sau 'Neprecizat'");

        RuleFor(x => x.Telefon)
            .Matches(@"^[0-9+\-\s\(\)]{10,15}$").WithMessage("Num?rul de telefon nu este valid")
            .When(x => !string.IsNullOrEmpty(x.Telefon));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Adresa de email nu este valid?")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.TelefonContactUrgenta)
            .Matches(@"^[0-9+\-\s\(\)]{10,15}$").WithMessage("Num?rul de telefon pentru contactul de urgen?? nu este valid")
            .When(x => !string.IsNullOrEmpty(x.TelefonContactUrgenta));
    }

    private static bool BeValidCNP(string? cnp)
    {
        if (string.IsNullOrEmpty(cnp) || cnp.Length != 13 || !cnp.All(char.IsDigit))
            return false;

        var weights = new[] { 2, 7, 9, 1, 4, 6, 3, 5, 8, 2, 7, 9 };
        var sum = 0;

        for (int i = 0; i < 12; i++)
        {
            sum += int.Parse(cnp[i].ToString()) * weights[i];
        }

        var checkDigit = sum % 11;
        if (checkDigit == 10) checkDigit = 1;

        return checkDigit == int.Parse(cnp[12].ToString());
    }

    private static bool BeValidGender(string gen)
    {
        return gen == "Masculin" || gen == "Feminin" || gen == "Neprecizat";
    }
}

public class CreateProgramareValidator : AbstractValidator<CreateProgramareRequest>
{
    public CreateProgramareValidator()
    {
        RuleFor(x => x.PacientID)
            .NotEmpty().WithMessage("Pacientul este obligatoriu");

        RuleFor(x => x.DoctorID)
            .NotEmpty().WithMessage("Doctorul este obligatoriu");

        RuleFor(x => x.DataProgramare)
            .NotEmpty().WithMessage("Data program?rii este obligatorie")
            .GreaterThan(DateTime.Now.AddMinutes(-30)).WithMessage("Data program?rii nu poate fi în trecut cu mai mult de 30 de minute")
            .LessThan(DateTime.Now.AddYears(2)).WithMessage("Data program?rii nu poate fi mai mult de 2 ani în viitor");

        RuleFor(x => x.TipProgramare)
            .MaximumLength(100).WithMessage("Tipul program?rii nu poate avea mai mult de 100 de caractere");

        RuleFor(x => x.Observatii)
            .MaximumLength(1000).WithMessage("Observa?iile nu pot avea mai mult de 1000 de caractere");

        RuleFor(x => x.CreatDe)
            .NotEmpty().WithMessage("Utilizatorul care creeaz? programarea este obligatoriu");
    }
}

public class CreateSemneVitaleValidator : AbstractValidator<CreateSemneVitaleRequest>
{
    public CreateSemneVitaleValidator()
    {
        RuleFor(x => x.PacientID)
            .NotEmpty().WithMessage("Pacientul este obligatoriu");

        RuleFor(x => x.TensiuneArterialaMax)
            .InclusiveBetween(60, 250).WithMessage("Tensiunea arterial? maxim? trebuie s? fie între 60 ?i 250 mmHg")
            .When(x => x.TensiuneArterialaMax.HasValue);

        RuleFor(x => x.TensiuneArterialaMin)
            .InclusiveBetween(40, 150).WithMessage("Tensiunea arterial? minim? trebuie s? fie între 40 ?i 150 mmHg")
            .When(x => x.TensiuneArterialaMin.HasValue);

        RuleFor(x => x)
            .Must(x => !x.TensiuneArterialaMax.HasValue || !x.TensiuneArterialaMin.HasValue || 
                      x.TensiuneArterialaMax > x.TensiuneArterialaMin)
            .WithMessage("Tensiunea arterial? maxim? trebuie s? fie mai mare decât cea minim?")
            .When(x => x.TensiuneArterialaMax.HasValue && x.TensiuneArterialaMin.HasValue);

        RuleFor(x => x.FrecariaCardiaca)
            .InclusiveBetween(30, 200).WithMessage("Frecven?a cardiac? trebuie s? fie între 30 ?i 200 bpm")
            .When(x => x.FrecariaCardiaca.HasValue);

        RuleFor(x => x.Temperatura)
            .InclusiveBetween(30.0m, 45.0m).WithMessage("Temperatura trebuie s? fie între 30.0 ?i 45.0 °C")
            .When(x => x.Temperatura.HasValue);

        RuleFor(x => x.Greutate)
            .InclusiveBetween(0.5m, 500.0m).WithMessage("Greutatea trebuie s? fie între 0.5 ?i 500.0 kg")
            .When(x => x.Greutate.HasValue);

        RuleFor(x => x.Inaltime)
            .InclusiveBetween(30, 250).WithMessage("În?l?imea trebuie s? fie între 30 ?i 250 cm")
            .When(x => x.Inaltime.HasValue);

        RuleFor(x => x.FrecariaRespiratorie)
            .InclusiveBetween(5, 60).WithMessage("Frecven?a respiratorie trebuie s? fie între 5 ?i 60/min")
            .When(x => x.FrecariaRespiratorie.HasValue);

        RuleFor(x => x.SaturatieOxigen)
            .InclusiveBetween(50.0m, 100.0m).WithMessage("Satura?ia de oxigen trebuie s? fie între 50.0 ?i 100.0%")
            .When(x => x.SaturatieOxigen.HasValue);
    }
}

public class CreateTriajValidator : AbstractValidator<CreateTriajRequest>
{
    public CreateTriajValidator()
    {
        RuleFor(x => x.ProgramareID)
            .NotEmpty().WithMessage("Programarea este obligatorie");

        RuleFor(x => x.NivelTriaj)
            .InclusiveBetween(1, 5).WithMessage("Nivelul de triaj trebuie s? fie între 1 ?i 5");

        RuleFor(x => x.PlangereaPrincipala)
            .NotEmpty().WithMessage("Plângerea principal? este obligatorie")
            .MaximumLength(1000).WithMessage("Plângerea principal? nu poate avea mai mult de 1000 de caractere");

        RuleFor(x => x.Observatii)
            .MaximumLength(2000).WithMessage("Observa?iile nu pot avea mai mult de 2000 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Observatii));
    }
}