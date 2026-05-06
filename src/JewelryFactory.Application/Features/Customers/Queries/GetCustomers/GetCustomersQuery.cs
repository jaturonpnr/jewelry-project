using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Customers.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.Customers.Queries.GetCustomers;

public class GetCustomersQuery : PagedRequest, IRequest<PagedResult<CustomerResponseDto>>;
