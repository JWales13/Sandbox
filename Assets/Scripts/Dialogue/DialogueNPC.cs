using UnityEngine;

// An NPC the player can talk to. Inherits the interaction plumbing from
// Interactable; all it adds is a name and some lines. Later this is where
// you'll branch dialogue, give quests, open the guild board, etc.
public class DialogueNPC : Interactable
{
    [SerializeField] string npcName = "Villager";

    [TextArea(2, 4)]
    [SerializeField] string[] lines;

    // Sets a sensible default prompt when you first add the component.
    void Reset() { prompt = "talk"; }

    public override void Interact(GameObject interactor)
    {
        if (DialogueUI.Instance != null)
            DialogueUI.Instance.StartDialogue(npcName, lines, transform);
        else
            Debug.LogWarning("No DialogueUI found in the scene.");
    }
}