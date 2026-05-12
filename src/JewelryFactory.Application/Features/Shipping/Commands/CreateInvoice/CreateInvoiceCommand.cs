using JewelryFactory.Application.Features.Shipping.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.Shipping.Commands.CreateInvoice;

public record CreateInvoiceCommand(CreateInvoiceDto Request) : IRequest<Guid>;
