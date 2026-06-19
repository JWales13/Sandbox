using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Self-building attributes screen (toggle with C). Lists every attribute with its
// value and a + button to spend attribute points. Built via UIBuilder; new enum
// entries appear automatically.
public class AttributesUI : MonoBehaviour
{
    [Header("Data")]
    public PlayerProgression progression;

    [Header("References")]
    public GameObject panel;
    public KeyCode toggleKey = KeyCode.C;

    [Header("Disabled while open")]
    public PlayerController playerController;
    public PlayerInteractor playerInteractor;

    readonly Dictionary<AttributeType, Text> valueTexts = new Dictionary<AttributeType, Text>();
    readonly List<Button> plusButtons = new List<Button>();
    Text pointsLabel;
    bool built, isOpen;

    void Start()
    {
        if (progression == null) progression = PlayerProgression.Instance;
        if (progression != null) progression.OnChanged += Refresh;
        if (panel != null) panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (progression != null) progression.OnChanged -= Refresh;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) Toggle();
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        if (isOpen && !built) Build();
        if (panel != null) panel.SetActive(isOpen);

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;
        if (playerController != null) playerController.enabled = !isOpen;
        if (playerInteractor != null) playerInteractor.enabled = !isOpen;

        if (isOpen) Refresh();
    }

    void Build()
    {
        built = true;
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

    public void Close()
    {
        isOpen = false;
        if (panel != null) panel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController != null) playerController.enabled = true;
        if (playerInteractor != null) playerInteractor.enabled = true;
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