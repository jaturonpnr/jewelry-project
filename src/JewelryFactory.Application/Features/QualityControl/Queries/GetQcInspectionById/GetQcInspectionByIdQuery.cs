using JewelryFactory.Application.Features.QualityControl.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.QualityControl.Queries.GetQcInspectionById;

public record GetQcInspectionByIdQuery(Guid Id) : IRequest<QcInspectionResponse>;
