using Client.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Validators
{
    public class RegisterModelValidator : AbstractValidator<RegisterModel>
    {
        public RegisterModelValidator()
        {
            RuleFor(x => x.NumeUtilizator)
                .NotEmpty().WithMessage("Numele de utilizator este obligatoriu")
                .MaximumLength(50).WithMessage("Numele de utilizator nu poate depași 50 de caractere");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email-ul este obligatoriu")
                .EmailAddress().WithMessage("Format email invalid");

            RuleFor(x => x.Parola)
                .NotEmpty().WithMessage("Parola este obligatorie")
                .MinimumLength(6).WithMessage("Parola trebuie sa aiba minim 6 caractere")
                .MaximumLength(100).WithMessage("Parola nu poate depași 100 de caractere");

            RuleFor(x => x.ConfirmaParola)
                .NotEmpty().WithMessage("Confirmarea parolei este obligatorie")
                .Equal(x => x.Parola).WithMessage("Parolele nu coincid");
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<RegisterModel>.CreateWithOptions((RegisterModel)model, x => x.IncludeProperties(propertyName)));
            if (result.IsValid)
                return Array.Empty<string>();
            return result.Errors.Select(e => e.ErrorMessage);
        };
    }
}