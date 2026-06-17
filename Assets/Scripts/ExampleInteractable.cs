using UnityEngine;
 
// A throwaway test interactable: changes color and logs when used.
// Put it on a Cube to confirm the system works, then delete it later.
// Notice how little code a new interaction takes — this is the pattern
// your NPCs, crops, and ore nodes will follow.
public class ExampleInteractable : Interactable
{
    public Color usedColor = Color.green;
    Renderer rend;
 
    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
    }
 
    public override void Interact(GameObject interactor)
    {
        Debug.Log($"{name} was used by {interactor.name}");
        if (rend != null) rend.material.color = usedColor;
    }
}
