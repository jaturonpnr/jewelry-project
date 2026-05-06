using JewelryFactory.Application.Features.MaterialTypes.DTOs;
using JewelryFactory.Domain.Entities;

namespace JewelryFactory.Application.Features.MaterialTypes.Mappings;

internal static class MaterialTypeMapper
{
    public static MaterialTypeResponseDto ToResponse(this MaterialType m) => new(
        m.Id, m.Code, m.Name, m.Category.ToString(), m.Unit.ToString(),
        m.PurityFraction, m.Karat, m.IsActive, m.Description, m.CreatedAt
    );
}
