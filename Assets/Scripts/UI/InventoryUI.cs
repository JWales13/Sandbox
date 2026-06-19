using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Self-building inventory grid (toggle I). Sizes itself and builds a title, slot
// grid, and close button via UIBuilder. Clicking an edible item eats it to heal.
public class InventoryUI : MonoBehaviour
{
    [Header("Data")]
    public Inventory inventory;

    [Header("References")]
    public GameObject panel;
    public KeyCode toggleKey = KeyCode.I;

    [Header("Disabled while open")]
    public PlayerController playerController;
    public PlayerInteractor playerInteractor;

    [Header("Style")]
    public Vector2 cellSize = new Vector2(96, 96);

    RectTransform slotGrid;
    readonly List<GameObject> cells = new List<GameObject>();
    bool built, isOpen;

    void Start()
    {
        if (inventory == null) inventory = Inventory.Instance;
        BuildStatic();
        if (inventory != null) inventory.OnChanged += Refresh;
        if (panel != null) panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (inventory != null) inventory.OnChanged -= Refresh;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) Toggle();
    }

    void BuildStatic()
    {
        if (panel == null || built) return;
        built = true;

        UIBuilder.SizeWindow(panel, new Vector2(0.22f, 0.16f), new Vector2(0.78f, 0.84f));

        UIBuilder.AnchoredLabel(panel.transform, "Inventory", 30, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0, -16), new Vector2(500, 40), true);

        slotGrid = UIBuilder.Area(panel.transform, "SlotGrid",
            new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.82f), Vector4.zero);
        var grid = slotGrid.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = cellSize;
        grid.spacing = new Vector2(8, 8);
        grid.padding = new RectOffset(10, 10, 10, 10);

        var close = UIBuilder.Button(panel.transform, "Close", Close);
        var crt = (RectTransform)close.transform;
        crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0f);
        crt.anchoredPosition = new Vector2(0, 16);
        crt.sizeDelta = new Vector2(150, 38);
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        if (panel != null) panel.SetActive(isOpen);

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;
        if (playerController != null) playerController.enabled = !isOpen;
        if (playerInteractor != null) playerInteractor.enabled = !isOpen;

        if (isOpen) Refresh();
    }

    public void Close()
    {
        isOpen = false;
        if (panel != null) panel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController != null) playerController.enabled = true;
        if (playerInteractor != null) playerInteractor.enabled = true;
    }

    void Refresh()
    {
        if (inventory == null || slotGrid == null) return;

        while (cells.Count < inventory.slots.Count) cells.Add(CreateCell(cells.Count));

        for (int i = 0; i < cells.Count; i++)
        {
            bool used = i < inventory.slots.Count;
            cells[i].SetActive(used);
            if (!used) continue;

            var slot = inventory.slots[i];
            var icon = cells[i].transform.GetChild(0).GetComponent<Image>();
            var label = cells[i].transform.GetChild(1).GetComponent<Text>();

            if (slot.IsEmpty) { icon.enabled = false; label.text = ""; }
            else
            {
                bool hasIcon = slot.item.icon != null;
                icon.enabled = hasIcon;
                if (hasIcon) icon.sprite = slot.item.icon;
                label.text = slot.count > 1 ? $"{slot.item.displayName}\nx{slot.count}" : slot.item.displayName;
            }
        }
    }

    void UseSlot(int index)
    {
        if (inventory == null || index < 0 || index >= inventory.slots.Count) return;
        var slot = inventory.slots[index];
        if (slot.IsEmpty || !slot.item.isEdible) return;

        var hp = PlayerHealth.Instance;
        if (hp == null || hp.CurrentHealth >= hp.MaxHealth) return;   // don't waste food at full

        hp.Heal(slot.item.healthRestore);
        inventory.Remove(slot.item, 1);
    }

    GameObject CreateCell(int index)
    {
        var cell = new GameObject("Slot", typeof(RectTransform), typeof(Image), typeof(Button));
        cell.transform.SetParent(slotGrid, false);
        UITheme.StyleSlot(cell.GetComponent<Image>());
        cell.GetComponent<Button>().onClick.AddListener(() => UseSlot(index));

        var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        var irt = (RectTransform)iconGO.transform;
        irt.SetParent(cell.transform, false);
        irt.anchorMin = Vector2.zero; irt.anchorMax = Vector2.one;
        irt.offsetMin = new Vector2(6, 6); irt.offsetMax = new Vector2(-6, -6);
        iconGO.GetComponent<Image>().raycastTarget = false;

        var labelGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
        var lrt = (RectTransform)labelGO.transform;
        lrt.SetParent(cell.transform, false);
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        var txt = labelGO.GetComponent<Text>();
        UITheme.StyleText(txt, 13);
        txt.alignment = TextAnchor.LowerCenter;
        txt.resizeTextForBestFit = true;
        txt.resizeTextMinSize = 8;
        txt.resizeTextMaxSize = 14;
        txt.raycastTarget = false;

        return cell;
    }
}