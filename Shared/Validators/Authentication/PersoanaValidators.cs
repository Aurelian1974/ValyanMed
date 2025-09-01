using FluentValidation;
using Shared.DTOs.Authentication;

namespace Shared.Validators.Authentication;

public class CreatePersoanaValidator : AbstractValidator<CreatePersoanaRequest>
{
    public CreatePersoanaValidator()
    {
        RuleFor(x => x.Nume)
            .NotEmpty().WithMessage("Numele este obligatoriu")
            .Length(2, 100).WithMessage("Numele trebuie sa aiba intre 2 si 100 de caractere");

        RuleFor(x => x.Prenume)
            .NotEmpty().WithMessage("Prenumele este obligatoriu")
            .Length(2, 100).WithMessage("Prenumele trebuie sa aiba intre 2 si 100 de caractere");

        RuleFor(x => x.CNP)
            .Length(13).When(x => !string.IsNullOrEmpty(x.CNP))
            .WithMessage("CNP-ul trebuie sa aiba exact 13 cifre")
            .Matches(@"^\d{13}$").When(x => !string.IsNullOrEmpty(x.CNP))
            .WithMessage("CNP-ul trebuie sa contina doar cifre");

        RuleFor(x => x.DataNasterii)
            .LessThan(DateTime.Today).When(x => x.DataNasterii.HasValue)
            .WithMessage("Data nasterii trebuie sa fie in trecut")
            .GreaterThan(DateTime.Today.AddYears(-120)).When(x => x.DataNasterii.HasValue)
            .WithMessage("Data nasterii nu este realista");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email-ul nu are un format valid");

        RuleFor(x => x.Telefon)
            .Matches(@"^[0-9+\-\s]+$").When(x => !string.IsNullOrEmpty(x.Telefon))
            .WithMessage("Numarul de telefon poate contine doar cifre, +, - si spatii");
    }
}

public class UpdatePersoanaValidator : AbstractValidator<UpdatePersoanaRequest>
{
    public UpdatePersoanaValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID-ul persoanei trebuie sa fie valid");

        RuleFor(x => x.Nume)
            .NotEmpty().WithMessage("Numele este obligatoriu")
            .Length(2, 100).WithMessage("Numele trebuie sa aiba intre 2 si 100 de caractere");

        RuleFor(x => x.Prenume)
            .NotEmpty().WithMessage("Prenumele este obligatoriu")
            .Length(2, 100).WithMessage("Prenumele trebuie sa aiba intre 2 si 100 de caractere");

        RuleFor(x => x.CNP)
            .Length(13).When(x => !string.IsNullOrEmpty(x.CNP))
            .WithMessage("CNP-ul trebuie sa aiba exact 13 cifre")
            .Matches(@"^\d{13}$").When(x => !string.IsNullOrEmpty(x.CNP))
            .WithMessage("CNP-ul trebuie sa contina doar cifre");

        RuleFor(x => x.DataNasterii)
            .LessThan(DateTime.Today).When(x => x.DataNasterii.HasValue)
            .WithMessage("Data nasterii trebuie sa fie in trecut")
            .GreaterThan(DateTime.Today.AddYears(-120)).When(x => x.DataNasterii.HasValue)
            .WithMessage("Data nasterii nu este realista");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email-ul nu are un format valid");

        RuleFor(x => x.Telefon)
            .Matches(@"^[0-9+\-\s]+$").When(x => !string.IsNullOrEmpty(x.Telefon))
            .WithMessage("Numarul de telefon poate contine doar cifre, +, - si spatii");
    }
}