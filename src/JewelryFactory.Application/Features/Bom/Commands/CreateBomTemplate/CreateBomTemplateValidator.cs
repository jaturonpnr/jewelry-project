using FluentValidation;

namespace JewelryFactory.Application.Features.Bom.Commands.CreateBomTemplate;

public class CreateBomTemplateValidator : AbstractValidator<CreateBomTemplateCommand>
{
    public CreateBomTemplateValidator()
    {
        RuleFor(x => x.Request.DesignCode)
            .NotEmpty().MaximumLength(50)
            .Matches(@"^[A-Za-z0-9\-]+$").WithMessage("DesignCode may only contain letters, digits, and hyphens.");

        RuleFor(x => x.Request.DesignName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Request.Description).MaximumLength(1000).When(x => x.Request.Description is not null);

        RuleFor(x => x.Request.OverheadPercent)
            .InclusiveBetween(0, 100).WithMessage("OverheadPercent must be between 0 and 100.");

        When(x => x.Request.MaterialLines.Count > 0, () =>
        {
            RuleForEach(x => x.Request.MaterialLines).ChildRules(line =>
            {
                line.RuleFor(m => m.MaterialDescription).NotEmpty().MaximumLength(200);
                line.RuleFor(m => m.QuantityGrams).GreaterThan(0);
                line.RuleFor(m => m.ExpectedLossPercent).InclusiveBetween(0, 100);
                line.RuleFor(m => m.UnitCostThbPerGram).GreaterThanOrEqualTo(0);
                line.RuleFor(m => m.PurityFraction)
                    .InclusiveBetween(0, 1).When(m => m.PurityFraction.HasValue)
                    .WithMessage("PurityFraction must be between 0 and 1.");
            });
        });

        When(x => x.Request.StoneLines.Count > 0, () =>
        {
            RuleForEach(x => x.Request.StoneLines).ChildRules(line =>
            {
                line.RuleFor(s => s.StoneType).NotEmpty().MaximumLength(100);
                line.RuleFor(s => s.StoneShape).NotEmpty().MaximumLength(100);
                line.RuleFor(s => s.SizeDescription).NotEmpty().MaximumLength(200);
                line.RuleFor(s => s.CaratPerStone).GreaterThan(0);
                line.RuleFor(s => s.Quantity).GreaterThan(0);
                line.RuleFor(s => s.UnitCostThbPerCarat).GreaterThanOrEqualTo(0);
            });
        });

        When(x => x.Request.LaborLines.Count > 0, () =>
        {
            RuleForEach(x => x.Request.LaborLines).ChildRules(line =>
            {
                line.RuleFor(l => l.EstimatedHours).GreaterThan(0);
                line.RuleFor(l => l.HourlyRateThb).GreaterThanOrEqualTo(0);
            });
        });
    }
}
