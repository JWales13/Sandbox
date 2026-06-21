using UnityEngine;

// An NPC that takes part in quests (e.g. the Guild Master). Talking to it tells
// the quest system, which advances any "talk to X" objective and turns in any
// quest waiting to be handed in here. Inherits interaction plumbing from
// Interactable; shows flavor lines through the existing DialogueUI.
public class QuestGiver : Interactable
{
    [SerializeField] string npcName = "Guild Master";

    [Tooltip("STABLE id used by quests' TalkToNpc / Turn-in targets, e.g. 'guild_master'.")]
    [SerializeField] string npcId = "guild_master";

    [Tooltip("Default chatter.")]
    [TextArea(2, 4)] [SerializeField] string[] greetingLines;

    [Tooltip("Shown instead of the greeting when talking advanced or handed in a quest.")]
    [TextArea(2, 4)] [SerializeField] string[] questLines;

    void Reset() { prompt = "talk"; }

    public override void Interact(GameObject interactor)
    {
        bool questEvent = QuestManager.Instance != null && QuestManager.Instance.NotifyNpcTalked(npcId);

        var lines = (questEvent && questLines != null && questLines.Length > 0) ? questLines : greetingLines;
        if (DialogueUI.Instance != null && lines != null && lines.Length > 0)
            DialogueUI.Instance.StartDialogue(npcName, lines, transform);
    }
}