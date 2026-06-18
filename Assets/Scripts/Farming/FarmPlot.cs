using UnityEngine;

// A tile you farm. Interact to plant (consumes a seed), wait while it grows
// (the crop visual scales up), then interact again to harvest produce + XP.
public class FarmPlot : Interactable
{
    public CropDataSO crop;
    [Tooltip("Child object (the plant mesh) that is shown and scaled as it grows.")]
    public Transform cropVisual;

    enum State { Empty, Growing, Ready }
    State state = State.Empty;
    float growTimer;

    // ---- Save/load: position-based key + state capture/restore ----
    public string SaveKey =>
        $"{Mathf.RoundToInt(transform.position.x)}_{Mathf.RoundToInt(transform.position.y)}_{Mathf.RoundToInt(transform.position.z)}";
    public int StateIndex => (int)state;
    public float GrowTimer => growTimer;

    public void RestoreState(int stateIndex, float timer)
    {
        state = (State)stateIndex;
        growTimer = timer;

        if (state == State.Empty)
        {
            ShowVisual(false);
        }
        else
        {
            ShowVisual(true);
            if (crop != null && cropVisual != null)
            {
                float t = state == State.Ready
                    ? 1f
                    : Mathf.Clamp01(growTimer / Mathf.Max(0.01f, crop.growthSeconds));
                cropVisual.localScale = Vector3.Lerp(crop.sproutScale, crop.fullScale, t);
            }
        }
    }

    void Start()
    {
        state = State.Empty;
        ShowVisual(false);
    }

    public override string GetPrompt()
    {
        switch (state)
        {
            case State.Empty:   return crop != null ? $"plant {crop.displayName}" : "plant";
            case State.Growing: return "growing...";
            case State.Ready:   return "harvest";
        }
        return prompt;
    }

    void Update()
    {
        if (state != State.Growing || crop == null) return;

        growTimer += Time.deltaTime;
        float t = Mathf.Clamp01(growTimer / Mathf.Max(0.01f, crop.growthSeconds));
        if (cropVisual != null)
            cropVisual.localScale = Vector3.Lerp(crop.sproutScale, crop.fullScale, t);

        if (t >= 1f) state = State.Ready;
    }

    public override void Interact(GameObject interactor)
    {
        switch (state)
        {
            case State.Empty:   TryPlant(); break;
            case State.Growing: break;            // not ready yet
            case State.Ready:   Harvest();  break;
        }
    }

    void TryPlant()
    {
        if (crop == null) return;

        // Consume a seed if this crop needs one.
        if (crop.seedItem != null)
        {
            if (Inventory.Instance == null || !Inventory.Instance.Remove(crop.seedItem, 1))
                return; // no seeds -> do nothing
        }

        state = State.Growing;
        growTimer = 0f;
        ShowVisual(true);
        if (cropVisual != null) cropVisual.localScale = crop.sproutScale;
    }

    void Harvest()
    {
        if (crop != null)
        {
            if (PlayerProgression.Instance != null && crop.subskill != null)
                PlayerProgression.Instance.AddSubskillXP(crop.subskill, crop.xpOnHarvest);

            if (Inventory.Instance != null && crop.produceItem != null)
            {
                int amount = crop.produceAmount;
                // CropYield perks increase the harvest.
                if (PlayerProgression.Instance != null)
                    amount = Mathf.RoundToInt(amount * (1f + PlayerProgression.Instance.GetStat(StatType.CropYield)));
                Inventory.Instance.Add(crop.produceItem, Mathf.Max(crop.produceAmount, amount));
            }
        }

        state = State.Empty;
        ShowVisual(false);
    }

    void ShowVisual(bool on)
    {
        if (cropVisual != null) cropVisual.gameObject.SetActive(on);
    }
}