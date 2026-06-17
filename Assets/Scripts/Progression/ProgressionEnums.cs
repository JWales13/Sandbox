// Global character attributes (effects wired later; tracked as numbers for now).
public enum AttributeType
{
    Strength,
    Intelligence,
    Stamina,
    Charm,
    Luck,
    Vitality    // scales max health (keep new entries at the END so old saves stay valid)
}

// Granular things that perks can modify. Add entries as systems are built.
public enum StatType
{
    MoveSpeed,
    MeleeDamage,
    CarryCapacity,
    CropYield,
    HarvestSpeed,
    SellPrice
}