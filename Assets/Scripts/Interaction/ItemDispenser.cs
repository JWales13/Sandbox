using UnityEngine;

// Gives the player items on interact (e.g. a seed barrel, a free chest).
// A simple reusable Interactable.
public class ItemDispenser : Interactable
{
    [SerializeField] ItemSO item;
    [SerializeField] int amount = 5;

    void Reset() { prompt = "take"; }

    public override void Interact(GameObject interactor)
    {
        if (Inventory.Instance != null && item != null)
            Inventory.Instance.Add(item, amount);
    }
}