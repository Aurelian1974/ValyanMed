using FluentValidation;

namespace Client.Shared.Dialogs;

public class UserFormValidator : AbstractValidator<UserFormModel>
{
    public UserFormValidator()
    {
        RuleFor(x => x.NumeUtilizator)
            .NotEmpty().WithMessage("Numele de utilizator este obligatoriu")
            .MinimumLength(3).WithMessage("Numele de utilizator trebuie sa aiba minim 3 caractere")
            .MaximumLength(50).WithMessage("Numele de utilizator nu poate depasi 50 de caractere")
            .Matches("^[a-zA-Z0-9._-]+$").WithMessage("Numele de utilizator poate contine doar litere, cifre, punct, linie si underscore");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email-ul este obligatoriu")
            .EmailAddress().WithMessage("Format email invalid")
            .MaximumLength(150).WithMessage("Email-ul nu poate depasi 150 de caractere");

        RuleFor(x => x.Telefon)
            .Matches(@"^[+]?[0-9\s\-\(\)]+$").WithMessage("Format telefon invalid")
            .MinimumLength(10).WithMessage("Numarul de telefon trebuie sa aiba minim 10 cifre")
            .MaximumLength(20).WithMessage("Numarul de telefon nu poate depasi 20 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Telefon));

        RuleFor(x => x.SelectedPersoanaId)
            .NotNull().WithMessage("Trebuie sa selectati o persoana");

        RuleFor(x => x.Parola)
            .MinimumLength(6).WithMessage("Parola trebuie sa aiba minim 6 caractere")
            .MaximumLength(100).WithMessage("Parola nu poate depasi 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.Parola));

        RuleFor(x => x.NovaParola)
            .MinimumLength(6).WithMessage("Parola noua trebuie sa aiba minim 6 caractere")
            .MaximumLength(100).WithMessage("Parola noua nu poate depasi 100 de caractere")
            .When(x => !string.IsNullOrEmpty(x.NovaParola));
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<UserFormModel>.CreateWithOptions((UserFormModel)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}