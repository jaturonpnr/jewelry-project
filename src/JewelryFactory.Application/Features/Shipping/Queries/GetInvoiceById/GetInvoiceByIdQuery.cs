using JewelryFactory.Application.Features.Shipping.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.Shipping.Queries.GetInvoiceById;

public record GetInvoiceByIdQuery(Guid Id) : IRequest<InvoiceResponse>;
