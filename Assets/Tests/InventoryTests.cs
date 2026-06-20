using NUnit.Framework;
using UnityEngine;

// Edit-mode tests for the inventory's stacking/removal logic.
public class InventoryTests
{
    Inventory inv;
    ItemSO item;

    [SetUp]
    public void Setup()
    {
        inv = new GameObject("Inv").AddComponent<Inventory>();
        inv.slotCount = 4;
        // Awake doesn't run in edit-mode tests, so create the slots manually.
        for (int i = 0; i < inv.slotCount; i++) inv.slots.Add(new InventorySlot());

        item = ScriptableObject.CreateInstance<ItemSO>();
        item.name = "TestItem";
        item.maxStack = 5;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(inv.gameObject);
        Object.DestroyImmediate(item);
    }

    [Test]
    public void Add_WithinStack_NoLeftover()
    {
        int leftover = inv.Add(item, 3);
        Assert.AreEqual(0, leftover);
        Assert.AreEqual(3, inv.CountOf(item));
    }

    [Test]
    public void Add_BeyondCapacity_ReportsLeftover()
    {
        // 4 slots * maxStack 5 = 20 capacity.
        int leftover = inv.Add(item, 22);
        Assert.AreEqual(2, leftover);
        Assert.AreEqual(20, inv.CountOf(item));
    }

    [Test]
    public void Remove_ReducesCount()
    {
        inv.Add(item, 5);
        Assert.IsTrue(inv.Remove(item, 2));
        Assert.AreEqual(3, inv.CountOf(item));
    }

    [Test]
    public void Remove_NotEnough_Fails()
    {
        inv.Add(item, 1);
        Assert.IsFalse(inv.Remove(item, 5));
        Assert.AreEqual(1, inv.CountOf(item));
    }
}