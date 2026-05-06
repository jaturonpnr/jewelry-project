using FluentValidation;
using JewelryFactory.Application.Features.Suppliers.Commands.CreateSupplier;
using JewelryFactory.Application.Features.Suppliers.Commands.UpdateSupplier;

namespace JewelryFactory.Application.Features.Suppliers.Validators;

public class CreateSupplierValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierValidator()
    {
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(50)
            .Matches("^[A-Z0-9_-]+$").WithMessage("Code may contain A-Z, 0-9, '-' and '_' only.");
        RuleFor(x => x.Request.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Email).EmailAddress().MaximumLength(150).When(x => !string.IsNullOrWhiteSpace(x.Request.Email));
        RuleFor(x => x.Request.Type).IsInEnum();
        RuleFor(x => x.Request.DefaultCurrency).IsInEnum();
        RuleFor(x => x.Request.PaymentTerms).IsInEnum();
    }
}

public class UpdateSupplierValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Email).EmailAddress().MaximumLength(150).When(x => !string.IsNullOrWhiteSpace(x.Request.Email));
        RuleFor(x => x.Request.Type).IsInEnum();
        RuleFor(x => x.Request.DefaultCurrency).IsInEnum();
        RuleFor(x => x.Request.PaymentTerms).IsInEnum();
    }
}
