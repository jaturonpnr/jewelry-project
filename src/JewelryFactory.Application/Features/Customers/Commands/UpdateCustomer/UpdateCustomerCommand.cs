using JewelryFactory.Application.Features.Customers.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.Customers.Commands.UpdateCustomer;

public record UpdateCustomerCommand(Guid Id, UpdateCustomerDto Request) : IRequest<CustomerResponseDto>;
