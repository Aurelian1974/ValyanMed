using FluentValidation;
using Shared.DTOs.Medical;

namespace Shared.Validators.Medical;

public class CreatePersonalMedicalValidator : AbstractValidator<CreatePersonalMedicalRequest>
{
    public CreatePersonalMedicalValidator()
    {
        RuleFor(x => x.Nume)
            .NotEmpty().WithMessage("Numele este obligatoriu")
            .MaximumLength(100).WithMessage("Numele nu poate avea mai mult de 100 de caractere")
            .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("Numele poate contine doar litere, spatii, cratima si apostrof");

        RuleFor(x => x.Prenume)
            .NotEmpty().WithMessage("Prenumele este obligatoriu")
            .MaximumLength(100).WithMessage("Prenumele nu poate avea mai mult de 100 de caractere")
            .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("Prenumele poate contine doar litere, spatii, cratima si apostrof");

        RuleFor(x => x.Pozitie)
            .NotEmpty().WithMessage("Pozitia este obligatorie")
            .MaximumLength(50).WithMessage("Pozitia nu poate avea mai mult de 50 de caractere");

        RuleFor(x => x.Specializare)
            .MaximumLength(100).WithMessage("Specializarea nu poate avea mai mult de 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Specializare));

        RuleFor(x => x.NumarLicenta)
            .MaximumLength(50).WithMessage("Numarul de licenta nu poate avea mai mult de 50 de caractere")
            .When(x => !string.IsNullOrEmpty(x.NumarLicenta));

        RuleFor(x => x.Telefon)
            .Matches(@"^[0-9+\-\s\(\)]{10,15}$").WithMessage("Numarul de telefon nu este valid")
            .When(x => !string.IsNullOrEmpty(x.Telefon));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Adresa de email nu este valida")
            .MaximumLength(100).WithMessage("Email-ul nu poate avea mai mult de 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Departament)
            .MaximumLength(100).WithMessage("Departamentul nu poate avea mai mult de 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Departament));
    }
}

public class UpdatePersonalMedicalValidator : AbstractValidator<UpdatePersonalMedicalRequest>
{
    public UpdatePersonalMedicalValidator()
    {
        RuleFor(x => x.PersonalID)
            .NotEmpty().WithMessage("ID-ul personalului medical este obligatoriu");

        RuleFor(x => x.Nume)
            .NotEmpty().WithMessage("Numele este obligatoriu")
            .MaximumLength(100).WithMessage("Numele nu poate avea mai mult de 100 de caractere")
            .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("Numele poate contine doar litere, spatii, cratima si apostrof");

        RuleFor(x => x.Prenume)
            .NotEmpty().WithMessage("Prenumele este obligatoriu")
            .MaximumLength(100).WithMessage("Prenumele nu poate avea mai mult de 100 de caractere")
            .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("Prenumele poate contine doar litere, spatii, cratima si apostrof");

        RuleFor(x => x.Pozitie)
            .NotEmpty().WithMessage("Pozitia este obligatorie")
            .MaximumLength(50).WithMessage("Pozitia nu poate avea mai mult de 50 de caractere");

        RuleFor(x => x.Specializare)
            .MaximumLength(100).WithMessage("Specializarea nu poate avea mai mult de 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Specializare));

        RuleFor(x => x.NumarLicenta)
            .MaximumLength(50).WithMessage("Numarul de licenta nu poate avea mai mult de 50 de caractere")
            .When(x => !string.IsNullOrEmpty(x.NumarLicenta));

        RuleFor(x => x.Telefon)
            .Matches(@"^[0-9+\-\s\(\)]{10,15}$").WithMessage("Numarul de telefon nu este valid")
            .When(x => !string.IsNullOrEmpty(x.Telefon));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Adresa de email nu este valida")
            .MaximumLength(100).WithMessage("Email-ul nu poate avea mai mult de 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Departament)
            .MaximumLength(100).WithMessage("Departamentul nu poate avea mai mult de 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Departament));
    }
}