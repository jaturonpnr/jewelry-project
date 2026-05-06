using JewelryFactory.Application.Features.Workers.DTOs;
using JewelryFactory.Domain.Entities;

namespace JewelryFactory.Application.Features.Workers.Mappings;

internal static class WorkerMapper
{
    public static WorkerResponseDto ToResponse(this Worker w) => new(
        w.Id, w.EmployeeCode, w.FullName, w.Phone, w.Email, w.Department,
        w.Position.ToString(), w.WageType.ToString(), w.WageRate, w.WageCurrency.ToString(),
        w.HiredDate, w.TerminatedDate, w.IsActive, w.Skills, w.Notes, w.UserId, w.CreatedAt
    );
}
