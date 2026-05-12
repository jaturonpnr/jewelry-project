using FluentValidation;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Application.Features.QualityControl.Commands.CreateQcInspection;

public class CreateQcInspectionValidator : AbstractValidator<CreateQcInspectionCommand>
{
    public CreateQcInspectionValidator()
    {
        RuleFor(x => x.Request.InspectorName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Request.Notes).MaximumLength(2000).When(x => x.Request.Notes is not null);

        // InProcess / Final must be linked to a work order
        RuleFor(x => x.Request.WorkOrderId)
            .NotNull().WithMessage("WorkOrderId is required for InProcess and Final inspections.")
            .When(x => x.Request.InspectionType != QcInspectionType.Incoming);

        // Incoming must be linked to a raw material
        RuleFor(x => x.Request.RawMaterialItemId)
            .NotNull().WithMessage("RawMaterialItemId is required for Incoming inspections.")
            .When(x => x.Request.InspectionType == QcInspectionType.Incoming);

        // ReworkStage only valid when Result == Rework
        RuleFor(x => x.Request.ReworkStage)
            .NotNull().WithMessage("ReworkStage is required when Result is Rework.")
            .When(x => x.Request.Result == QcResult.Rework);

        RuleFor(x => x.Request.ActualWeightGrams)
            .GreaterThan(0).When(x => x.Request.ActualWeightGrams.HasValue);

        When(x => x.Request.Defects.Count > 0, () =>
        {
            RuleForEach(x => x.Request.Defects).ChildRules(d =>
            {
                d.RuleFor(x => x.DefectType).NotEmpty().MaximumLength(100);
                d.RuleFor(x => x.Quantity).GreaterThan(0);
                d.RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description is not null);
            });
        });

        // Critical defect → must be Fail or Rework
        RuleFor(x => x.Request.Result)
            .Must((cmd, result) =>
            {
                var hasCritical = cmd.Request.Defects.Any(d => d.Severity == DefectSeverity.Critical);
                return !hasCritical || result != QcResult.Pass;
            })
            .WithMessage("Inspection with Critical defects cannot be marked as Pass.");
    }
}
