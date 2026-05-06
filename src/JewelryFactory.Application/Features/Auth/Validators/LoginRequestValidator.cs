using FluentValidation;
using JewelryFactory.Application.Features.Auth.Commands.Login;

namespace JewelryFactory.Application.Features.Auth.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Request.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Request.Password)
            .NotEmpty();
    }
}
