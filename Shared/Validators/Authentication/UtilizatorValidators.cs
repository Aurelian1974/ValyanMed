using FluentValidation;
using Shared.DTOs.Authentication;

namespace Shared.Validators.Authentication;

public class CreateUtilizatorValidator : AbstractValidator<CreateUtilizatorRequest>
{
    public CreateUtilizatorValidator()
    {
        RuleFor(x => x.NumeUtilizator)
            .NotEmpty().WithMessage("Numele de utilizator este obligatoriu")
            .Length(3, 50).WithMessage("Numele de utilizator trebuie sa aiba intre 3 si 50 de caractere")
            .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("Numele de utilizator poate contine doar litere, cifre, punct, liniuta si underscore");

        RuleFor(x => x.Parola)
            .NotEmpty().WithMessage("Parola este obligatorie")
            .MinimumLength(8).WithMessage("Parola trebuie sa aiba cel putin 8 caractere")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            .WithMessage("Parola trebuie sa contina cel putin o litera mica, o litera mare, o cifra si un caracter special");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email-ul este obligatoriu")
            .EmailAddress().WithMessage("Formatul email-ului nu este valid")
            .MaximumLength(150).WithMessage("Email-ul nu poate avea mai mult de 150 de caractere");

        RuleFor(x => x.Telefon)
            .Matches(@"^[0-9+\-\s()]+$").When(x => !string.IsNullOrEmpty(x.Telefon))
            .WithMessage("Formatul numarului de telefon nu este valid")
            .Length(10, 20).When(x => !string.IsNullOrEmpty(x.Telefon))
            .WithMessage("Numarul de telefon trebuie sa aiba intre 10 si 20 de caractere");

        RuleFor(x => x.PersoanaId)
            .GreaterThan(0).WithMessage("Trebuie sa selectati o persoana valida");
    }
}

public class UpdateUtilizatorValidator : AbstractValidator<UpdateUtilizatorRequest>
{
    public UpdateUtilizatorValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID-ul utilizatorului trebuie sa fie valid");

        RuleFor(x => x.NumeUtilizator)
            .NotEmpty().WithMessage("Numele de utilizator este obligatoriu")
            .Length(3, 50).WithMessage("Numele de utilizator trebuie sa aiba intre 3 si 50 de caractere")
            .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("Numele de utilizator poate contine doar litere, cifre, punct, liniuta si underscore");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email-ul este obligatoriu")
            .EmailAddress().WithMessage("Formatul email-ului nu este valid")
            .MaximumLength(150).WithMessage("Email-ul nu poate avea mai mult de 150 de caractere");

        RuleFor(x => x.Telefon)
            .Matches(@"^[0-9+\-\s()]+$").When(x => !string.IsNullOrEmpty(x.Telefon))
            .WithMessage("Formatul numarului de telefon nu este valid")
            .Length(10, 20).When(x => !string.IsNullOrEmpty(x.Telefon))
            .WithMessage("Numarul de telefon trebuie sa aiba intre 10 si 20 de caractere");

        RuleFor(x => x.PersoanaId)
            .GreaterThan(0).WithMessage("Trebuie sa selectati o persoana valida");

        RuleFor(x => x.NovaParola)
            .MinimumLength(8).When(x => !string.IsNullOrEmpty(x.NovaParola))
            .WithMessage("Parola trebuie sa aiba cel putin 8 caractere")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            .When(x => !string.IsNullOrEmpty(x.NovaParola))
            .WithMessage("Parola trebuie sa contina cel putin o litera mica, o litera mare, o cifra si un caracter special");
    }
}

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.NumeUtilizatorSauEmail)
            .NotEmpty().WithMessage("Numele de utilizator sau email-ul este obligatoriu");

        RuleFor(x => x.Parola)
            .NotEmpty().WithMessage("Parola este obligatorie");
    }
}