using System;
using System.Collections.Generic;
using UnityEngine;

// Holds one equipped item per slot. Is an IStatSource (so worn gear feeds the
// Stats pipeline) and ISaveable. Equip/unequip swap items with the inventory.
public class Equipment : MonoBehaviour, IStatSource, ISaveable
{
    public static Equipment Instance { get; private set; }
    public event Action OnChanged;

    readonly Dictionary<EquipSlot, EquipmentSO> equipped = new Dictionary<EquipSlot, EquipmentSO>();
    readonly Dictionary<string, ItemSO> itemByName = new Dictionary<string, ItemSO>();

    void Awake()
    {
        Instance = this;
        foreach (var it in Resources.LoadAll<ItemSO>("")) itemByName[it.name] = it;
    }

    public EquipmentSO Get(EquipSlot slot) => equipped.TryGetValue(slot, out var v) ? v : null;

    public void Equip(EquipmentSO item)
    {
        if (item == null || Inventory.Instance == null) return;
        if (!Inventory.Instance.Remove(item, 1)) return;                 // must own it

        if (equipped.TryGetValue(item.slot, out var cur) && cur != null)
            Inventory.Instance.Add(cur, 1);                              // previous piece back to bag

        equipped[item.slot] = item;
        Changed();
    }

    public void Unequip(EquipSlot slot)
    {
        if (equipped.TryGetValue(slot, out var cur) && cur != null && Inventory.Instance != null)
        {
            Inventory.Instance.Add(cur, 1);
            equipped[slot] = null;
            Changed();
        }
    }

    void Changed()
    {
        OnChanged?.Invoke();
        if (Stats.Instance != null) Stats.Instance.Recalculate();
    }

    public void CollectModifiers(List<StatModifier> into)
    {
        foreach (var kv in equipped)
            if (kv.Value != null)
                foreach (var m in kv.Value.modifiers) into.Add(m);
    }

    // ---- Save ----
    public string SaveId => "equipment";

    public string WriteState()
    {
        var data = new EquipmentSaveData();
        foreach (var kv in equipped)
            if (kv.Value != null)
                data.slots.Add(new SlotItem { slot = (int)kv.Key, itemName = kv.Value.name });
        return JsonUtility.ToJson(data);
    }

    public void ReadState(string json)
    {
        equipped.Clear();
        var data = JsonUtility.FromJson<EquipmentSaveData>(json);
        if (data != null)
            foreach (var s in data.slots)
                if (itemByName.TryGetValue(s.itemName, out var it) && it is EquipmentSO eq)
                    equipped[(EquipSlot)s.slot] = eq;
        Changed();
    }
}