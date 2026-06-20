using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Attributes window (toggle C). Lists each attribute with a + button to spend
// points. Built via UIBuilder; open/close handled by UIWindow.
public class AttributesUI : UIWindow
{
    [Header("Attributes")]
    [SerializeField] PlayerProgression progression;
    [SerializeField] KeyCode toggleKey = KeyCode.C;

    readonly Dictionary<AttributeType, Text> valueTexts = new Dictionary<AttributeType, Text>();
    readonly List<Button> plusButtons = new List<Button>();
    Text pointsLabel;

    protected override void Start()
    {
        if (progression == null) progression = PlayerProgression.Instance;
        base.Start();
        if (progression != null) progression.OnChanged += Refresh;
    }

    void OnDestroy() { if (progression != null) progression.OnChanged -= Refresh; }

    void Update() { if (Input.GetKeyDown(toggleKey)) Toggle(); }

    protected override void OnOpened() => Refresh();

    protected override void Build()
    {
        if (panel == null) return;
        UIBuilder.SizeWindow(panel, new Vector2(0.3f, 0.16f), new Vector2(0.7f, 0.84f));

        UIBuilder.AnchoredLabel(panel.transform, "Attributes", 30, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0, -16), new Vector2(500, 40), true);
        pointsLabel = UIBuilder.AnchoredLabel(panel.transform, "", 18, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0, -56), new Vector2(500, 30), true);

        var list = UIBuilder.VerticalList(panel.transform, "AttrList",
            new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.80f), Vector4.zero);

        foreach (AttributeType a in Enum.GetValues(typeof(AttributeType)))
            CreateRow(list, a);

        var close = UIBuilder.Button(panel.transform, "Close", Close);
        var crt = (RectTransform)close.transform;
        crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0f);
        crt.anchoredPosition = new Vector2(0, 16);
        crt.sizeDelta = new Vector2(150, 38);
    }

    void CreateRow(RectTransform list, AttributeType a)
    {
        var row = new GameObject(a.ToString(), typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(list, false);
        var hl = row.GetComponent<HorizontalLayoutGroup>();
        hl.spacing = 10; hl.childControlWidth = true; hl.childControlHeight = true; hl.childForceExpandWidth = false;
        row.GetComponent<LayoutElement>().minHeight = 42;

        var val = UIBuilder.Label(row.transform, $"{a}: 0", 20, TextAnchor.MiddleLeft);
        var le = val.gameObject.AddComponent<LayoutElement>(); le.minWidth = 260; le.flexibleWidth = 1;
        valueTexts[a] = val;

        AttributeType captured = a;
        var plus = UIBuilder.Button(row.transform, "+", () => { progression.InvestAttribute(captured); Refresh(); }, 22);
        var ble = plus.gameObject.AddComponent<LayoutElement>(); ble.minWidth = 54; ble.minHeight = 36;
        plusButtons.Add(plus);
    }

    void Refresh()
    {
        if (progression == null) return;
        if (pointsLabel != null) pointsLabel.text = $"Attribute Points: {progression.AttributePoints}";

        foreach (var kv in valueTexts)
            kv.Value.text = $"{kv.Key}: {progression.GetAttribute(kv.Key)}";

        bool hasPoints = progression.AttributePoints > 0;
        foreach (var b in plusButtons) b.interactable = hasPoints;
    }
}