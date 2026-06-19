using System.Collections.Generic;
using UnityEngine;

// Turns attribute points into stat modifiers for the Stats pipeline.
// Soft caps (diminishing returns) live here; the pipeline just sums contributions.
public class AttributeEffects : MonoBehaviour, IStatSource
{
    public PlayerProgression progression;

    [Header("Strength → melee damage (flat per point)")]
    public float strengthPerDamage = 2f;

    [Header("Vitality → max health (flat per point)")]
    public float healthPerVitality = 10f;

    [Header("Agility → move-speed bonus (soft-capped fraction)")]
    public float agilityMaxBonus = 0.4f;
    public float agilityK = 20f;

    [Header("Defense → damage reduction (soft-capped fraction)")]
    public float defenseMaxReduction = 0.8f;
    public float defenseK = 50f;

    void Start()
    {
        if (progression == null) progression = PlayerProgression.Instance;
    }

    public void CollectModifiers(List<StatModifier> into)
    {
        var p = progression != null ? progression : PlayerProgression.Instance;
        if (p == null) return;

        int str = p.GetAttribute(AttributeType.Strength);
        int vit = p.GetAttribute(AttributeType.Vitality);
        int agi = p.GetAttribute(AttributeType.Agility);
        int def = p.GetAttribute(AttributeType.Defense);

        Add(into, StatType.MeleeDamage, str * strengthPerDamage);
        Add(into, StatType.MaxHealthBonus, vit * healthPerVitality);
        Add(into, StatType.MoveSpeed, SoftCap(agi, agilityMaxBonus, agilityK));        // added onto base 1
        Add(into, StatType.DamageReduction, SoftCap(def, defenseMaxReduction, defenseK));
    }

    static void Add(List<StatModifier> into, StatType stat, float value)
    {
        if (value != 0f) into.Add(new StatModifier { stat = stat, value = value, op = StatOp.Flat });
    }

    // value = max * points / (points + K)  — approaches max, diminishing per point.
    static float SoftCap(int points, float max, float k) => max * points / (points + k);
}