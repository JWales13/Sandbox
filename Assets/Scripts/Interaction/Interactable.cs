using UnityEngine;
 
// Base class for ANYTHING the player can interact with.
// Make an NPC, crop, ore node, sign, forge, etc. by inheriting from this
// and overriding Interact(). The PlayerInteractor handles detection + input.
public abstract class Interactable : MonoBehaviour
{
    [Tooltip("Short verb shown in the prompt, e.g. 'talk', 'harvest', 'mine'.")]
    public string prompt = "interact";
 
    // What happens when the player interacts. 'interactor' is the player object.
    public abstract void Interact(GameObject interactor);
 
    // Override if a subclass needs a dynamic prompt (e.g. "open" vs "close").
    public virtual string GetPrompt() => prompt;
}
