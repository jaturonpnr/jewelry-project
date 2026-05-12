using FluentValidation;

namespace JewelryFactory.Application.Features.Shipping.Commands.CreateShipment;

public class CreateShipmentValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentValidator()
    {
        RuleFor(x => x.Request.Carrier).MaximumLength(100).When(x => x.Request.Carrier is not null);
        RuleFor(x => x.Request.TrackingNumber).MaximumLength(100).When(x => x.Request.TrackingNumber is not null);
        RuleFor(x => x.Request.ShippingMethod).MaximumLength(100).When(x => x.Request.ShippingMethod is not null);
        RuleFor(x => x.Request.PackingNotes).MaximumLength(2000).When(x => x.Request.PackingNotes is not null);

        RuleFor(x => x.Request.Items).NotEmpty().WithMessage("Shipment must have at least one item.");

        When(x => x.Request.Items.Count > 0, () =>
        {
            RuleForEach(x => x.Request.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.Description).NotEmpty().MaximumLength(300);
                item.RuleFor(i => i.Quantity).GreaterThan(0);
                item.RuleFor(i => i.WeightGrams).GreaterThanOrEqualTo(0);
            });
        });
    }
}
