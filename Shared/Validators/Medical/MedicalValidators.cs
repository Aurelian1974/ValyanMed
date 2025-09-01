using FluentValidation;
using Shared.DTOs.Medical;

namespace Shared.Validators.Medical;

public class CreatePacientValidator : AbstractValidator<CreatePacientRequest>
{
    public CreatePacientValidator()
    {
        RuleFor(x => x.Nume)
            .NotEmpty().WithMessage("Numele este obligatoriu")
            .Length(1, 100).WithMessage("Numele trebuie sa aiba intre 1 si 100 de caractere")
            .Matches(@"^[a-zA-Z?גמ???ÂÎ??\s\-']+$").WithMessage("Numele poate contine doar litere, spatii, cratimi si apostrofuri");

        RuleFor(x => x.Prenume)
            .NotEmpty().WithMessage("Prenumele este obligatoriu")
            .Length(1, 100).WithMessage("Prenumele trebuie sa aiba intre 1 si 100 de caractere")
            .Matches(@"^[a-zA-Z?גמ???ÂÎ??\s\-']+$").WithMessage("Prenumele poate contine doar litere, spatii, cratimi si apostrofuri");

        RuleFor(x => x.CNP)
            .Length(13).WithMessage("CNP-ul trebuie sa aiba exact 13 cifre")
            .Matches(@"^\d{13}$").WithMessage("CNP-ul poate contine doar cifre")
            .Must(BeValidCNP).WithMessage("CNP-ul nu este valid")
            .When(x => !string.IsNullOrEmpty(x.CNP));

        RuleFor(x => x.DataNasterii)
            .NotEmpty().WithMessage("Data nasterii este obligatorie")
            .LessThan(DateTime.Today).WithMessage("Data nasterii nu poate fi in viitor")
            .GreaterThan(DateTime.Today.AddYears(-150)).WithMessage("Data nasterii nu poate fi mai mult de 150 de ani in urma");

        RuleFor(x => x.Gen)
            .NotEmpty().WithMessage("Genul este obligatoriu")
            .Must(BeValidGender).WithMessage("Genul trebuie sa fie 'Masculin', 'Feminin' sau 'Neprecizat'");

        RuleFor(x => x.Telefon)
            .Matches(@"^[0-9+\-\s\(\)]{10,15}$").WithMessage("Numarul de telefon nu este valid")
            .When(x => !string.IsNullOrEmpty(x.Telefon));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Adresa de email nu este valida")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.TelefonContactUrgenta)
            .Matches(@"^[0-9+\-\s\(\)]{10,15}$").WithMessage("Numarul de telefon pentru contactul de urgenta nu este valid")
            .When(x => !string.IsNullOrEmpty(x.TelefonContactUrgenta));
    }

    private static bool BeValidCNP(string? cnp)
    {
        if (string.IsNullOrEmpty(cnp) || cnp.Length != 13 || !cnp.All(char.IsDigit))
            return false;

        // Validare CNP romanesc simplificata
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
            .Length(1, 100).WithMessage("Numele trebuie sa aiba intre 1 si 100 de caractere")
            .Matches(@"^[a-zA-Z?גמ???ÂÎ??\s\-']+$").WithMessage("Numele poate contine doar litere, spatii, cratimi si apostrofuri");

        RuleFor(x => x.Prenume)
            .NotEmpty().WithMessage("Prenumele este obligatoriu")
            .Length(1, 100).WithMessage("Prenumele trebuie sa aiba intre 1 si 100 de caractere")
            .Matches(@"^[a-zA-Z?גמ???ÂÎ??\s\-']+$").WithMessage("Prenumele poate contine doar litere, spatii, cratimi si apostrofuri");

        RuleFor(x => x.CNP)
            .Length(13).WithMessage("CNP-ul trebuie sa aiba exact 13 cifre")
            .Matches(@"^\d{13}$").WithMessage("CNP-ul poate contine doar cifre")
            .Must(BeValidCNP).WithMessage("CNP-ul nu este valid")
            .When(x => !string.IsNullOrEmpty(x.CNP));

        RuleFor(x => x.DataNasterii)
            .NotEmpty().WithMessage("Data nasterii este obligatorie")
            .LessThan(DateTime.Today).WithMessage("Data nasterii nu poate fi in viitor")
            .GreaterThan(DateTime.Today.AddYears(-150)).WithMessage("Data nasterii nu poate fi mai mult de 150 de ani in urma");

        RuleFor(x => x.Gen)
            .NotEmpty().WithMessage("Genul este obligatoriu")
            .Must(BeValidGender).WithMessage("Genul trebuie sa fie 'Masculin', 'Feminin' sau 'Neprecizat'");

        RuleFor(x => x.Telefon)
            .Matches(@"^[0-9+\-\s\(\)]{10,15}$").WithMessage("Numarul de telefon nu este valid")
            .When(x => !string.IsNullOrEmpty(x.Telefon));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Adresa de email nu este valida")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.TelefonContactUrgenta)
            .Matches(@"^[0-9+\-\s\(\)]{10,15}$").WithMessage("Numarul de telefon pentru contactul de urgenta nu este valid")
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
            .NotEmpty().WithMessage("Data programarii este obligatorie")
            .GreaterThan(DateTime.Now.AddMinutes(-30)).WithMessage("Data programarii nu poate fi in trecut cu mai mult de 30 de minute")
            .LessThan(DateTime.Now.AddYears(2)).WithMessage("Data programarii nu poate fi mai mult de 2 ani in viitor");

        RuleFor(x => x.TipProgramare)
            .MaximumLength(100).WithMessage("Tipul programarii nu poate avea mai mult de 100 de caractere");

        RuleFor(x => x.Observatii)
            .MaximumLength(1000).WithMessage("Observatiile nu pot avea mai mult de 1000 de caractere");

        RuleFor(x => x.CreatDe)
            .NotEmpty().WithMessage("Utilizatorul care creeaza programarea este obligatoriu");
    }
}

