using UnityEngine;
using UnityEngine.UI;

// Reusable UI construction + layout helpers, all themed via UITheme.
// UI scripts use these instead of hand-rolling text/buttons/rows/containers,
// so the look and layout stay consistent and live in one place.
public static class UIBuilder
{
    // Size a window to a centered fraction of the screen (anchors 0..1).
    public static void SizeWindow(GameObject panel, Vector2 anchorMin, Vector2 anchorMax)
    {
        if (panel == null) return;
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = panel.GetComponent<Image>();
        if (img != null) UITheme.StylePanel(img);
    }

    // A child rect region. margins = (left, bottom, right, top) insets.
    public static RectTransform Area(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector4 margins)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = new Vector2(margins.x, margins.y);
        rt.offsetMax = new Vector2(-margins.z, -margins.w);
        return rt;
    }

    public static Text Label(Transform parent, string text, int size, TextAnchor align, bool heading = false)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.text = text; t.alignment = align;
        UITheme.StyleText(t, size, heading);
        return t;
    }

    // Anchored label (its own positioned rect).
    public static Text AnchoredLabel(Transform parent, string text, int size, TextAnchor align, Vector2 anchor, Vector2 pos, Vector2 sizeDelta, bool heading = false)
    {
        var t = Label(parent, text, size, align, heading);
        var rt = (RectTransform)t.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;
        return t;
    }

    public static Button Button(Transform parent, string label, System.Action onClick, int size = 16)
    {
        var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var btn = go.GetComponent<Button>();
        UITheme.StyleButton(btn);

        var t = Label(go.transform, label, size, TextAnchor.MiddleCenter);
        var lrt = (RectTransform)t.transform;
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        t.raycastTarget = false;

        if (onClick != null) btn.onClick.AddListener(() => onClick());
        return btn;
    }

    // A child container with a themed background + vertical stacking of rows.
    public static RectTransform VerticalList(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector4 margins)
    {
        var rt = Area(parent, name, aMin, aMax, margins);
        UITheme.StyleSlot(rt.gameObject.AddComponent<Image>());
        var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
        v.spacing = 4; v.padding = new RectOffset(8, 8, 8, 8);
        v.childControlWidth = true; v.childControlHeight = true;
        v.childForceExpandWidth = true; v.childForceExpandHeight = false;
        v.childAlignment = TextAnchor.UpperCenter;
        return rt;
    }

    // A row: flexible label on the left + an action button on the right.
    public static void Row(Transform parent, string labelText, string buttonText, bool enabled, System.Action onClick)
    {
        var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        var hl = row.GetComponent<HorizontalLayoutGroup>();
        hl.spacing = 8; hl.childControlWidth = true; hl.childControlHeight = true; hl.childForceExpandWidth = false;
        row.GetComponent<LayoutElement>().minHeight = 36;

        var label = Label(row.transform, labelText, 16, TextAnchor.MiddleLeft);
        var le = label.gameObject.AddComponent<LayoutElement>();
        le.minWidth = 240; le.flexibleWidth = 1;

        var btn = Button(row.transform, buttonText, onClick);
        var ble = btn.gameObject.AddComponent<LayoutElement>();
        ble.minWidth = 84; ble.minHeight = 30;
        btn.interactable = enabled;
    }

    // Clear all generated children of a container.
    public static void Clear(Transform container)
    {
        if (container == null) return;
        for (int i = container.childCount - 1; i >= 0; i--) Object.Destroy(container.GetChild(i).gameObject);
    }
}