using System.Collections.Generic;

// How a modifier combines: Flat is added, Percent stacks multiplicatively.
public enum StatOp { Flat, Percent }

// One contribution to a stat. Perks, attributes, equipment, and buffs all emit these.
[System.Serializable]
public struct StatModifier
{
    public StatType stat;
    public float value;
    public StatOp op;
}

// Anything that contributes stat modifiers (attributes, perks, equipment, buffs).
// Stats collects every IStatSource and aggregates them.
public interface IStatSource
{
    void CollectModifiers(List<StatModifier> into);
}