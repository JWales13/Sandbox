using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemSO item;
    public int count;
    public bool IsEmpty => item == null || count <= 0;
}

// The player's inventory: a fixed set of stacking slots.
// Other systems use Inventory.Instance.Add(...) / Remove(...) / CountOf(...).
public class Inventory : MonoBehaviour, ISaveable
{
    public static Inventory Instance { get; private set; }

    public string SaveId => "inventory";
    public string WriteState() => JsonUtility.ToJson(CaptureData());
    public void ReadState(string data) => RestoreData(JsonUtility.FromJson<InventorySaveData>(data));

    public int slotCount = 24;
    public List<InventorySlot> slots = new List<InventorySlot>();
    public event Action OnChanged;

    // Item assets found under any Resources folder, keyed by asset name (for save/load).
    readonly Dictionary<string, ItemSO> itemByName = new Dictionary<string, ItemSO>();

    void Awake()
    {
        Instance = this;
        foreach (var it in Resources.LoadAll<ItemSO>(""))
            itemByName[it.name] = it;
        while (slots.Count < slotCount) slots.Add(new InventorySlot());
    }

    // Adds items, stacking into existing stacks then empty slots.
    // Returns the leftover amount that didn't fit (0 = all added).
    public int Add(ItemSO item, int amount)
    {
        if (item == null || amount <= 0) return amount;

        foreach (var s in slots)
        {
            if (amount <= 0) break;
            if (s.item == item && s.count < item.maxStack)
            {
                int add = Mathf.Min(item.maxStack - s.count, amount);
                s.count += add;
                amount -= add;
            }
        }

        foreach (var s in slots)
        {
            if (amount <= 0) break;
            if (s.IsEmpty)
            {
                s.item = item;
                int add = Mathf.Min(item.maxStack, amount);
                s.count = add;
                amount -= add;
            }
        }

        OnChanged?.Invoke();
        return amount;
    }

    public bool Remove(ItemSO item, int amount)
    {
        if (item == null || amount <= 0 || CountOf(item) < amount) return false;

        foreach (var s in slots)
        {
            if (amount <= 0) break;
            if (s.item == item)
            {
                int take = Mathf.Min(s.count, amount);
                s.count -= take;
                amount -= take;
                if (s.count <= 0) s.item = null;
            }
        }

        OnChanged?.Invoke();
        return true;
    }

    public int CountOf(ItemSO item)
    {
        int total = 0;
        foreach (var s in slots)
            if (s.item == item) total += s.count;
        return total;
    }

    // ---------- Save / load ----------

    public InventorySaveData CaptureData()
    {
        var data = new InventorySaveData();
        foreach (var s in slots)
            data.slots.Add(new ItemStackData
            {
                itemName = s.IsEmpty ? "" : s.item.name,
                count = s.IsEmpty ? 0 : s.count
            });
        return data;
    }

    public void RestoreData(InventorySaveData data)
    {
        if (data == null) return;

        while (slots.Count < data.slots.Count) slots.Add(new InventorySlot());

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < data.slots.Count
                && !string.IsNullOrEmpty(data.slots[i].itemName)
                && itemByName.TryGetValue(data.slots[i].itemName, out var item))
            {
                slots[i].item = item;
                slots[i].count = data.slots[i].count;
            }
            else
            {
                slots[i].item = null;
                slots[i].count = 0;
            }
        }

        OnChanged?.Invoke();
    }
}