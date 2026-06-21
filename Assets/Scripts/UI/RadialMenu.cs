using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Hold-to-open radial menu (controller: L; keyboard: Tab). Point the left stick
// (or WASD) at a slice; releasing the button opens that window. Built in code,
// overlaid on the Canvas. Player control is suspended while the wheel is up so
// the stick drives selection instead of movement.
//
// Each window is a concrete-typed field, so you can drag the object that holds
// the script (e.g. GameSystems) straight in and Unity picks the right component
// automatically — no need to disambiguate. Leave any field empty to skip it.
public class RadialMenu : MonoBehaviour
{
    [Header("Windows (placed clockwise from the top)")]
    [SerializeField] InventoryUI inventory;
    [SerializeField] QuestLogUI journal;
    [SerializeField] SkillTreeUI skills;
    [SerializeField] EquipmentUI equipment;
    [SerializeField] AttributesUI attributes;

    [Header("Suspended while the wheel is open")]
    [SerializeField] PlayerController playerController;
    [SerializeField] PlayerInteractor playerInteractor;
    [SerializeField] PlayerCombat playerCombat;

    [Header("Layout")]
    [SerializeField] float radius = 170f;
    [SerializeField] Transform uiRoot;     // Canvas; auto-found if empty

    struct Slot { public string label; public UIWindow window; }
    readonly List<Slot> slots = new List<Slot>();

    GameObject panel;
    readonly List<Text> labels = new List<Text>();
    int highlighted = -1;
    bool open;

    void Start()
    {
        BuildSlots();

        if (uiRoot == null)
        {
            var c = FindAnyObjectByType<Canvas>();
            if (c != null) uiRoot = c.transform;
        }
        BuildUI();
        if (panel != null) panel.SetActive(false);

        if (GameInput.Instance != null)
        {
            GameInput.Instance.RadialOpened += OpenWheel;
            GameInput.Instance.RadialClosed += CloseWheel;
        }
    }

    void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.RadialOpened -= OpenWheel;
            GameInput.Instance.RadialClosed -= CloseWheel;
        }
    }

    // Wheel order, clockwise from top. Unassigned windows are skipped.
    void BuildSlots()
    {
        slots.Clear();
        Add("Inventory", inventory);
        Add("Journal", journal);
        Add("Skills", skills);
        Add("Equipment", equipment);
        Add("Attributes", attributes);
    }

    void Add(string label, UIWindow window)
    {
        if (window != null) slots.Add(new Slot { label = label, window = window });
    }

    void BuildUI()
    {
        if (uiRoot == null) return;

        panel = new GameObject("RadialMenu", typeof(RectTransform), typeof(CanvasGroup));
        var prt = (RectTransform)panel.transform;
        prt.SetParent(uiRoot, false);
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;
        panel.GetComponent<CanvasGroup>().blocksRaycasts = false;   // overlay, not clickable

        var center = new GameObject("Center", typeof(RectTransform));
        var crt = (RectTransform)center.transform;
        crt.SetParent(prt, false);
        crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.anchoredPosition = Vector2.zero;

        UIBuilder.AnchoredLabel(center.transform, "Menu", 16, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120, 30));

        labels.Clear();
        int n = Mathf.Max(1, slots.Count);
        for (int i = 0; i < slots.Count; i++)
        {
            float theta = (Mathf.PI * 2f) * i / n;                 // 0 = top, clockwise
            Vector2 pos = new Vector2(Mathf.Sin(theta), Mathf.Cos(theta)) * radius;

            var t = UIBuilder.Label(center.transform, slots[i].label, 18, TextAnchor.MiddleCenter, true);
            var rt = (RectTransform)t.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(160, 40);
            labels.Add(t);
        }
    }

    void OpenWheel()
    {
        if (open || panel == null) return;
        // Don't open over another window, a conversation, or while paused.
        if (UIWindow.Current != null) return;
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen) return;
        if (Time.timeScale == 0f) return;

        open = true;
        panel.SetActive(true);
        SetControl(false);
        highlighted = -1;
        UpdateHighlight(Vector2.zero);
    }

    void CloseWheel()
    {
        if (!open) return;
        open = false;
        if (panel != null) panel.SetActive(false);
        SetControl(true);

        if (highlighted >= 0 && highlighted < slots.Count && slots[highlighted].window != null)
            slots[highlighted].window.Open();   // the window takes over cursor/control from here
    }

    void Update()
    {
        if (!open || GameInput.Instance == null) return;
        UpdateHighlight(GameInput.Instance.RadialStick);
    }

    void UpdateHighlight(Vector2 dir)
    {
        int sel = -1;
        if (dir.sqrMagnitude > 0.25f)   // deadzone: need a clear push
        {
            float ang = Mathf.Atan2(dir.x, dir.y);   // 0 at top, increases clockwise
            if (ang < 0f) ang += Mathf.PI * 2f;
            int n = Mathf.Max(1, slots.Count);
            sel = Mathf.RoundToInt(ang / (Mathf.PI * 2f) * n) % n;
        }
        highlighted = sel;

        for (int i = 0; i < labels.Count; i++)
        {
            bool on = i == highlighted;
            labels[i].color = on ? Color.white : new Color(1f, 1f, 1f, 0.45f);
            labels[i].transform.localScale = on ? Vector3.one * 1.25f : Vector3.one;
        }
    }

    void SetControl(bool on)
    {
        if (playerController != null) playerController.enabled = on;
        if (playerInteractor != null) playerInteractor.enabled = on;
        if (playerCombat != null) playerCombat.enabled = on;
    }
}