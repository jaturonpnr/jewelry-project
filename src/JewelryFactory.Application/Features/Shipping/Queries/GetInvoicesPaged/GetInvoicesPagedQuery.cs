using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Shipping.DTOs;
using JewelryFactory.Domain.Enums;
using MediatR;

namespace JewelryFactory.Application.Features.Shipping.Queries.GetInvoicesPaged;

public record GetInvoicesPagedQuery(
    int Page, int PageSize,
    InvoiceStatus? Status,
    Guid? CustomerId,
    string? Search
) : IRequest<PagedResult<InvoiceSummary>>;
