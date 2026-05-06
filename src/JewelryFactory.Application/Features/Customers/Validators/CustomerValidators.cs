using FluentValidation;
using JewelryFactory.Application.Features.Customers.Commands.CreateCustomer;
using JewelryFactory.Application.Features.Customers.Commands.UpdateCustomer;

namespace JewelryFactory.Application.Features.Customers.Validators;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Request.Code)
            .NotEmpty().MaximumLength(50)
            .Matches("^[A-Z0-9_-]+$").WithMessage("Code may contain A-Z, 0-9, '-' and '_' only.");

        RuleFor(x => x.Request.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Email).EmailAddress().MaximumLength(150).When(x => !string.IsNullOrWhiteSpace(x.Request.Email));
        RuleFor(x => x.Request.Phone).MaximumLength(50);
        RuleFor(x => x.Request.TaxId).MaximumLength(50);
        RuleFor(x => x.Request.CreditLimit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.Type).IsInEnum();
        RuleFor(x => x.Request.DefaultCurrency).IsInEnum();
        RuleFor(x => x.Request.PaymentTerms).IsInEnum();
    }
}

public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Email).EmailAddress().MaximumLength(150).When(x => !string.IsNullOrWhiteSpace(x.Request.Email));
        RuleFor(x => x.Request.CreditLimit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.Type).IsInEnum();
        RuleFor(x => x.Request.DefaultCurrency).IsInEnum();
        RuleFor(x => x.Request.PaymentTerms).IsInEnum();
    }
}
