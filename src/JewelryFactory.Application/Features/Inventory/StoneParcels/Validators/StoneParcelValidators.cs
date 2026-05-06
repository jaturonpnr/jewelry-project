using FluentValidation;
using JewelryFactory.Application.Features.Inventory.StoneParcels.Commands;

namespace JewelryFactory.Application.Features.Inventory.StoneParcels.Validators;

public class ReceiveStoneParcelValidator : AbstractValidator<ReceiveStoneParcelCommand>
{
    public ReceiveStoneParcelValidator()
    {
        RuleFor(x => x.Request.ParcelCode).NotEmpty().MaximumLength(50)
            .Matches("^[A-Z0-9_-]+$");
        RuleFor(x => x.Request.MaterialTypeId).NotEmpty();
        RuleFor(x => x.Request.TotalCarat).GreaterThan(0);
        RuleFor(x => x.Request.StoneCount).GreaterThan(0);
        RuleFor(x => x.Request.UnitCostPerCarat).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.Location).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.CostCurrency).IsInEnum();
        RuleFor(x => x.Request.Shape).IsInEnum();
    }
}

public class AdjustStoneParcelValidator : AbstractValidator<AdjustStoneParcelCommand>
{
    public AdjustStoneParcelValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.Reason).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.RowVersion).NotEmpty();
    }
}
