using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Central derived-stats aggregator. Collects modifiers from every IStatSource
// (attributes, perks, equipment, buffs...) and exposes one Get(StatType).
// Final = (base + sum of Flat) * (1 + sum of Percent).
public class Stats : MonoBehaviour
{
    public static Stats Instance { get; private set; }
    public event Action OnChanged;

    readonly List<IStatSource> sources = new List<IStatSource>();
    readonly Dictionary<StatType, float> values = new Dictionary<StatType, float>();
    readonly List<StatModifier> scratch = new List<StatModifier>();

    void Awake() { Instance = this; }

    void Start()
    {
        sources.Clear();
        sources.AddRange(FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude).OfType<IStatSource>());

        // Recompute whenever progression (attributes/perks) changes.
        if (PlayerProgression.Instance != null) PlayerProgression.Instance.OnChanged += Recalculate;
        Recalculate();
    }

    void OnDestroy()
    {
        if (PlayerProgression.Instance != null) PlayerProgression.Instance.OnChanged -= Recalculate;
    }

    // Equipment/buffs added at runtime call these.
    public void Register(IStatSource s)
    {
        if (s != null && !sources.Contains(s)) { sources.Add(s); Recalculate(); }
    }

    public void Unregister(IStatSource s)
    {
        if (sources.Remove(s)) Recalculate();
    }

    public void Recalculate()
    {
        scratch.Clear();
        foreach (var s in sources) s?.CollectModifiers(scratch);

        values.Clear();
        foreach (StatType st in Enum.GetValues(typeof(StatType)))
        {
            float flat = BaseValue(st);
            float percent = 0f;
            foreach (var m in scratch)
                if (m.stat == st)
                {
                    if (m.op == StatOp.Percent) percent += m.value;
                    else flat += m.value;
                }
            values[st] = flat * (1f + percent);
        }

        OnChanged?.Invoke();
    }

    public float Get(StatType stat) => values.TryGetValue(stat, out var v) ? v : BaseValue(stat);

    // Multiplier-style stats start at 1; additive/bonus stats start at 0.
    static float BaseValue(StatType s)
    {
        switch (s)
        {
            case StatType.MoveSpeed:
            case StatType.CropYield:
            case StatType.SellPrice:
            case StatType.HarvestSpeed:
                return 1f;
            default:
                return 0f;
        }
    }
}