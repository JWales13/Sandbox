using System.Collections.Generic;

// On-disk payload for the quest system (JsonUtility-friendly: no dictionaries).
[System.Serializable]
public class QuestSaveData
{
    public List<string> completed = new List<string>();
    public List<ActiveQuestData> active = new List<ActiveQuestData>();
}

[System.Serializable]
public class ActiveQuestData
{
    public string questId;
    public List<int> progress = new List<int>();
}