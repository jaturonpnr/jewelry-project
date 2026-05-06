using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// Factory worker. Distinct from system User — a User logs in; a Worker performs production tasks.
/// A Worker can OPTIONALLY link to a User if they have system access.
/// </summary>
public class Worker : AuditableEntity
{
    public required string EmployeeCode { get; set; }   // unique, e.g. EMP-0001
    public required string FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Department { get; set; }

    public WorkerPosition Position { get; set; } = WorkerPosition.Other;
    public WageType WageType { get; set; } = WageType.Monthly;
    public decimal WageRate { get; set; }                // amount per WageType unit
    public Currency WageCurrency { get; set; } = Currency.THB;

    public DateTime HiredDate { get; set; }
    public DateTime? TerminatedDate { get; set; }
    public bool IsActive { get; set; } = true;

    public string? Skills { get; set; }                  // free text or JSON
    public string? Notes { get; set; }

    // Optional link to system user
    public Guid? UserId { get; set; }
    public User? User { get; set; }
}
