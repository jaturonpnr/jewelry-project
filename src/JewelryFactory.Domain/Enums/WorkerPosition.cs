namespace JewelryFactory.Domain.Enums;

/// <summary>
/// Production positions in a jewelry factory.
/// Used both for skill-matching to job stages and HR.
/// </summary>
public enum WorkerPosition
{
    Designer = 1,
    WaxModeler = 2,
    Caster = 3,
    Filer = 4,
    StoneSetter = 5,
    Polisher = 6,
    Plater = 7,
    Assembler = 8,
    QcInspector = 9,
    Packer = 10,
    Supervisor = 11,
    Other = 99
}

public enum WageType
{
    Monthly = 1,
    Daily = 2,
    Hourly = 3,
    PieceRate = 4
}
