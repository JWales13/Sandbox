using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// Edit-mode tests for the Stats aggregation math (base + flat, then *(1+percent)).
public class StatsTests
{
    // A test stand-in for a modifier source (perks, attributes, equipment...).
    class FakeSource : IStatSource
    {
        public readonly List<StatModifier> mods = new List<StatModifier>();
        public void CollectModifiers(List<StatModifier> into) => into.AddRange(mods);
    }

    Stats stats;

    [SetUp]
    public void Setup() => stats = new GameObject("Stats").AddComponent<Stats>();

    [TearDown]
    public void Teardown() => Object.DestroyImmediate(stats.gameObject);

    [Test]
    public void BonusStat_BaseIsZero()
    {
        Assert.AreEqual(0f, stats.Get(StatType.MeleeDamage), 0.001f);
    }

    [Test]
    public void MultiplierStat_BaseIsOne()
    {
        Assert.AreEqual(1f, stats.Get(StatType.MoveSpeed), 0.001f);
    }

    [Test]
    public void Flat_AddsToBonusStat()
    {
        var src = new FakeSource();
        src.mods.Add(new StatModifier { stat = StatType.MeleeDamage, value = 5, op = StatOp.Flat });
        stats.Register(src);
        Assert.AreEqual(5f, stats.Get(StatType.MeleeDamage), 0.001f);
    }

    [Test]
    public void Flat_OnMultiplierStat_AddsToOne()
    {
        var src = new FakeSource();
        src.mods.Add(new StatModifier { stat = StatType.MoveSpeed, value = 0.2f, op = StatOp.Flat });
        stats.Register(src);
        Assert.AreEqual(1.2f, stats.Get(StatType.MoveSpeed), 0.001f);
    }

    [Test]
    public void Percent_ScalesTotal()
    {
        var src = new FakeSource();
        src.mods.Add(new StatModifier { stat = StatType.MeleeDamage, value = 10, op = StatOp.Flat });
        src.mods.Add(new StatModifier { stat = StatType.MeleeDamage, value = 0.5f, op = StatOp.Percent });
        stats.Register(src);
        // (0 + 10) * (1 + 0.5) = 15
        Assert.AreEqual(15f, stats.Get(StatType.MeleeDamage), 0.001f);
    }
}