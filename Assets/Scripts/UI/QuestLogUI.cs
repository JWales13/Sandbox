using UnityEngine;
using UnityEngine.UI;

// The quest journal (toggle J). Lists active quests and their objectives with
// live progress, refreshing whenever the quest system changes.
public class QuestLogUI : UIWindow
{
    [SerializeField] KeyCode toggleKey = KeyCode.J;

    RectTransform listRoot;

    protected override void Start()
    {
        base.Start();
        if (QuestManager.Instance != null) QuestManager.Instance.OnChanged += RefreshIfOpen;
    }

    void OnDestroy()
    {
        if (QuestManager.Instance != null) QuestManager.Instance.OnChanged -= RefreshIfOpen;
    }

    void Update() { if (Input.GetKeyDown(toggleKey)) Toggle(); }

    protected override void OnOpened() => Refresh();
    void RefreshIfOpen() { if (IsOpen) Refresh(); }

    protected override void Build()
    {
        if (panel == null) return;
        UIBuilder.SizeWindow(panel, new Vector2(0.24f, 0.16f), new Vector2(0.76f, 0.84f));

        UIBuilder.AnchoredLabel(panel.transform, "Quest Journal", 30, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0, -16), new Vector2(500, 40), true);

        listRoot = UIBuilder.VerticalList(panel.transform, "Quests",
            new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.82f), Vector4.zero);

        var close = UIBuilder.Button(panel.transform, "Close", Close);
        var crt = (RectTransform)close.transform;
        crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0f);
        crt.anchoredPosition = new Vector2(0, 16);
        crt.sizeDelta = new Vector2(150, 38);
    }

    void Refresh()
    {
        if (listRoot == null) return;
        UIBuilder.Clear(listRoot);

        var qm = QuestManager.Instance;
        bool any = false;

        if (qm != null)
            foreach (var q in qm.ActiveQuests)
            {
                any = true;
                UIBuilder.Label(listRoot, q.title, 20, TextAnchor.MiddleLeft, true);

                for (int i = 0; i < q.objectives.Count; i++)
                {
                    int prog = qm.GetObjectiveProgress(q, i);
                    UIBuilder.Label(listRoot, "   • " + q.objectives[i].Label(prog), 15, TextAnchor.MiddleLeft);
                }

                if (qm.IsReadyToTurnIn(q))
                    UIBuilder.Label(listRoot, "   ➜ Ready to turn in", 15, TextAnchor.MiddleLeft);
            }

        if (!any)
            UIBuilder.Label(listRoot, "No active quests.", 16, TextAnchor.MiddleCenter);
    }
}