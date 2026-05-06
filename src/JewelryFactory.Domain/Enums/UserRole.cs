namespace JewelryFactory.Domain.Enums;

/// <summary>
/// System roles per CLAUDE.md §9.
/// Stored as string in DB for readability and forward-compatibility.
/// </summary>
public enum UserRole
{
    Admin = 1,
    Manager = 2,
    Sales = 3,
    Production = 4,
    Qc = 5,
    Designer = 6,
    Worker = 7
}
