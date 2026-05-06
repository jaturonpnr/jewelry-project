using FluentValidation;
using JewelryFactory.Application.Features.Auth.Commands.Register;

namespace JewelryFactory.Application.Features.Auth.Validators;

/// <summary>
/// Validates the MediatR command (not the inner DTO) so it runs in the ValidationBehavior pipeline.
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid")
            .MaximumLength(150);

        RuleFor(x => x.Request.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain a digit");

        RuleFor(x => x.Request.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(150);

        RuleFor(x => x.Request.Role)
            .IsInEnum().WithMessage("Invalid role");
    }
}
