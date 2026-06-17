using System.Collections.Generic;
using UnityEngine;

// Plain serializable containers for JsonUtility. Dictionaries aren't supported,
// so progression state is flattened into lists of key/value entries.

[System.Serializable]
public class GameSaveData
{
    public ProgressionSaveData progression = new ProgressionSaveData();
    public InventorySaveData inventory = new InventorySaveData();
    public Vector3 playerPos;
    public Vector3 playerEuler;
}

[System.Serializable]
public class InventorySaveData
{
    public List<ItemStackData> slots = new List<ItemStackData>();
}

[System.Serializable]
public class ItemStackData
{
    public string itemName;   // ItemSO asset name; empty = empty slot
    public int count;
}

[System.Serializable]
public class ProgressionSaveData
{
    public int characterLevel = 1;
    public int characterXpIntoLevel;
    public int attributePoints;

    public List<IntEntry> attributes = new List<IntEntry>();        // key = (int)AttributeType
    public List<StringIntEntry> perkPoints = new List<StringIntEntry>();    // key = discipline asset name
    public List<StringIntEntry> subskillLevels = new List<StringIntEntry>(); // key = subskill asset name
    public List<StringIntEntry> subskillXp = new List<StringIntEntry>();     // key = subskill asset name
    public List<string> unlockedPerks = new List<string>();         // perk asset names
}

[System.Serializable]
public class IntEntry
{
    public int key;
    public int value;
}

[System.Serializable]
public class StringIntEntry
{
    public string key;
    public int value;
}