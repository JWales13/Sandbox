using System.Collections.Generic;
using UnityEngine;

public enum EquipSlot { Weapon, Head, Chest, Legs, Accessory }

// An equippable item. Inherits everything from ItemSO (name, icon, prices...) and
// adds a slot + the stat modifiers it grants while worn.
// Create via Create > Items > Equipment. Keep these under a Resources folder.
[CreateAssetMenu(menuName = "Items/Equipment", fileName = "NewEquipment")]
public class EquipmentSO : ItemSO
{
    public EquipSlot slot;
    public List<StatModifier> modifiers = new List<StatModifier>();
}