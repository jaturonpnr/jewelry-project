using JewelryFactory.Application.Features.Shipping.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.Shipping.Commands.UpdateInvoiceStatus;

public record UpdateInvoiceStatusCommand(Guid Id, UpdateInvoiceStatusDto Request) : IRequest;
