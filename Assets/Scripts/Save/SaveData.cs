using System.Collections.Generic;
using UnityEngine;

// On-disk save: a flat list of id -> json entries, one per ISaveable.
[System.Serializable]
public class SaveFile
{
    public List<SaveEntry> entries = new List<SaveEntry>();
}

[System.Serializable]
public class SaveEntry
{
    public string id;
    public string json;
}

// ---- Per-system serializable payloads (each ISaveable JSON-serializes one of these) ----

[System.Serializable]
public class ProgressionSaveData
{
    public int characterLevel = 1;
    public int characterXpIntoLevel;
    public int attributePoints;

    public List<IntEntry> attributes = new List<IntEntry>();
    public List<StringIntEntry> perkPoints = new List<StringIntEntry>();
    public List<StringIntEntry> subskillLevels = new List<StringIntEntry>();
    public List<StringIntEntry> subskillXp = new List<StringIntEntry>();
    public List<string> unlockedPerks = new List<string>();
}

[System.Serializable]
public class InventorySaveData
{
    public List<ItemStackData> slots = new List<ItemStackData>();
}

[System.Serializable]
public class ItemStackData
{
    public string itemName;
    public int count;
}

[System.Serializable]
public class FarmPlotSaveData
{
    public int state;
    public float growTimer;
}

[System.Serializable]
public class TransformState
{
    public Vector3 pos;
    public Vector3 euler;
}

[System.Serializable]
public class EquipmentSaveData
{
    public List<SlotItem> slots = new List<SlotItem>();
}

[System.Serializable]
public class SlotItem
{
    public int slot;
    public string itemName;
}

[System.Serializable]
public class IntEntry { public int key; public int value; }

[System.Serializable]
public class StringIntEntry { public string key; public int value; }