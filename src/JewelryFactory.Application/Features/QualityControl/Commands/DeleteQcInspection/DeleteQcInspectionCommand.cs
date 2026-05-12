using MediatR;

namespace JewelryFactory.Application.Features.QualityControl.Commands.DeleteQcInspection;

public record DeleteQcInspectionCommand(Guid Id) : IRequest;
