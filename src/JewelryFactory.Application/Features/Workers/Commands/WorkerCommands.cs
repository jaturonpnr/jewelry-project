using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Workers.DTOs;
using JewelryFactory.Application.Features.Workers.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Workers.Commands;

public record CreateWorkerCommand(CreateWorkerDto Request) : IRequest<WorkerResponseDto>;
public record UpdateWorkerCommand(Guid Id, UpdateWorkerDto Request) : IRequest<WorkerResponseDto>;
public record DeleteWorkerCommand(Guid Id) : IRequest;

public class CreateWorkerHandler(IApplicationDbContext db)
    : IRequestHandler<CreateWorkerCommand, WorkerResponseDto>
{
    public async Task<WorkerResponseDto> Handle(CreateWorkerCommand command, CancellationToken ct)
    {
        var dto = command.Request;
        var code = dto.EmployeeCode.Trim().ToUpperInvariant();

        if (await db.Workers.AnyAsync(w => w.EmployeeCode == code, ct))
            throw new BusinessRuleException($"Employee code '{code}' is already in use.");

        var worker = new Worker
        {
            EmployeeCode = code,
            FullName = dto.FullName.Trim(),
            Phone = dto.Phone?.Trim(),
            Email = dto.Email?.Trim().ToLowerInvariant(),
            Department = dto.Department?.Trim(),
            Position = dto.Position,
            WageType = dto.WageType,
            WageRate = dto.WageRate,
            WageCurrency = dto.WageCurrency,
            HiredDate = dto.HiredDate,
            Skills = dto.Skills?.Trim(),
            Notes = dto.Notes?.Trim(),
            UserId = dto.UserId
        };

        db.Workers.Add(worker);
        await db.SaveChangesAsync(ct);
        return worker.ToResponse();
    }
}

public class UpdateWorkerHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateWorkerCommand, WorkerResponseDto>
{
    public async Task<WorkerResponseDto> Handle(UpdateWorkerCommand command, CancellationToken ct)
    {
        var worker = await db.Workers.FirstOrDefaultAsync(w => w.Id == command.Id, ct)
            ?? throw new NotFoundException(nameof(Worker), command.Id);

        var dto = command.Request;
        worker.FullName = dto.FullName.Trim();
        worker.Phone = dto.Phone?.Trim();
        worker.Email = dto.Email?.Trim().ToLowerInvariant();
        worker.Department = dto.Department?.Trim();
        worker.Position = dto.Position;
        worker.WageType = dto.WageType;
        worker.WageRate = dto.WageRate;
        worker.WageCurrency = dto.WageCurrency;
        worker.HiredDate = dto.HiredDate;
        worker.TerminatedDate = dto.TerminatedDate;
        worker.IsActive = dto.IsActive;
        worker.Skills = dto.Skills?.Trim();
        worker.Notes = dto.Notes?.Trim();
        worker.UserId = dto.UserId;

        await db.SaveChangesAsync(ct);
        return worker.ToResponse();
    }
}

public class DeleteWorkerHandler(IApplicationDbContext db) : IRequestHandler<DeleteWorkerCommand>
{
    public async Task Handle(DeleteWorkerCommand command, CancellationToken ct)
    {
        var worker = await db.Workers.FirstOrDefaultAsync(w => w.Id == command.Id, ct)
            ?? throw new NotFoundException(nameof(Worker), command.Id);
        db.Workers.Remove(worker);
        await db.SaveChangesAsync(ct);
    }
}
