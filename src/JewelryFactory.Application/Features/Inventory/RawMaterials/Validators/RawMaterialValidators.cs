using FluentValidation;
using JewelryFactory.Application.Features.Inventory.RawMaterials.Commands.AdjustRawMaterial;
using JewelryFactory.Application.Features.Inventory.RawMaterials.Commands.ReceiveRawMaterial;

namespace JewelryFactory.Application.Features.Inventory.RawMaterials.Validators;

public class ReceiveRawMaterialValidator : AbstractValidator<ReceiveRawMaterialCommand>
{
    public ReceiveRawMaterialValidator()
    {
        RuleFor(x => x.Request.MaterialTypeId).NotEmpty();
        RuleFor(x => x.Request.LotNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.Location).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.Quantity).GreaterThan(0).WithMessage("Quantity must be > 0");
        RuleFor(x => x.Request.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.CostCurrency).IsInEnum();
        RuleFor(x => x.Request.ReceivedDate).LessThanOrEqualTo(DateTime.UtcNow.AddDays(1));
    }
}

public class AdjustRawMaterialValidator : AbstractValidator<AdjustRawMaterialCommand>
{
    public AdjustRawMaterialValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.QuantityDelta).NotEqual(0).WithMessage("QuantityDelta cannot be zero");
        RuleFor(x => x.Request.Reason).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.RowVersion).NotEmpty().WithMessage("RowVersion is required for optimistic concurrency");
    }
}
