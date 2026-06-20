using System;
using System.Collections.Generic;
using UnityEngine;

// Holds one equipped item per slot. Is an IStatSource (so worn gear feeds the
// Stats pipeline) and ISaveable. Equip/unequip swap items with the inventory.
public class Equipment : MonoBehaviour, IStatSource, ISaveable
{
    public static Equipment Instance { get; private set; }
    public event Action OnChanged;

    public ItemDatabaseSO database;   // resolves item assets by name (for save/load)

    readonly Dictionary<EquipSlot, EquipmentSO> equipped = new Dictionary<EquipSlot, EquipmentSO>();

    void Awake() { Instance = this; }

    public EquipmentSO Get(EquipSlot slot) => equipped.TryGetValue(slot, out var v) ? v : null;

    public EquipmentSO CurrentWeapon => Get(EquipSlot.Weapon);
    public ToolType CurrentTool => CurrentWeapon != null ? CurrentWeapon.toolType : ToolType.None;

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
        if (data != null && database != null)
            foreach (var s in data.slots)
                if (database.GetByName(s.itemName) is EquipmentSO eq)
                    equipped[(EquipSlot)s.slot] = eq;
        Changed();
    }
}