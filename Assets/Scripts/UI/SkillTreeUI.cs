using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Tabbed skill-tree window. One tab per discipline; clicking a tab rebuilds the
// node graph for that discipline. Nodes are positioned by each perk's treePosition,
// prerequisite lines are drawn, and clicking an available node spends a point from
// that discipline's pool.
public class SkillTreeUI : MonoBehaviour
{
    [Header("Data")]
    public List<DisciplineSO> disciplines = new List<DisciplineSO>();
    public PlayerProgression progression;

    [Header("Scene references")]
    public GameObject rootPanel;
    public RectTransform tabContainer;   // top bar for discipline tabs (gets a HorizontalLayoutGroup)
    public RectTransform treeRoot;       // nodes/lines are drawn here
    public Text pointsLabel;
    public KeyCode toggleKey = KeyCode.K;

    [Header("Disabled while open")]
    public PlayerController playerController;
    public PlayerInteractor playerInteractor;

    [Header("Style")]
    public Vector2 nodeSize = new Vector2(150, 60);
    public Color unlockedColor = new Color(0.20f, 0.70f, 0.30f);
    public Color availableColor = new Color(0.85f, 0.75f, 0.20f);
    public Color lockedColor = new Color(0.30f, 0.30f, 0.30f);
    public Color lineColor = new Color(1f, 1f, 1f, 0.4f);

    readonly Dictionary<SkillPerkSO, Image> nodes = new Dictionary<SkillPerkSO, Image>();
    readonly List<GameObject> spawned = new List<GameObject>();   // current tree's nodes + lines
    DisciplineSO active;
    Font font;
    bool tabsBuilt;
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

        if (isOpen)
        {
            if (!tabsBuilt) BuildTabs();
            if (active == null && disciplines.Count > 0) SwitchTo(disciplines[0]);
        }

        if (rootPanel != null) rootPanel.SetActive(isOpen);
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;
        if (playerController != null) playerController.enabled = !isOpen;
        if (playerInteractor != null) playerInteractor.enabled = !isOpen;

        if (isOpen) Refresh();
    }

    void BuildTabs()
    {
        tabsBuilt = true;
        var parent = tabContainer != null ? tabContainer : treeRoot;
        if (parent == null) return;

        if (tabContainer != null && tabContainer.GetComponent<HorizontalLayoutGroup>() == null)
        {
            var h = tabContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 6; h.padding = new RectOffset(6, 6, 6, 6);
            h.childControlWidth = true; h.childControlHeight = true; h.childForceExpandWidth = false;
        }

        foreach (var d in disciplines)
        {
            if (d == null) continue;
            var captured = d;

            var go = new GameObject(d.displayName + " Tab", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var le = go.GetComponent<LayoutElement>(); le.minWidth = 120; le.minHeight = 32;
            go.GetComponent<Image>().color = new Color(0.20f, 0.20f, 0.25f);

            var label = MakeText(d.displayName, 16, TextAnchor.MiddleCenter);
            var lrt = (RectTransform)label.transform;
            lrt.SetParent(go.transform, false);
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
            label.raycastTarget = false;

            go.GetComponent<Button>().onClick.AddListener(() => SwitchTo(captured));
        }
    }

    public void SwitchTo(DisciplineSO d)
    {
        active = d;
        ClearTree();
        BuildTree(d);
        Refresh();
    }

    void ClearTree()
    {
        foreach (var go in spawned) if (go != null) Destroy(go);
        spawned.Clear();
        nodes.Clear();
    }

    void BuildTree(DisciplineSO d)
    {
        if (d == null || treeRoot == null) return;

        foreach (var s in d.subskills)
        {
            if (s == null) continue;
            foreach (var p in s.perks)
            {
                if (p == null) continue;
                foreach (var pre in p.prerequisites)
                    if (pre != null) CreateLine(pre.treePosition, p.treePosition);
            }
        }

        foreach (var s in d.subskills)
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
        spawned.Add(go);

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

        spawned.Add(go);
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

        if (pointsLabel != null && active != null)
            pointsLabel.text = $"{active.displayName} — Perk Points: {progression.GetPerkPoints(active)}";

        foreach (var kv in nodes)
        {
            if (kv.Value == null) continue;
            if (progression.IsUnlocked(kv.Key)) kv.Value.color = unlockedColor;
            else if (progression.CanUnlock(kv.Key)) kv.Value.color = availableColor;
            else kv.Value.color = lockedColor;
        }
    }
}