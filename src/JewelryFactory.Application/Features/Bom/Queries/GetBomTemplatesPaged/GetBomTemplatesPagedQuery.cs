using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Bom.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.Bom.Queries.GetBomTemplatesPaged;

public record GetBomTemplatesPagedQuery(
    int Page,
    int PageSize,
    string? Search,
    bool? IsActive
) : IRequest<PagedResult<BomTemplateSummaryResponse>>;
