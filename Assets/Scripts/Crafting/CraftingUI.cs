using System.Text;
using UnityEngine;
using UnityEngine.UI;

// Self-building crafting window. Needs a root panel + player references; it sizes
// itself and builds the title, recipe list, and close button via UIBuilder.
public class CraftingUI : MonoBehaviour
{
    public static CraftingUI Instance { get; private set; }

    [Header("References")]
    public GameObject panel;

    [Header("Disabled while open")]
    public PlayerController playerController;
    public PlayerInteractor playerInteractor;
    public PlayerCombat playerCombat;

    CraftingStation current;
    Text titleLabel;
    RectTransform recipeList;

    void Awake() { Instance = this; }

    void Start()
    {
        BuildStatic();
        if (panel != null) panel.SetActive(false);
        if (Inventory.Instance != null) Inventory.Instance.OnChanged += Refresh;
        if (PlayerProgression.Instance != null) PlayerProgression.Instance.OnChanged += Refresh;
    }

    void OnDestroy()
    {
        if (Inventory.Instance != null) Inventory.Instance.OnChanged -= Refresh;
        if (PlayerProgression.Instance != null) PlayerProgression.Instance.OnChanged -= Refresh;
    }

    void BuildStatic()
    {
        if (panel == null) return;
        UIBuilder.SizeWindow(panel, new Vector2(0.2f, 0.14f), new Vector2(0.8f, 0.86f));

        titleLabel = UIBuilder.AnchoredLabel(panel.transform, "Crafting", 30, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0, -16), new Vector2(500, 40), true);

        recipeList = UIBuilder.VerticalList(panel.transform, "RecipeList",
            new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.82f), Vector4.zero);

        var close = UIBuilder.Button(panel.transform, "Close", Close);
        var crt = (RectTransform)close.transform;
        crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0f);
        crt.anchoredPosition = new Vector2(0, 16);
        crt.sizeDelta = new Vector2(150, 38);
    }

    public void Open(CraftingStation station)
    {
        current = station;
        if (panel != null) panel.SetActive(true);
        if (titleLabel != null) titleLabel.text = station != null ? station.stationName : "Crafting";
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SetControl(false);
        Refresh();
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SetControl(true);
        current = null;
    }

    void Refresh()
    {
        if (panel == null || !panel.activeSelf) return;

        UIBuilder.Clear(recipeList);
        if (current == null) return;

        foreach (var r in current.recipes)
        {
            if (r == null) continue;
            var captured = r;
            UIBuilder.Row(recipeList, Describe(r), "Craft", CanCraft(r), () => Craft(captured));
        }
    }

    bool CanCraft(RecipeSO r)
    {
        if (r == null || Inventory.Instance == null) return false;
        if (r.subskill != null && PlayerProgression.Instance != null &&
            PlayerProgression.Instance.GetSubskillLevel(r.subskill) < r.requiredSubskillLevel)
            return false;
        foreach (var inp in r.inputs)
            if (inp.item == null || Inventory.Instance.CountOf(inp.item) < inp.amount) return false;
        return true;
    }

    void Craft(RecipeSO r)
    {
        if (!CanCraft(r)) return;
        foreach (var inp in r.inputs) Inventory.Instance.Remove(inp.item, inp.amount);
        if (r.output != null) Inventory.Instance.Add(r.output, r.outputAmount);
        if (r.subskill != null && PlayerProgression.Instance != null)
            PlayerProgression.Instance.AddSubskillXP(r.subskill, r.xpReward);
    }

    string Describe(RecipeSO r)
    {
        var sb = new StringBuilder();
        sb.Append(r.output != null ? r.output.displayName : "?");
        sb.Append($" x{r.outputAmount}   ←   ");
        for (int i = 0; i < r.inputs.Count; i++)
        {
            var inp = r.inputs[i];
            sb.Append($"{(inp.item != null ? inp.item.displayName : "?")} x{inp.amount}");
            if (i < r.inputs.Count - 1) sb.Append(", ");
        }
        return sb.ToString();
    }

    void SetControl(bool on)
    {
        if (playerController != null) playerController.enabled = on;
        if (playerInteractor != null) playerInteractor.enabled = on;
        if (playerCombat != null) playerCombat.enabled = on;
    }
}