using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Application.Features.Workers.DTOs;

public record CreateWorkerDto(
    string EmployeeCode,
    string FullName,
    string? Phone,
    string? Email,
    string? Department,
    WorkerPosition Position,
    WageType WageType,
    decimal WageRate,
    Currency WageCurrency,
    DateTime HiredDate,
    string? Skills,
    string? Notes,
    Guid? UserId
);

public record UpdateWorkerDto(
    string FullName,
    string? Phone,
    string? Email,
    string? Department,
    WorkerPosition Position,
    WageType WageType,
    decimal WageRate,
    Currency WageCurrency,
    DateTime HiredDate,
    DateTime? TerminatedDate,
    bool IsActive,
    string? Skills,
    string? Notes,
    Guid? UserId
);

public record WorkerResponseDto(
    Guid Id,
    string EmployeeCode,
    string FullName,
    string? Phone,
    string? Email,
    string? Department,
    string Position,
    string WageType,
    decimal WageRate,
    string WageCurrency,
    DateTime HiredDate,
    DateTime? TerminatedDate,
    bool IsActive,
    string? Skills,
    string? Notes,
    Guid? UserId,
    DateTime CreatedAt
);
