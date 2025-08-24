using FluentValidation;

namespace Client.Shared.Dialogs;

public class PersonFormValidator : AbstractValidator<PersonFormModel>
{
    public PersonFormValidator()
    {
        RuleFor(x => x.Nume)
            .NotEmpty().WithMessage("Numele este obligatoriu")
            .MinimumLength(2).WithMessage("Numele trebuie sa aiba minim 2 caractere")
            .MaximumLength(100).WithMessage("Numele nu poate depasi 100 de caractere");

        RuleFor(x => x.Prenume)
            .NotEmpty().WithMessage("Prenumele este obligatoriu")
            .MinimumLength(2).WithMessage("Prenumele trebuie sa aiba minim 2 caractere")
            .MaximumLength(100).WithMessage("Prenumele nu poate depasi 100 de caractere");

        RuleFor(x => x.CNP)
            .Length(13).WithMessage("CNP-ul trebuie sa aiba exact 13 cifre")
            .When(x => !string.IsNullOrEmpty(x.CNP));

        RuleFor(x => x.DataNasterii)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Data nasterii nu poate fi in viitor")
            .GreaterThan(DateTime.Today.AddYears(-150)).WithMessage("Data nasterii nu poate fi mai mult de 150 de ani in urma")
            .When(x => x.DataNasterii.HasValue);

        RuleFor(x => x.Judet)
            .MaximumLength(100).WithMessage("Judetul nu poate depasi 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Judet));

        RuleFor(x => x.Localitate)
            .MaximumLength(100).WithMessage("Localitatea nu poate depasi 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Localitate));

        RuleFor(x => x.Strada)
            .MaximumLength(150).WithMessage("Strada nu poate depasi 150 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Strada));

        RuleFor(x => x.NumarStrada)
            .MaximumLength(50).WithMessage("Numarul strazii nu poate depasi 50 de caractere")
            .When(x => !string.IsNullOrEmpty(x.NumarStrada));

        RuleFor(x => x.CodPostal)
            .MaximumLength(20).WithMessage("Codul postal nu poate depasi 20 de caractere")
            .When(x => !string.IsNullOrEmpty(x.CodPostal));

        RuleFor(x => x.PozitieOrganizatie)
            .MaximumLength(100).WithMessage("Pozitia in organizatie nu poate depasi 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.PozitieOrganizatie));

        RuleFor(x => x.SerieActIdentitate)
            .MaximumLength(2).WithMessage("Seria actului de identitate nu poate depasi 2 caractere")
            .When(x => !string.IsNullOrEmpty(x.SerieActIdentitate));

        RuleFor(x => x.NumarActIdentitate)
            .MaximumLength(6).WithMessage("Numarul actului de identitate nu poate depasi 6 caractere")
            .When(x => !string.IsNullOrEmpty(x.NumarActIdentitate));
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<PersonFormModel>.CreateWithOptions((PersonFormModel)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}