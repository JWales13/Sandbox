using System.Collections.Generic;
using UnityEngine;

public enum EquipSlot { Weapon, Head, Chest, Legs, Accessory }

// What ACTION a held item enables — gates tool-based gathering (mining, logging).
// This is about utility, NOT weapon class (a future "Battle Axe" is a weapon, not
// a Hatchet). Keep entries in place / append new ones at the END (enums serialize
// by their integer position, so reordering would corrupt existing assets).
public enum ToolType { None, Sword, Hatchet, Pickaxe, Hoe, FishingRod }

// An equippable item. Inherits everything from ItemSO (name, icon, prices...) and
// adds a slot + the stat modifiers it grants while worn.
// Create via Create > Items > Equipment. Keep these under a Resources folder.
[CreateAssetMenu(menuName = "Items/Equipment", fileName = "NewEquipment")]
public class EquipmentSO : ItemSO
{
    public EquipSlot slot;
    public ToolType toolType = ToolType.None;     // Weapon-slot items can be tools
    public List<StatModifier> modifiers = new List<StatModifier>();

    [Header("Held visual (Weapon slot)")]
    public GameObject worldModel;                 // prefab shown in the hand when equipped
    public Vector3 gripPosition;
    public Vector3 gripEuler;
    public Vector3 gripScale = Vector3.one;
}