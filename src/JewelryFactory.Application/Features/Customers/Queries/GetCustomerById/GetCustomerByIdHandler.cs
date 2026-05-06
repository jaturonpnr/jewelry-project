using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Customers.DTOs;
using JewelryFactory.Application.Features.Customers.Mappings;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetCustomerByIdQuery, CustomerResponseDto>
{
    public async Task<CustomerResponseDto> Handle(GetCustomerByIdQuery query, CancellationToken ct)
    {
        var customer = await db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Customer), query.Id);

        return customer.ToResponse();
    }
}
