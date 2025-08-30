using Client.Services.Authentication;
using FluentValidation;
using Shared.DTOs.Authentication;
using Shared.Validators.Authentication;

namespace Client.Validators.Authentication;

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