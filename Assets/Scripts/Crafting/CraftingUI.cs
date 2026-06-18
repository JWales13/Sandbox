using System.Text;
using UnityEngine;
using UnityEngine.UI;

// Self-building crafting window. Open(station) lists its recipes; each row shows
// "Output xN  ←  ingredients" and a Craft button enabled only when you can make it.
public class CraftingUI : MonoBehaviour
{
    public static CraftingUI Instance { get; private set; }

    [Header("Scene references")]
    public GameObject panel;
    public RectTransform recipeContainer;   // gets a VerticalLayoutGroup
    public Text titleLabel;

    [Header("Disabled while open")]
    public PlayerController playerController;
    public PlayerInteractor playerInteractor;
    public PlayerCombat playerCombat;

    CraftingStation current;
    Font font;

    void Awake() { Instance = this; }

    void Start()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        EnsureLayout(recipeContainer);
        if (panel != null) panel.SetActive(false);
        if (Inventory.Instance != null) Inventory.Instance.OnChanged += Refresh;
        if (PlayerProgression.Instance != null) PlayerProgression.Instance.OnChanged += Refresh;
    }

    void OnDestroy()
    {
        if (Inventory.Instance != null) Inventory.Instance.OnChanged -= Refresh;
        if (PlayerProgression.Instance != null) PlayerProgression.Instance.OnChanged -= Refresh;
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

    bool CanCraft(RecipeSO r)
    {
        if (r == null || Inventory.Instance == null) return false;

        if (r.subskill != null && PlayerProgression.Instance != null &&
            PlayerProgression.Instance.GetSubskillLevel(r.subskill) < r.requiredSubskillLevel)
            return false;

        foreach (var inp in r.inputs)
            if (inp.item == null || Inventory.Instance.CountOf(inp.item) < inp.amount)
                return false;

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

    void Refresh()
    {
        if (panel == null || !panel.activeSelf) return;

        Clear(recipeContainer);
        if (current == null) return;

        foreach (var r in current.recipes)
        {
            if (r == null) continue;
            var captured = r;
            MakeRow(recipeContainer, Describe(r), "Craft", CanCraft(r), () => Craft(captured));
        }
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

    // ---------- UI helpers ----------

    void EnsureLayout(RectTransform c)
    {
        if (c == null) return;
        var v = c.GetComponent<VerticalLayoutGroup>();
        if (v == null) v = c.gameObject.AddComponent<VerticalLayoutGroup>();
        v.spacing = 4;
        v.padding = new RectOffset(6, 6, 6, 6);
        v.childControlWidth = true; v.childControlHeight = true;
        v.childForceExpandWidth = true; v.childForceExpandHeight = false;
    }

    void Clear(RectTransform c)
    {
        if (c == null) return;
        for (int i = c.childCount - 1; i >= 0; i--) Destroy(c.GetChild(i).gameObject);
    }

    void MakeRow(RectTransform parent, string label, string buttonText, bool enabled, System.Action onClick)
    {
        var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        var hl = row.GetComponent<HorizontalLayoutGroup>();
        hl.spacing = 8;
        hl.childControlWidth = true; hl.childControlHeight = true;
        hl.childForceExpandWidth = false;
        row.GetComponent<LayoutElement>().minHeight = 34;

        var labelTxt = MakeText(label, 15, TextAnchor.MiddleLeft);
        labelTxt.transform.SetParent(row.transform, false);
        var le = labelTxt.gameObject.AddComponent<LayoutElement>();
        le.minWidth = 300; le.flexibleWidth = 1;

        var btnGO = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(row.transform, false);
        var ble = btnGO.AddComponent<LayoutElement>();
        ble.minWidth = 80; ble.minHeight = 28;
        btnGO.GetComponent<Image>().color = new Color(0.25f, 0.5f, 0.9f);

        var bt = MakeText(buttonText, 15, TextAnchor.MiddleCenter);
        bt.transform.SetParent(btnGO.transform, false);
        var brt = (RectTransform)bt.transform;
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
        brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
        bt.raycastTarget = false;

        var btn = btnGO.GetComponent<Button>();
        btn.interactable = enabled;
        btn.onClick.AddListener(() => onClick());
    }

    Text MakeText(string s, int size, TextAnchor align)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        var t = go.GetComponent<Text>();
        t.text = s; t.font = font; t.fontSize = size; t.color = Color.white; t.alignment = align;
        return t;
    }

    void SetControl(bool on)
    {
        if (playerController != null) playerController.enabled = on;
        if (playerInteractor != null) playerInteractor.enabled = on;
        if (playerCombat != null) playerCombat.enabled = on;
    }
}