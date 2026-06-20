using UnityEngine;
using UnityEngine.UI;

// Drop this on a Canvas (or any UI root) to theme the STATIC, hand-placed UI
// under it at startup — panels, titles, close buttons, etc. Code-generated
// content (rows, nodes) themes itself in its own script, so this only needs to
// catch the pieces built in the Inspector. Runs once at Start.
//
// Note: it styles every Image as a panel unless the object also has a Button.
// If a small decorative Image (e.g. a crosshair) gets restyled, add it to the
// 'ignore' list.
[DisallowMultipleComponent]
public class UIThemeApplier : MonoBehaviour
{
    [SerializeField] bool styleButtons = true;
    [SerializeField] bool stylePanels = true;
    [SerializeField] bool styleTexts = true;
    [SerializeField] GameObject[] ignore;

    void Start() { Apply(); }

    public void Apply()
    {
        foreach (var img in GetComponentsInChildren<Image>(true))
        {
            if (IsIgnored(img.gameObject)) continue;

            var btn = img.GetComponent<Button>();
            if (btn != null) { if (styleButtons) UITheme.StyleButton(btn); }
            else if (stylePanels) UITheme.StylePanel(img);
        }

        if (styleTexts)
            foreach (var t in GetComponentsInChildren<Text>(true))
                if (!IsIgnored(t.gameObject)) UITheme.StyleText(t);
    }

    bool IsIgnored(GameObject go)
    {
        if (ignore == null) return false;
        foreach (var g in ignore) if (g == go) return true;
        return false;
    }
}