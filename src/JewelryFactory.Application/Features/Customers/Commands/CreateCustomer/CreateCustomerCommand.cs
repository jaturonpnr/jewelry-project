using JewelryFactory.Application.Features.Customers.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(CreateCustomerDto Request) : IRequest<CustomerResponseDto>;
