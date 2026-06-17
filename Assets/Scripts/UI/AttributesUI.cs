using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// A self-building attributes screen (toggle with C). Lists every attribute with
// its value and a + button to spend attribute points. Disables player control
// while open. New attributes added to the enum appear automatically.
public class AttributesUI : MonoBehaviour
{
    [Header("Data")]
    public PlayerProgression progression;

    [Header("Scene references")]
    public GameObject rootPanel;
    public RectTransform rowContainer;   // gets a VerticalLayoutGroup
    public Text pointsLabel;
    public KeyCode toggleKey = KeyCode.C;

    [Header("Disabled while open")]
    public PlayerController playerController;
    public PlayerInteractor playerInteractor;

    readonly Dictionary<AttributeType, Text> valueTexts = new Dictionary<AttributeType, Text>();
    readonly List<Button> plusButtons = new List<Button>();
    Font font;
    bool built;
    bool isOpen;

    void Start()
    {
        if (progression == null) progression = PlayerProgression.Instance;
        if (progression != null) progression.OnChanged += Refresh;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (rootPanel != null) rootPanel.SetActive(false);
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
        if (rootPanel != null) rootPanel.SetActive(isOpen);

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;
        if (playerController != null) playerController.enabled = !isOpen;
        if (playerInteractor != null) playerInteractor.enabled = !isOpen;

        if (isOpen) Refresh();
    }

    void Build()
    {
        built = true;
        if (rowContainer == null) return;

        var v = rowContainer.GetComponent<VerticalLayoutGroup>();
        if (v == null) v = rowContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        v.spacing = 8;
        v.padding = new RectOffset(12, 12, 12, 12);
        v.childControlWidth = true; v.childControlHeight = true;
        v.childForceExpandWidth = true; v.childForceExpandHeight = false;

        foreach (AttributeType a in Enum.GetValues(typeof(AttributeType)))
            CreateRow(a);
    }

    void CreateRow(AttributeType a)
    {
        var row = new GameObject(a.ToString(), typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(rowContainer, false);
        var hl = row.GetComponent<HorizontalLayoutGroup>();
        hl.spacing = 10;
        hl.childControlWidth = true; hl.childControlHeight = true;
        hl.childForceExpandWidth = false;
        row.GetComponent<LayoutElement>().minHeight = 40;

        var valueTxt = MakeText($"{a}: 0", 20, TextAnchor.MiddleLeft);
        valueTxt.transform.SetParent(row.transform, false);
        var le = valueTxt.gameObject.AddComponent<LayoutElement>();
        le.minWidth = 240;
        valueTexts[a] = valueTxt;

        var btnGO = new GameObject("Plus", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(row.transform, false);
        var ble = btnGO.AddComponent<LayoutElement>();
        ble.minWidth = 50; ble.minHeight = 36;
        btnGO.GetComponent<Image>().color = new Color(0.25f, 0.5f, 0.9f);

        var plusTxt = MakeText("+", 22, TextAnchor.MiddleCenter);
        plusTxt.transform.SetParent(btnGO.transform, false);
        var prt = (RectTransform)plusTxt.transform;
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;
        plusTxt.raycastTarget = false;

        AttributeType captured = a;
        var btn = btnGO.GetComponent<Button>();
        btn.onClick.AddListener(() => { progression.InvestAttribute(captured); Refresh(); });
        plusButtons.Add(btn);
    }

    Text MakeText(string s, int size, TextAnchor align)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        var t = go.GetComponent<Text>();
        t.text = s; t.font = font; t.fontSize = size; t.color = Color.white; t.alignment = align;
        return t;
    }

    void Refresh()
    {
        if (progression == null) return;
        if (pointsLabel != null) pointsLabel.text = $"Attribute Points: {progression.AttributePoints}";

        foreach (var kv in valueTexts)
            kv.Value.text = $"{kv.Key}: {progression.GetAttribute(kv.Key)}";

        bool hasPoints = progression.AttributePoints > 0;
        foreach (var b in plusButtons)
            b.interactable = hasPoints;
    }
}