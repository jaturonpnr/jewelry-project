using FluentValidation;

namespace JewelryFactory.Application.Features.Shipping.Commands.CreateInvoice;

public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.Request.CustomerId).NotEmpty();
        RuleFor(x => x.Request.DueDate).GreaterThanOrEqualTo(x => x.Request.InvoiceDate)
            .WithMessage("Due date must be on or after invoice date.");
        RuleFor(x => x.Request.ExchangeRateToThb).GreaterThan(0);
        RuleFor(x => x.Request.PaymentTerms).MaximumLength(200).When(x => x.Request.PaymentTerms is not null);
        RuleFor(x => x.Request.Notes).MaximumLength(2000).When(x => x.Request.Notes is not null);
        RuleFor(x => x.Request.LineItems).NotEmpty().WithMessage("Invoice must have at least one line item.");

        When(x => x.Request.LineItems.Count > 0, () =>
        {
            RuleForEach(x => x.Request.LineItems).ChildRules(l =>
            {
                l.RuleFor(i => i.Description).NotEmpty().MaximumLength(300);
                l.RuleFor(i => i.Quantity).GreaterThan(0);
                l.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
                l.RuleFor(i => i.DiscountPercent).InclusiveBetween(0, 100);
            });
        });
    }
}
