using FluentValidation;
using JewelryFactory.Application.Features.MaterialTypes.Commands;

namespace JewelryFactory.Application.Features.MaterialTypes.Validators;

public class CreateMaterialTypeValidator : AbstractValidator<CreateMaterialTypeCommand>
{
    public CreateMaterialTypeValidator()
    {
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(50)
            .Matches("^[A-Z0-9_-]+$");
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Request.Category).IsInEnum();
        RuleFor(x => x.Request.Unit).IsInEnum();
        RuleFor(x => x.Request.PurityFraction).InclusiveBetween(0.0001m, 1.0000m).When(x => x.Request.PurityFraction.HasValue);
        RuleFor(x => x.Request.Karat).InclusiveBetween(1, 24).When(x => x.Request.Karat.HasValue);
    }
}

public class UpdateMaterialTypeValidator : AbstractValidator<UpdateMaterialTypeCommand>
{
    public UpdateMaterialTypeValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Request.Category).IsInEnum();
        RuleFor(x => x.Request.Unit).IsInEnum();
        RuleFor(x => x.Request.PurityFraction).InclusiveBetween(0.0001m, 1.0000m).When(x => x.Request.PurityFraction.HasValue);
        RuleFor(x => x.Request.Karat).InclusiveBetween(1, 24).When(x => x.Request.Karat.HasValue);
    }
}
