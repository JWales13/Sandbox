using UnityEngine;
using UnityEngine.UI;

// Lists a board's side quests: available ones get an Accept button, accepted /
// completed ones show their status. Construction is in Build(); open/close,
// cursor and control are handled by UIWindow.
public class QuestBoardUI : UIWindow
{
    public static QuestBoardUI Instance { get; private set; }

    QuestBoard current;
    Text titleLabel;
    RectTransform listRoot;

    void Awake() { Instance = this; }

    public void Open(QuestBoard board)
    {
        current = board;
        base.Open();
    }

    protected override void OnOpened()
    {
        if (titleLabel != null) titleLabel.text = current != null ? current.BoardName : "Quest Board";
        Refresh();
    }

    protected override void OnClosed() => current = null;

    protected override void Build()
    {
        if (panel == null) return;
        UIBuilder.SizeWindow(panel, new Vector2(0.24f, 0.16f), new Vector2(0.76f, 0.84f));

        titleLabel = UIBuilder.AnchoredLabel(panel.transform, "Quest Board", 30, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0, -16), new Vector2(500, 40), true);

        listRoot = UIBuilder.VerticalList(panel.transform, "Postings",
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
        if (current == null || qm == null) return;

        bool anyShown = false;
        foreach (var q in current.Postings)
        {
            if (q == null) continue;

            if (qm.IsAvailable(q))
            {
                var captured = q;
                UIBuilder.Row(listRoot, q.title, "Accept", true, () => { qm.Accept(captured); Refresh(); });
                anyShown = true;
            }
            else if (qm.IsActive(q))
            {
                UIBuilder.Row(listRoot, q.title + "   (in progress)", "Accepted", false, null);
                anyShown = true;
            }
            else if (qm.IsCompleted(q))
            {
                UIBuilder.Row(listRoot, q.title + "   (done)", "✔", false, null);
                anyShown = true;
            }
            // else: locked by a prerequisite — hidden until it unlocks.
        }

        if (!anyShown)
            UIBuilder.Label(listRoot, "No notices right now. Check back later.", 16, TextAnchor.MiddleCenter);
    }
}