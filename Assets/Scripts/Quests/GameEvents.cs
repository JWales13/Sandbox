using System;

// A tiny global event bus that decouples quests from the systems that feed them.
// Combat calls GameEvents.EnemyWasKilled(id); the quest system listens — neither
// needs a reference to the other. (NPC talk is handled directly via
// QuestManager.NotifyNpcTalked, since a quest NPC is inherently quest-aware.)
public static class GameEvents
{
    public static event Action<string> EnemyKilled;

    public static void EnemyWasKilled(string enemyId)
    {
        if (!string.IsNullOrEmpty(enemyId)) EnemyKilled?.Invoke(enemyId);
    }
}