public class CreateSemneVitaleValidator : AbstractValidator<CreateSemneVitaleRequest>
{
    public CreateSemneVitaleValidator()
    {
        RuleFor(x => x.PacientID)
            .NotEmpty().WithMessage("Pacientul este obligatoriu");

        RuleFor(x => x.TensiuneArterialaMax)
            .InclusiveBetween(60, 250).WithMessage("Tensiunea arteriala maxima trebuie sa fie intre 60 si 250 mmHg")
            .When(x => x.TensiuneArterialaMax.HasValue);

        RuleFor(x => x.TensiuneArterialaMin)
            .InclusiveBetween(40, 150).WithMessage("Tensiunea arteriala minima trebuie sa fie intre 40 si 150 mmHg")
            .When(x => x.TensiuneArterialaMin.HasValue);

        RuleFor(x => x)
            .Must(x => !x.TensiuneArterialaMax.HasValue || !x.TensiuneArterialaMin.HasValue || 
                      x.TensiuneArterialaMax > x.TensiuneArterialaMin)
            .WithMessage("Tensiunea arteriala maxima trebuie sa fie mai mare decat cea minima")
            .When(x => x.TensiuneArterialaMax.HasValue && x.TensiuneArterialaMin.HasValue);

        RuleFor(x => x.FrecariaCardiaca)
            .InclusiveBetween(30, 200).WithMessage("Frecventa cardiaca trebuie sa fie intre 30 si 200 bpm")
            .When(x => x.FrecariaCardiaca.HasValue);

        RuleFor(x => x.Temperatura)
            .InclusiveBetween(30.0m, 45.0m).WithMessage("Temperatura trebuie sa fie intre 30.0 si 45.0 grade C")
            .When(x => x.Temperatura.HasValue);

        RuleFor(x => x.Greutate)
            .InclusiveBetween(0.5m, 500.0m).WithMessage("Greutatea trebuie sa fie intre 0.5 si 500.0 kg")
            .When(x => x.Greutate.HasValue);

        RuleFor(x => x.Inaltime)
            .InclusiveBetween(30, 250).WithMessage("Inaltimea trebuie sa fie intre 30 si 250 cm")
            .When(x => x.Inaltime.HasValue);

        RuleFor(x => x.FrecariaRespiratorie)
            .InclusiveBetween(5, 60).WithMessage("Frecventa respiratorie trebuie sa fie intre 5 si 60/min")
            .When(x => x.FrecariaRespiratorie.HasValue);

        RuleFor(x => x.SaturatieOxigen)
            .InclusiveBetween(50.0m, 100.0m).WithMessage("Saturatia de oxigen trebuie sa fie intre 50.0 si 100.0%")
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
            .InclusiveBetween(1, 5).WithMessage("Nivelul de triaj trebuie sa fie intre 1 si 5");

        RuleFor(x => x.PlangereaPrincipala)
            .NotEmpty().WithMessage("Plangerea principala este obligatorie")
            .MaximumLength(1000).WithMessage("Plangerea principala nu poate avea mai mult de 1000 de caractere");

        RuleFor(x => x.Observatii)
            .MaximumLength(2000).WithMessage("Observatiile nu pot avea mai mult de 2000 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Observatii));
    }
}