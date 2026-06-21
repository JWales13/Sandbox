using System.Collections.Generic;
using UnityEngine;

// What kind of thing an objective tracks. Append new entries at the END
// (enums serialize by integer position, like our other data assets).
public enum ObjectiveType { TalkToNpc, CollectItem, DefeatEnemy }

// One step of a quest. Pure data, embedded in a QuestSO.
[System.Serializable]
public class QuestObjective
{
    public ObjectiveType type = ObjectiveType.TalkToNpc;

    [Tooltip("TalkToNpc / DefeatEnemy: the target's Quest Id (matches the NPC's or enemy's id). CollectItem: leave blank and set Target Item.")]
    public string targetId = "";

    [Tooltip("CollectItem: the item to gather.")]
    public ItemSO targetItem;

    [Tooltip("How many are needed (talk = 1, collect / defeat = N).")]
    public int requiredAmount = 1;

    [Tooltip("CollectItem only: remove the gathered items from the inventory when the quest is handed in (on for fetch/deliver quests).")]
    public bool consumeOnTurnIn = true;

    [Tooltip("Shown in the quest log, e.g. 'Gather Medicinal Herbs'. If blank, a default is generated.")]
    public string description = "";

    // A log line including live progress, e.g. "Gather Herbs  (2/3)".
    public string Label(int current)
    {
        int req = Mathf.Max(1, requiredAmount);
        string body = string.IsNullOrEmpty(description) ? Auto() : description;
        return req > 1
            ? $"{body}  ({Mathf.Min(current, req)}/{req})"
            : $"{body}  {(current >= req ? "✔" : "")}";
    }

    string Auto()
    {
        switch (type)
        {
            case ObjectiveType.CollectItem: return targetItem != null ? $"Gather {targetItem.displayName}" : "Gather item";
            case ObjectiveType.DefeatEnemy: return "Defeat enemies";
            default: return "Speak with " + (string.IsNullOrEmpty(targetId) ? "someone" : targetId);
        }
    }
}

// A reward bundle entry.
[System.Serializable]
public class ItemReward { public ItemSO item; public int amount = 1; }

// A single quest. Pure data — create via Create > Quests > Quest.
// Authored quests are assets; the radiant generator (later) will stamp these out
// from templates, so everything downstream works the same way.
[CreateAssetMenu(menuName = "Quests/Quest", fileName = "NewQuest")]
public class QuestSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique, STABLE id used in saves. Don't change it once saves exist.")]
    public string questId = "quest_id";
    public string title = "New Quest";
    [TextArea] public string summary;
    [Tooltip("Optional: which discipline's questline this belongs to.")]
    public DisciplineSO discipline;

    [Header("Flow")]
    [Tooltip("Auto-accept at game start. Use for an intro quest.")]
    public bool autoStart = false;
    [Tooltip("Optional: this quest only becomes available once the required quest is completed.")]
    public QuestSO requiredQuest;
    [Tooltip("Optional: NPC Quest Id the player must talk to in order to hand this in. Blank = completes automatically when objectives are met.")]
    public string turnInNpcId = "";

    [Header("Objectives")]
    public List<QuestObjective> objectives = new List<QuestObjective>();

    [Header("Rewards")]
    public int rewardCoins = 0;
    public SubskillSO rewardSubskill;
    public int rewardXp = 0;
    public List<ItemReward> rewardItems = new List<ItemReward>();
}