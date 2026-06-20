using UnityEngine;

// A crop you harvest for XP. Inherits interaction from Interactable.
// On harvest it grants XP to a subskill (e.g. Cultivation), then hides and
// regrows after a delay. This is the real version of the debug "T" key.
public class HarvestableCrop : Interactable
{
    [Header("Reward")]
    [SerializeField] SubskillSO subskill;     // e.g. Cultivation
    [SerializeField] int xpReward = 40;

    [Header("Yield")]
    [SerializeField] ItemSO yieldItem;        // what the player receives (e.g. Wheat)
    [SerializeField] int yieldAmount = 1;

    [Header("Regrow")]
    [Tooltip("Seconds until the crop regrows. 0 or less = harvested once, then destroyed.")]
    [SerializeField] float regrowSeconds = 5f;

    void Reset() { prompt = "harvest"; }

    public override void Interact(GameObject interactor)
    {
        if (PlayerProgression.Instance != null && subskill != null)
            PlayerProgression.Instance.AddSubskillXP(subskill, xpReward);

        if (Inventory.Instance != null && yieldItem != null)
            Inventory.Instance.Add(yieldItem, yieldAmount);

        if (regrowSeconds <= 0f) { Destroy(gameObject); return; }

        SetReady(false);                         // hide + can't be re-harvested
        Invoke(nameof(Regrow), regrowSeconds);
    }

    void Regrow() => SetReady(true);

    // Toggle renderers + colliders (not the GameObject itself, so timers keep running).
    void SetReady(bool ready)
    {
        foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = ready;
        foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = ready;
    }
}