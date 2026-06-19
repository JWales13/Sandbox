using UnityEngine;
using UnityEngine.UI;

// The single home for the game's UI look (vibrant anime). Change colors/sizes
// here and every code-built screen updates. Provides a procedurally generated
// rounded sprite so panels/buttons have soft corners with no art assets.
public static class UITheme
{
    // ---- Palette ----
    public static readonly Color PanelBg   = new Color32(26, 20, 46, 235);   // deep indigo, translucent
    public static readonly Color SlotBg    = new Color32(45, 35, 70, 220);   // lighter slate-purple
    public static readonly Color Accent    = new Color32(255, 64, 160, 255);  // magenta/pink
    public static readonly Color AccentAlt = new Color32(70, 220, 255, 255);  // cyan
    public static readonly Color ButtonHi  = new Color32(255, 120, 195, 255); // hover pink
    public static readonly Color ButtonDn  = new Color32(200, 40, 120, 255);  // pressed pink
    public static readonly Color Disabled  = new Color32(80, 75, 95, 255);
    public static readonly Color TextMain  = new Color32(245, 245, 255, 255);
    public static readonly Color TextDim   = new Color32(180, 180, 205, 255);
    public static readonly Color OutlineCol = new Color32(10, 6, 24, 220);

    static Font _font;
    public static Font Font
    {
        get
        {
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return _font;
        }
    }

    // ---- Rounded 9-slice sprite (generated once) ----
    static Sprite _rounded;
    public static Sprite RoundedSprite()
    {
        if (_rounded != null) return _rounded;

        const int size = 32, radius = 10;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        var px = new Color32[size * size];

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x < radius ? radius - x : (x > size - 1 - radius ? x - (size - 1 - radius) : 0);
                float dy = y < radius ? radius - y : (y > size - 1 - radius ? y - (size - 1 - radius) : 0);
                bool outside = dx > 0 && dy > 0 && Mathf.Sqrt(dx * dx + dy * dy) > radius;
                px[y * size + x] = outside ? new Color32(255, 255, 255, 0) : new Color32(255, 255, 255, 255);
            }

        tex.SetPixels32(px);
        tex.Apply();
        _rounded = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
            100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        return _rounded;
    }

    // ---- Helpers ----

    public static void StylePanel(Image img, bool accentTint = false)
    {
        if (img == null) return;
        img.sprite = RoundedSprite();
        img.type = Image.Type.Sliced;
        img.color = accentTint ? SlotBg : PanelBg;
    }

    public static void StyleSlot(Image img)
    {
        if (img == null) return;
        img.sprite = RoundedSprite();
        img.type = Image.Type.Sliced;
        img.color = SlotBg;
    }

    public static void StyleButton(Button btn)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = RoundedSprite();
            img.type = Image.Type.Sliced;
            img.color = Color.white; // tinted by the ColorBlock below
        }
        var cb = btn.colors;
        cb.normalColor = Accent;
        cb.highlightedColor = ButtonHi;
        cb.pressedColor = ButtonDn;
        cb.selectedColor = Accent;
        cb.disabledColor = Disabled;
        cb.fadeDuration = 0.08f;
        btn.colors = cb;
    }

    // Keep the text's existing size; just apply font/color/outline.
    public static void StyleText(Text t)
    {
        if (t != null) StyleText(t, t.fontSize);
    }

    public static void StyleText(Text t, int size, bool heading = false, bool dim = false)
    {
        if (t == null) return;
        t.font = Font;
        t.fontSize = size;
        t.fontStyle = heading ? FontStyle.Bold : FontStyle.Normal;
        t.color = dim ? TextDim : TextMain;

        // Outline for readability over vibrant backgrounds.
        var outline = t.GetComponent<Outline>();
        if (outline == null) outline = t.gameObject.AddComponent<Outline>();
        outline.effectColor = OutlineCol;
        outline.effectDistance = new Vector2(1.5f, -1.5f);
    }
}