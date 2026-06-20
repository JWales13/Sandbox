using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Equipment window (toggle G). One row per slot showing the worn item + an
// Unequip button. Built via UIBuilder; open/close handled by UIWindow.
public class EquipmentUI : UIWindow
{
    [SerializeField] KeyCode toggleKey = KeyCode.G;

    EquipSlot[] slots;
    readonly List<Text> rowLabels = new List<Text>();
    readonly List<Button> unequipButtons = new List<Button>();

    protected override void Start()
    {
        base.Start();
        if (Equipment.Instance != null) Equipment.Instance.OnChanged += Refresh;
    }

    void OnDestroy() { if (Equipment.Instance != null) Equipment.Instance.OnChanged -= Refresh; }

    void Update() { if (Input.GetKeyDown(toggleKey)) Toggle(); }

    protected override void OnOpened() => Refresh();

    protected override void Build()
    {
        if (panel == null) return;
        UIBuilder.SizeWindow(panel, new Vector2(0.30f, 0.18f), new Vector2(0.70f, 0.82f));

        UIBuilder.AnchoredLabel(panel.transform, "Equipment", 30, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0, -16), new Vector2(500, 40), true);

        var list = UIBuilder.VerticalList(panel.transform, "EquipList",
            new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.82f), Vector4.zero);

        slots = (EquipSlot[])Enum.GetValues(typeof(EquipSlot));
        foreach (var s in slots)
        {
            EquipSlot captured = s;

            var row = new GameObject(s.ToString(), typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(list, false);
            var hl = row.GetComponent<HorizontalLayoutGroup>();
            hl.spacing = 10; hl.childControlWidth = true; hl.childControlHeight = true; hl.childForceExpandWidth = false;
            row.GetComponent<LayoutElement>().minHeight = 42;

            var lbl = UIBuilder.Label(row.transform, "", 18, TextAnchor.MiddleLeft);
            var le = lbl.gameObject.AddComponent<LayoutElement>(); le.minWidth = 300; le.flexibleWidth = 1;
            rowLabels.Add(lbl);

            var btn = UIBuilder.Button(row.transform, "Unequip", () => Equipment.Instance?.Unequip(captured), 16);
            var ble = btn.gameObject.AddComponent<LayoutElement>(); ble.minWidth = 120; ble.minHeight = 34;
            unequipButtons.Add(btn);
        }

        var close = UIBuilder.Button(panel.transform, "Close", Close);
        var crt = (RectTransform)close.transform;
        crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0f);
        crt.anchoredPosition = new Vector2(0, 16);
        crt.sizeDelta = new Vector2(150, 38);
    }

    void Refresh()
    {
        if (slots == null || Equipment.Instance == null) return;
        for (int i = 0; i < slots.Length; i++)
        {
            var eq = Equipment.Instance.Get(slots[i]);
            rowLabels[i].text = $"{slots[i]}:  {(eq != null ? eq.displayName : "—")}";
            unequipButtons[i].interactable = eq != null;
        }
    }
}