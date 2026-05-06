using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Application.Features.MaterialTypes.DTOs;

public record CreateMaterialTypeDto(
    string Code,
    string Name,
    MaterialCategory Category,
    MaterialUnit Unit,
    decimal? PurityFraction,
    int? Karat,
    string? Description
);

public record UpdateMaterialTypeDto(
    string Name,
    MaterialCategory Category,
    MaterialUnit Unit,
    decimal? PurityFraction,
    int? Karat,
    bool IsActive,
    string? Description
);

public record MaterialTypeResponseDto(
    Guid Id,
    string Code,
    string Name,
    string Category,
    string Unit,
    decimal? PurityFraction,
    int? Karat,
    bool IsActive,
    string? Description,
    DateTime CreatedAt
);
