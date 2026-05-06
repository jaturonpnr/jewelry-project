namespace JewelryFactory.Domain.Enums;

public enum MaterialCategory
{
    Metal = 1,           // Gold, Silver, Platinum, Palladium
    PreciousStone = 2,   // Diamond, Ruby, Sapphire, Emerald
    SemiPreciousStone = 3,
    Finding = 4,         // Clasps, jump rings, posts
    Consumable = 5       // Wax, casting plaster, polish
}

public enum MaterialUnit
{
    Gram = 1,    // metals
    Carat = 2,   // stones
    Piece = 3,   // findings
    Liter = 4,   // chemicals
    Kilogram = 5
}
