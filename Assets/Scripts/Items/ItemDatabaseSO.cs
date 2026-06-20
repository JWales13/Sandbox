using System.Collections.Generic;
using UnityEngine;

// A registry of every item in the game, used to resolve item assets by name
// (for save/load) without the magic "Resources" folder. Items can then live
// anywhere under Data/Items, organized semantically.
//
// Create via Create > Items > Item Database. Right-click the asset →
// "Rescan All Items" to auto-populate from the project (editor only).
[CreateAssetMenu(menuName = "Items/Item Database", fileName = "ItemDatabase")]
public class ItemDatabaseSO : ScriptableObject
{
    public List<ItemSO> items = new List<ItemSO>();

    Dictionary<string, ItemSO> byName;

    public ItemSO GetByName(string itemName)
    {
        if (byName == null) BuildLookup();
        return byName.TryGetValue(itemName, out var it) ? it : null;
    }

    void BuildLookup()
    {
        byName = new Dictionary<string, ItemSO>();
        foreach (var it in items)
            if (it != null) byName[it.name] = it;
    }

#if UNITY_EDITOR
    [ContextMenu("Rescan All Items")]
    public void Rescan()
    {
        items.Clear();
        foreach (var guid in UnityEditor.AssetDatabase.FindAssets("t:ItemSO"))
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var it = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemSO>(path);
            if (it != null) items.Add(it);
        }
        byName = null;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"ItemDatabase: found {items.Count} items.");
    }
#endif
}