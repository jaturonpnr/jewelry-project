using FluentValidation;
using JewelryFactory.Application.Features.Inventory.StoneItems.Commands;

namespace JewelryFactory.Application.Features.Inventory.StoneItems.Validators;

public class ReceiveStoneItemValidator : AbstractValidator<ReceiveStoneItemCommand>
{
    public ReceiveStoneItemValidator()
    {
        RuleFor(x => x.Request.ItemCode).NotEmpty().MaximumLength(50)
            .Matches("^[A-Z0-9_-]+$");
        RuleFor(x => x.Request.MaterialTypeId).NotEmpty();
        RuleFor(x => x.Request.CaratWeight).GreaterThan(0).LessThan(1000);
        RuleFor(x => x.Request.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.Location).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.Shape).IsInEnum();
        RuleFor(x => x.Request.CertAuthority).IsInEnum();
        RuleFor(x => x.Request.CostCurrency).IsInEnum();
        RuleFor(x => x.Request.ReceivedDate).LessThanOrEqualTo(DateTime.UtcNow.AddDays(1));
    }
}

public class UpdateStoneItemStatusValidator : AbstractValidator<UpdateStoneItemStatusCommand>
{
    public UpdateStoneItemStatusValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.Status).IsInEnum();
        RuleFor(x => x.Request.RowVersion).NotEmpty();
    }
}
