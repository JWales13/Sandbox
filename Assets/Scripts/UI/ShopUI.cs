using UnityEngine;
using UnityEngine.UI;

// Shop window. Open(shop) sets the merchant then opens via UIWindow.
public class ShopUI : UIWindow
{
    public static ShopUI Instance { get; private set; }

    Shopkeeper current;
    Text titleLabel, coinsLabel;
    RectTransform buyList, sellList;

    void Awake() { Instance = this; }

    protected override void Start()
    {
        base.Start();
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
        base.Open();
    }

    protected override void OnOpened()
    {
        if (titleLabel != null) titleLabel.text = current != null ? current.shopName : "Shop";
        Refresh();
    }

    protected override void OnClosed() => current = null;

    protected override void Build()
    {
        if (panel == null) return;
        UIBuilder.SizeWindow(panel, new Vector2(0.15f, 0.12f), new Vector2(0.85f, 0.88f));

        titleLabel = UIBuilder.AnchoredLabel(panel.transform, "Shop", 30, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0, -16), new Vector2(500, 40), true);
        coinsLabel = UIBuilder.AnchoredLabel(panel.transform, "Coins: 0", 18, TextAnchor.MiddleRight,
            new Vector2(1f, 1f), new Vector2(-24, -18), new Vector2(240, 30));

        UIBuilder.AnchoredLabel(panel.transform, "Buy", 18, TextAnchor.MiddleCenter,
            new Vector2(0.26f, 0.83f), Vector2.zero, new Vector2(160, 28), true);
        UIBuilder.AnchoredLabel(panel.transform, "Sell", 18, TextAnchor.MiddleCenter,
            new Vector2(0.74f, 0.83f), Vector2.zero, new Vector2(160, 28), true);

        buyList = UIBuilder.VerticalList(panel.transform, "BuyList",
            new Vector2(0.04f, 0.12f), new Vector2(0.48f, 0.80f), Vector4.zero);
        sellList = UIBuilder.VerticalList(panel.transform, "SellList",
            new Vector2(0.52f, 0.12f), new Vector2(0.96f, 0.80f), Vector4.zero);

        var close = UIBuilder.Button(panel.transform, "Close", Close);
        var crt = (RectTransform)close.transform;
        crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0f);
        crt.anchoredPosition = new Vector2(0, 16);
        crt.sizeDelta = new Vector2(150, 38);
    }

    void Refresh()
    {
        if (panel == null || !panel.activeSelf) return;

        if (coinsLabel != null && Wallet.Instance != null)
            coinsLabel.text = $"Coins: {Wallet.Instance.coins}";

        UIBuilder.Clear(buyList);
        if (current != null)
            foreach (var item in current.stock)
                if (item != null)
                {
                    var captured = item;
                    bool afford = Wallet.Instance != null && Wallet.Instance.CanAfford(item.buyPrice);
                    UIBuilder.Row(buyList, $"{item.displayName}   ({item.buyPrice}g)", "Buy", afford, () => Buy(captured));
                }

        UIBuilder.Clear(sellList);
        if (Inventory.Instance != null)
            foreach (var slot in Inventory.Instance.slots)
                if (!slot.IsEmpty)
                {
                    var captured = slot.item;
                    UIBuilder.Row(sellList, $"{slot.item.displayName} x{slot.count}   ({slot.item.sellPrice}g)", "Sell", true, () => Sell(captured));
                }
    }

    void Buy(ItemSO item)
    {
        if (Wallet.Instance == null || Inventory.Instance == null) return;
        if (!Wallet.Instance.CanAfford(item.buyPrice)) return;
        int leftover = Inventory.Instance.Add(item, 1);
        if (leftover == 0) Wallet.Instance.Spend(item.buyPrice);
    }

    void Sell(ItemSO item)
    {
        if (Wallet.Instance == null || Inventory.Instance == null) return;
        if (!Inventory.Instance.Remove(item, 1)) return;

        int price = item.sellPrice;
        if (PlayerProgression.Instance != null)
            price = Mathf.RoundToInt(price * (1f + PlayerProgression.Instance.GetStat(StatType.SellPrice)));
        Wallet.Instance.Add(price);
    }
}