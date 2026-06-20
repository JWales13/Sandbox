using NUnit.Framework;
using UnityEngine;

// Edit-mode test: the inventory survives a save -> wipe -> load round trip,
// resolving items through an ItemDatabase (as the real save flow does).
public class SaveRoundTripTests
{
    [Test]
    public void Inventory_SaveThenLoad_PreservesItems()
    {
        var db = ScriptableObject.CreateInstance<ItemDatabaseSO>();
        var item = ScriptableObject.CreateInstance<ItemSO>();
        item.name = "Wood";
        item.maxStack = 99;
        db.items.Add(item);

        var inv = new GameObject("Inv").AddComponent<Inventory>();
        inv.slotCount = 4;
        for (int i = 0; i < inv.slotCount; i++) inv.slots.Add(new InventorySlot());
        inv.database = db;

        inv.Add(item, 7);
        string json = inv.WriteState();

        // Wipe, confirm empty, then load.
        inv.RestoreData(new InventorySaveData());
        Assert.AreEqual(0, inv.CountOf(item));

        inv.ReadState(json);
        Assert.AreEqual(7, inv.CountOf(item));

        Object.DestroyImmediate(inv.gameObject);
        Object.DestroyImmediate(item);
        Object.DestroyImmediate(db);
    }
}