using UnityEngine;
using UnityEngine.UI;

// A self-building, always-visible health bar (top-left). Resizes a colored
// fill rect to current/max and shows the numbers.
public class HealthBarUI : MonoBehaviour
{
    public PlayerHealth health;
    public RectTransform parent;                       // a Canvas to attach to (auto-found if empty)
    public Vector2 size = new Vector2(260, 26);
    public Vector2 position = new Vector2(20, -20);    // offset from top-left

    RectTransform fillRect;
    Text label;
    Font font;

    void Start()
    {
        if (health == null) health = FindAnyObjectByType<PlayerHealth>();
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Build();
        if (health != null) health.OnHealthChanged += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (health != null) health.OnHealthChanged -= Refresh;
    }

    void Build()
    {
        if (parent == null)
        {
            var c = FindAnyObjectByType<Canvas>();
            if (c != null) parent = (RectTransform)c.transform;
        }
        if (parent == null) return;

        var bg = new GameObject("HealthBar", typeof(RectTransform), typeof(Image));
        var brt = (RectTransform)bg.transform;
        brt.SetParent(parent, false);
        brt.anchorMin = brt.anchorMax = new Vector2(0, 1);
        brt.pivot = new Vector2(0, 1);
        brt.anchoredPosition = position;
        brt.sizeDelta = size;
        bg.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);

        var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillRect = (RectTransform)fillGO.transform;
        fillRect.SetParent(brt, false);
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.anchoredPosition = new Vector2(3, 0);
        fillRect.sizeDelta = new Vector2(size.x - 6, -6);
        fillGO.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f);

        var labelGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
        var lrt = (RectTransform)labelGO.transform;
        lrt.SetParent(brt, false);
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        label = labelGO.GetComponent<Text>();
        label.font = font;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
    }

    void Refresh()
    {
        if (health == null || fillRect == null) return;
        float ratio = health.MaxHealth > 0 ? (float)health.CurrentHealth / health.MaxHealth : 0f;
        fillRect.sizeDelta = new Vector2((size.x - 6) * ratio, -6);
        if (label != null) label.text = $"{health.CurrentHealth} / {health.MaxHealth}";
    }
}