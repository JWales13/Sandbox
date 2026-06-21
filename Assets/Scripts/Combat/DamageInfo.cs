using UnityEngine;

// How damage is categorized. Append new entries at the END (used by resistances
// later). Physical covers melee/arrows; magic schools come later.
public enum DamageType { Physical, Magic, Fire, Ice }

// A single packet of damage. Every attack style (melee, arrow, spell, trap) fills
// one of these and hands it to IDamageable.TakeDamage, so targets and feedback
// treat all sources the same way.
public struct DamageInfo
{
    public int amount;
    public DamageType type;
    public Vector3 hitPoint;     // where it landed (for VFX / floating numbers); zero = unspecified
    public Vector3 knockback;    // world-space displacement to shove the target
    public GameObject source;    // who dealt it

    // Convenience for sources that only care about a number (enemy melee, traps...).
    public static DamageInfo Simple(int amount, GameObject source = null) => new DamageInfo
    {
        amount = amount,
        type = DamageType.Physical,
        source = source
    };
}