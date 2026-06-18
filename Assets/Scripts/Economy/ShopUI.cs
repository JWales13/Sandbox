using UnityEngine;
using UnityEngine.UI;

// Self-building shop window. Open(shopkeeper) shows a Buy list (the shop's stock)
// and a Sell list (your inventory), with a coin balance. Rows are generated in code.
public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    [Header("Scene references")]
    public GameObject panel;
    public RectTransform buyContainer;   // gets a VerticalLayoutGroup
    public RectTransform sellContainer;  // gets a VerticalLayoutGroup
    public Text coinsLabel;
    public Text titleLabel;

    [Header("Disabled while open")]
    public PlayerController playerController;
    public PlayerInteractor playerInteractor;
    public PlayerCombat playerCombat;

    Shopkeeper current;
    Font font;

    void Awake() { Instance = this; }

    void Start()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        EnsureLayout(buyContainer);
        EnsureLayout(sellContainer);
        if (panel != null) panel.SetActive(false);
        if (Wallet.Instance != null) Wallet.Instance.OnChanged += Refresh;
        if (Inventory.Instance != null) Inventory.Instance.OnChanged += Refresh;
    }

    void OnDestroy()
    {
        if (Wallet.Instance != null) Wallet.Instance.OnChanged -= Refresh;
        if (Inventory.Instance != null) Inventory.Instance.OnChanged -= Refresh;
    }

    public void Open(Shopkeeper shop)
    {
        current = shop;
        if (panel != null) panel.SetActive(true);
        if (titleLabel != null) titleLabel.text = shop != null ? shop.shopName : "Shop";
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SetControl(false);
        Refresh();
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SetControl(true);
        current = null;
    }

    void Refresh()
    {
        if (panel == null || !panel.activeSelf) return;

        if (coinsLabel != null && Wallet.Instance != null)
            coinsLabel.text = $"Coins: {Wallet.Instance.coins}";

        Clear(buyContainer);
        if (current != null)
            foreach (var item in current.stock)
                if (item != null)
                {
                    var captured = item;
                    MakeRow(buyContainer, $"{item.displayName}   ({item.buyPrice}g)", "Buy", () => Buy(captured));
                }

        Clear(sellContainer);
        if (Inventory.Instance != null)
            foreach (var slot in Inventory.Instance.slots)
                if (!slot.IsEmpty)
                {
                    var captured = slot.item;
                    MakeRow(sellContainer, $"{slot.item.displayName} x{slot.count}   ({slot.item.sellPrice}g)", "Sell", () => Sell(captured));
                }
    }

    void Buy(ItemSO item)
    {
        if (Wallet.Instance == null || Inventory.Instance == null) return;
        if (!Wallet.Instance.CanAfford(item.buyPrice)) return;

        int leftover = Inventory.Instance.Add(item, 1);
        if (leftover == 0) Wallet.Instance.Spend(item.buyPrice);   // only charge if it actually fit
    }

    void Sell(ItemSO item)
    {
        if (Wallet.Instance == null || Inventory.Instance == null) return;
        if (!Inventory.Instance.Remove(item, 1)) return;

        int price = item.sellPrice;
        // SellPrice perks (e.g. Merchant) raise how much you get.
        if (PlayerProgression.Instance != null)
            price = Mathf.RoundToInt(price * (1f + PlayerProgression.Instance.GetStat(StatType.SellPrice)));
        Wallet.Instance.Add(price);
    }

    // ---------- UI building ----------

    void EnsureLayout(RectTransform c)
    {
        if (c == null) return;
        var v = c.GetComponent<VerticalLayoutGroup>();
        if (v == null) v = c.gameObject.AddComponent<VerticalLayoutGroup>();
        v.spacing = 4;
        v.padding = new RectOffset(6, 6, 6, 6);
        v.childControlWidth = true; v.childControlHeight = true;
        v.childForceExpandWidth = true; v.childForceExpandHeight = false;
    }

    void Clear(RectTransform c)
    {
        if (c == null) return;
        for (int i = c.childCount - 1; i >= 0; i--) Destroy(c.GetChild(i).gameObject);
    }

    void MakeRow(RectTransform parent, string label, string buttonText, System.Action onClick)
    {
        var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        var hl = row.GetComponent<HorizontalLayoutGroup>();
        hl.spacing = 8;
        hl.childControlWidth = true; hl.childControlHeight = true;
        hl.childForceExpandWidth = false;
        row.GetComponent<LayoutElement>().minHeight = 34;

        var labelTxt = MakeText(label, 16, TextAnchor.MiddleLeft);
        labelTxt.transform.SetParent(row.transform, false);
        var le = labelTxt.gameObject.AddComponent<LayoutElement>();
        le.minWidth = 240; le.flexibleWidth = 1;

        var btnGO = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(row.transform, false);
        var ble = btnGO.AddComponent<LayoutElement>();
        ble.minWidth = 72; ble.minHeight = 28;
        btnGO.GetComponent<Image>().color = new Color(0.25f, 0.5f, 0.9f);

        var bt = MakeText(buttonText, 16, TextAnchor.MiddleCenter);
        bt.transform.SetParent(btnGO.transform, false);
        var brt = (RectTransform)bt.transform;
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
        brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
        bt.raycastTarget = false;

        btnGO.GetComponent<Button>().onClick.AddListener(() => onClick());
    }

    Text MakeText(string s, int size, TextAnchor align)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        var t = go.GetComponent<Text>();
        t.text = s; t.font = font; t.fontSize = size; t.color = Color.white; t.alignment = align;
        return t;
    }

    void SetControl(bool on)
    {
        if (playerController != null) playerController.enabled = on;
        if (playerInteractor != null) playerInteractor.enabled = on;
        if (playerCombat != null) playerCombat.enabled = on;
    }
}