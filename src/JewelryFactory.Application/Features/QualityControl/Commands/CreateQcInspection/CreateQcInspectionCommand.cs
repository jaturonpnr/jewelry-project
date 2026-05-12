using JewelryFactory.Application.Features.QualityControl.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.QualityControl.Commands.CreateQcInspection;

public record CreateQcInspectionCommand(CreateQcInspectionDto Request) : IRequest<Guid>;
