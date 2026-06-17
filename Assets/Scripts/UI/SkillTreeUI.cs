using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// A self-building skill-tree window. Press the toggle key to open it; it draws
// one clickable node per perk (positioned by each perk's treePosition) with
// connector lines for prerequisites, and colors nodes by state. Click an
// available node to spend a discipline perk point.
public class SkillTreeUI : MonoBehaviour
{
    [Header("Data")]
    public DisciplineSO discipline;
    public PlayerProgression progression;

    [Header("Scene references")]
    public GameObject rootPanel;       // the window root (hidden until opened)
    public RectTransform treeRoot;     // container the nodes/lines are drawn in
    public Text pointsLabel;           // shows this discipline's perk points
    public KeyCode toggleKey = KeyCode.K;

    [Header("Disabled while the tree is open")]
    public PlayerController playerController;
    public PlayerInteractor playerInteractor;

    [Header("Style")]
    public Vector2 nodeSize = new Vector2(150, 60);
    public Color unlockedColor = new Color(0.20f, 0.70f, 0.30f);
    public Color availableColor = new Color(0.85f, 0.75f, 0.20f);
    public Color lockedColor = new Color(0.30f, 0.30f, 0.30f);
    public Color lineColor = new Color(1f, 1f, 1f, 0.4f);

    readonly Dictionary<SkillPerkSO, Image> nodes = new Dictionary<SkillPerkSO, Image>();
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
        if (discipline == null || treeRoot == null) return;

        // Lines first so they render behind the nodes.
        foreach (var s in discipline.subskills)
        {
            if (s == null) continue;
            foreach (var p in s.perks)
            {
                if (p == null) continue;
                foreach (var pre in p.prerequisites)
                    if (pre != null) CreateLine(pre.treePosition, p.treePosition);
            }
        }

        foreach (var s in discipline.subskills)
        {
            if (s == null) continue;
            foreach (var p in s.perks)
                if (p != null) CreateNode(p);
        }
    }

    void CreateNode(SkillPerkSO perk)
    {
        var go = new GameObject(perk.displayName, typeof(RectTransform), typeof(Image), typeof(Button));
        var rt = (RectTransform)go.transform;
        rt.SetParent(treeRoot, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = nodeSize;
        rt.anchoredPosition = perk.treePosition;

        nodes[perk] = go.GetComponent<Image>();

        go.GetComponent<Button>().onClick.AddListener(() =>
        {
            progression.TryUnlock(perk);
            Refresh();
        });

        var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
        var lrt = (RectTransform)label.transform;
        lrt.SetParent(rt, false);
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;

        var txt = label.GetComponent<Text>();
        txt.text = perk.displayName;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.font = font;
        txt.resizeTextForBestFit = true;
        txt.resizeTextMinSize = 8;
        txt.resizeTextMaxSize = 18;
        txt.raycastTarget = false;
    }

    void CreateLine(Vector2 a, Vector2 b)
    {
        var go = new GameObject("Line", typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(treeRoot, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);

        Vector2 dir = b - a;
        rt.sizeDelta = new Vector2(dir.magnitude, 4f);
        rt.anchoredPosition = a + dir * 0.5f;
        rt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        var img = go.GetComponent<Image>();
        img.color = lineColor;
        img.raycastTarget = false;
    }

    void Refresh()
    {
        if (progression == null) return;

        if (discipline != null && pointsLabel != null)
            pointsLabel.text = $"{discipline.displayName} — Perk Points: {progression.GetPerkPoints(discipline)}";

        foreach (var kv in nodes)
        {
            if (kv.Value == null) continue;
            if (progression.IsUnlocked(kv.Key)) kv.Value.color = unlockedColor;
            else if (progression.CanUnlock(kv.Key)) kv.Value.color = availableColor;
            else kv.Value.color = lockedColor;
        }
    }
}