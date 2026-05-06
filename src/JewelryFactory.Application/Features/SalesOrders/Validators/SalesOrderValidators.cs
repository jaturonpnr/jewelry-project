using FluentValidation;
using JewelryFactory.Application.Features.SalesOrders.Commands.ChangeSalesOrderStatus;
using JewelryFactory.Application.Features.SalesOrders.Commands.CreateSalesOrder;
using JewelryFactory.Application.Features.SalesOrders.Commands.UpdateSalesOrder;

namespace JewelryFactory.Application.Features.SalesOrders.Validators;

public class CreateSalesOrderValidator : AbstractValidator<CreateSalesOrderCommand>
{
    public CreateSalesOrderValidator()
    {
        RuleFor(x => x.Request.OrderNumber).NotEmpty().MaximumLength(50)
            .Matches("^[A-Z0-9_-]+$");
        RuleFor(x => x.Request.CustomerId).NotEmpty();
        RuleFor(x => x.Request.Currency).IsInEnum();
        RuleFor(x => x.Request.OrderDate).LessThanOrEqualTo(DateTime.UtcNow.AddDays(1));
        RuleFor(x => x.Request.DiscountAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.TaxRatePercent).InclusiveBetween(0, 100);
        RuleFor(x => x.Request.ShippingCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.Items).NotEmpty().WithMessage("Order must have at least one item");
        RuleForEach(x => x.Request.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description).NotEmpty().MaximumLength(500);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
            item.RuleFor(i => i.LineDiscount).GreaterThanOrEqualTo(0);
        });
    }
}

public class UpdateSalesOrderValidator : AbstractValidator<UpdateSalesOrderCommand>
{
    public UpdateSalesOrderValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.RowVersion).NotEmpty();
        RuleFor(x => x.Request.DiscountAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.TaxRatePercent).InclusiveBetween(0, 100);
        RuleFor(x => x.Request.ShippingCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.Items).NotEmpty();
    }
}

public class ChangeSalesOrderStatusValidator : AbstractValidator<ChangeSalesOrderStatusCommand>
{
    public ChangeSalesOrderStatusValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.NewStatus).IsInEnum();
        RuleFor(x => x.Request.RowVersion).NotEmpty();
    }
}
