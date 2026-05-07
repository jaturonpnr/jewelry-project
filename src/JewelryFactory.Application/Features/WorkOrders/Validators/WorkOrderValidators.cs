using FluentValidation;
using JewelryFactory.Application.Features.WorkOrders.Commands;
using JewelryFactory.Application.Features.WorkOrders.Commands.ChangeWorkOrderStatus;
using JewelryFactory.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using JewelryFactory.Application.Features.WorkOrders.Commands.UpdateWorkOrder;

namespace JewelryFactory.Application.Features.WorkOrders.Validators;

public class CreateWorkOrderValidator : AbstractValidator<CreateWorkOrderCommand>
{
    public CreateWorkOrderValidator()
    {
        RuleFor(x => x.Request.WorkOrderNumber).NotEmpty().MaximumLength(50)
            .Matches("^[A-Z0-9_-]+$");
        RuleFor(x => x.Request.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Request.Quantity).GreaterThan(0);
        RuleFor(x => x.Request.Priority).IsInEnum();
        When(x => x.Request.StageEstimates is not null, () =>
        {
            RuleForEach(x => x.Request.StageEstimates!).ChildRules(s =>
            {
                s.RuleFor(e => e.Stage).IsInEnum();
                s.RuleFor(e => e.EstimatedHours).GreaterThanOrEqualTo(0);
            });
        });
    }
}

public class UpdateWorkOrderValidator : AbstractValidator<UpdateWorkOrderCommand>
{
    public UpdateWorkOrderValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.RowVersion).NotEmpty();
        RuleFor(x => x.Request.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Request.Quantity).GreaterThan(0);
        RuleFor(x => x.Request.Priority).IsInEnum();
    }
}

public class ChangeWorkOrderStatusValidator : AbstractValidator<ChangeWorkOrderStatusCommand>
{
    public ChangeWorkOrderStatusValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.NewStatus).IsInEnum();
        RuleFor(x => x.Request.RowVersion).NotEmpty();
    }
}

public class StartStageValidator : AbstractValidator<StartStageCommand>
{
    public StartStageValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.StageId).NotEmpty();
        RuleFor(x => x.Request.RowVersion).NotEmpty();
        RuleFor(x => x.Request.WeightInGrams).GreaterThanOrEqualTo(0).When(x => x.Request.WeightInGrams.HasValue);
    }
}

public class CompleteStageValidator : AbstractValidator<CompleteStageCommand>
{
    public CompleteStageValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.StageId).NotEmpty();
        RuleFor(x => x.Request.ActualHours).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.RowVersion).NotEmpty();
    }
}

public class SkipStageValidator : AbstractValidator<SkipStageCommand>
{
    public SkipStageValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.StageId).NotEmpty();
        RuleFor(x => x.Request.Reason).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.RowVersion).NotEmpty();
    }
}

public class FailStageValidator : AbstractValidator<FailStageCommand>
{
    public FailStageValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.StageId).NotEmpty();
        RuleFor(x => x.Request.FailureReason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Request.RowVersion).NotEmpty();
    }
}
