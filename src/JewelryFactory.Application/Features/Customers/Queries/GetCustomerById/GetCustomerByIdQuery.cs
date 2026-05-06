using JewelryFactory.Application.Features.Customers.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerResponseDto>;
