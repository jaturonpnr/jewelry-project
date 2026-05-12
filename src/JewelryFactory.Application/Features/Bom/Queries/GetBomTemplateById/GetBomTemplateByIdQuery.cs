using JewelryFactory.Application.Features.Bom.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.Bom.Queries.GetBomTemplateById;

public record GetBomTemplateByIdQuery(Guid Id) : IRequest<BomTemplateResponse>;
