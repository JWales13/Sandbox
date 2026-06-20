using UnityEngine;

// A reusable gathering node: mine ore, chop logs, forage, etc. Interacting yields
// an item, grants a subskill's XP, then the node depletes and respawns (or is
// destroyed). Generalizes the crop/dispenser pattern — just data, no per-type code.
public class Gatherable : Interactable
{
    [Header("Yield")]
    [SerializeField] ItemSO item;
    [SerializeField] int amount = 1;

    [Header("Reward")]
    [SerializeField] SubskillSO subskill;   // e.g. Mining, Logging, Foraging
    [SerializeField] int xpReward = 10;

    [Header("Respawn")]
    [Tooltip("Seconds until it comes back. 0 or less = removed after one gather.")]
    [SerializeField] float respawnSeconds = 8f;

    [Header("Tool")]
    [Tooltip("Tool the player must have equipped (None = any).")]
    [SerializeField] ToolType requiredTool = ToolType.None;

    bool depleted;

    void Reset() { prompt = "gather"; }

    public override string GetPrompt()
    {
        if (requiredTool != ToolType.None && !HasTool())
            return $"{prompt} (needs {requiredTool})";
        return prompt;
    }

    bool HasTool()
    {
        if (requiredTool == ToolType.None) return true;
        return Equipment.Instance != null && Equipment.Instance.CurrentTool == requiredTool;
    }

    public override void Interact(GameObject interactor)
    {
        if (depleted) return;
        if (!HasTool()) return;   // wrong/no tool equipped

        if (PlayerProgression.Instance != null && subskill != null)
            PlayerProgression.Instance.AddSubskillXP(subskill, xpReward);
        if (Inventory.Instance != null && item != null)
            Inventory.Instance.Add(item, amount);

        if (respawnSeconds <= 0f) { Destroy(gameObject); return; }

        SetAvailable(false);
        Invoke(nameof(Respawn), respawnSeconds);
    }

    void Respawn() => SetAvailable(true);

    // Toggle renderers + colliders (not the GameObject, so the respawn timer runs).
    void SetAvailable(bool on)
    {
        depleted = !on;
        foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = on;
        foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = on;
    }
}