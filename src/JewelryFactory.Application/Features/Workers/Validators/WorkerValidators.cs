using FluentValidation;
using JewelryFactory.Application.Features.Workers.Commands;

namespace JewelryFactory.Application.Features.Workers.Validators;

public class CreateWorkerValidator : AbstractValidator<CreateWorkerCommand>
{
    public CreateWorkerValidator()
    {
        RuleFor(x => x.Request.EmployeeCode).NotEmpty().MaximumLength(50)
            .Matches("^[A-Z0-9_-]+$");
        RuleFor(x => x.Request.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Request.Email).EmailAddress().MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Request.Email));
        RuleFor(x => x.Request.WageRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.HiredDate).LessThanOrEqualTo(DateTime.UtcNow.AddDays(1));
        RuleFor(x => x.Request.Position).IsInEnum();
        RuleFor(x => x.Request.WageType).IsInEnum();
        RuleFor(x => x.Request.WageCurrency).IsInEnum();
    }
}

public class UpdateWorkerValidator : AbstractValidator<UpdateWorkerCommand>
{
    public UpdateWorkerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Request.WageRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.Position).IsInEnum();
        RuleFor(x => x.Request.WageType).IsInEnum();
        RuleFor(x => x.Request.WageCurrency).IsInEnum();
    }
}
