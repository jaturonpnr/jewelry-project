using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.QualityControl.DTOs;
using JewelryFactory.Domain.Enums;
using MediatR;

namespace JewelryFactory.Application.Features.QualityControl.Queries.GetQcInspectionsPaged;

public record GetQcInspectionsPagedQuery(
    int Page,
    int PageSize,
    QcInspectionType? InspectionType,
    QcResult? Result,
    Guid? WorkOrderId,
    DateTime? FromDate,
    DateTime? ToDate
) : IRequest<PagedResult<QcInspectionSummary>>;
