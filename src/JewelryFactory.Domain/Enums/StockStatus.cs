namespace JewelryFactory.Domain.Enums;

public enum StockStatus
{
    InStock = 1,
    Reserved = 2,        // allocated to a work/sales order
    InProduction = 3,    // currently being used
    Sold = 4,
    WrittenOff = 5,      // damaged/lost
    Returned = 6
}

public enum StoneClarity
{
    FL = 1,    // Flawless
    IF = 2,    // Internally Flawless
    VVS1 = 3, VVS2 = 4,
    VS1 = 5,  VS2 = 6,
    SI1 = 7,  SI2 = 8,
    I1 = 9,   I2 = 10, I3 = 11
}

public enum StoneColor
{
    // Diamond D-Z scale
    D = 1, E = 2, F = 3,
    G = 4, H = 5, I = 6, J = 7,
    K = 8, L = 9, M = 10,
    Fancy = 99   // colored stones
}

public enum StoneShape
{
    Round = 1,
    Princess = 2,
    Cushion = 3,
    Oval = 4,
    Emerald = 5,
    Marquise = 6,
    Pear = 7,
    Heart = 8,
    Asscher = 9,
    Radiant = 10,
    Baguette = 11,
    Other = 99
}

public enum StoneCertAuthority
{
    None = 0,
    GIA = 1,
    IGI = 2,
    AGS = 3,
    HRD = 4,
    GUBELIN = 5,
    SSEF = 6,
    Other = 99
}

public enum StockMovementType
{
    Receipt = 1,         // incoming from supplier
    Issue = 2,           // outgoing to production
    Adjustment = 3,      // manual correction (+ or -)
    Transfer = 4,        // location change
    Reserve = 5,         // allocate but don't deduct
    Release = 6,         // un-reserve
    WriteOff = 7,        // loss/damage
    Return = 8           // return from production
}

public enum InventoryItemType
{
    RawMaterial = 1,
    StoneItem = 2,
    StoneParcel = 3,
    FinishedGoods = 4
}
