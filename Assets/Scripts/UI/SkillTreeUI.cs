using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Tabbed skill-tree window (toggle K). Built via UIBuilder/UITheme; open/close
// handled by UIWindow. One tab per discipline; switching rebuilds the node graph.
public class SkillTreeUI : UIWindow
{
    [Header("Skill tree")]
    [SerializeField] List<DisciplineSO> disciplines = new List<DisciplineSO>();
    [SerializeField] PlayerProgression progression;
    [SerializeField] KeyCode toggleKey = KeyCode.K;

    [Header("Style")]
    [SerializeField] Vector2 nodeSize = new Vector2(200, 75);
    [SerializeField] float nodeSpacing = 1.5f;
    [SerializeField] Color unlockedColor = new Color(0.20f, 0.70f, 0.30f);
    [SerializeField] Color availableColor = new Color(0.85f, 0.75f, 0.20f);
    [SerializeField] Color lockedColor = new Color(0.30f, 0.30f, 0.30f);
    [SerializeField] Color lineColor = new Color(1f, 1f, 1f, 0.4f);

    readonly Dictionary<SkillPerkSO, Image> nodes = new Dictionary<SkillPerkSO, Image>();
    readonly List<GameObject> spawned = new List<GameObject>();
    RectTransform tabContainer, treeRoot;
    Text pointsLabel;
    DisciplineSO active;

    protected override void Start()
    {
        if (progression == null) progression = PlayerProgression.Instance;
        base.Start();
        if (progression != null) progression.OnChanged += Refresh;
    }

    void OnDestroy() { if (progression != null) progression.OnChanged -= Refresh; }

    void Update() { if (Input.GetKeyDown(toggleKey)) Toggle(); }

    protected override void OnOpened()
    {
        if (active == null && disciplines.Count > 0) SwitchTo(disciplines[0]);
        else Refresh();
    }

    protected override void Build()
    {
        if (panel == null) return;
        UIBuilder.SizeWindow(panel, new Vector2(0.15f, 0.12f), new Vector2(0.85f, 0.88f));

        tabContainer = UIBuilder.Area(panel.transform, "TabBar",
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector4(12, -64, 12, 12));
        var h = tabContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 6; h.padding = new RectOffset(6, 6, 6, 6);
        h.childControlWidth = true; h.childControlHeight = true; h.childForceExpandWidth = false;

        treeRoot = UIBuilder.Area(panel.transform, "TreeArea",
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector4(12, 50, 12, 74));

        pointsLabel = UIBuilder.AnchoredLabel(panel.transform, "", 18, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0f), new Vector2(0, 14), new Vector2(360, 30), true);

        var close = UIBuilder.Button(panel.transform, "Close", Close);
        var crt = (RectTransform)close.transform;
        crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(1f, 0f);   // bottom-right
        crt.anchoredPosition = new Vector2(-16, 10);
        crt.sizeDelta = new Vector2(120, 36);

        foreach (var d in disciplines)
        {
            if (d == null) continue;
            var captured = d;
            var btn = UIBuilder.Button(tabContainer, d.displayName, () => SwitchTo(captured));
            var le = btn.gameObject.AddComponent<LayoutElement>();
            le.minWidth = 130; le.minHeight = 36;
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
        rt.anchoredPosition = perk.treePosition * nodeSpacing;

        var nodeImg = go.GetComponent<Image>();
        nodeImg.sprite = UITheme.RoundedSprite();
        nodeImg.type = Image.Type.Sliced;
        nodes[perk] = nodeImg;
        spawned.Add(go);

        go.GetComponent<Button>().onClick.AddListener(() => { progression.TryUnlock(perk); Refresh(); });

        var txt = UIBuilder.Label(rt, perk.displayName, 16, TextAnchor.MiddleCenter);
        var lrt = (RectTransform)txt.transform;
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        txt.resizeTextForBestFit = true;
        txt.resizeTextMinSize = 8; txt.resizeTextMaxSize = 18;
        txt.raycastTarget = false;
    }

    void CreateLine(Vector2 a, Vector2 b)
    {
        a *= nodeSpacing; b *= nodeSpacing;

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