using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// A self-building inventory grid. Press the toggle key to open it; it draws one
// cell per slot (icon if the item has one, otherwise the name) and refreshes
// whenever the inventory changes. Adds a GridLayoutGroup to the container itself.
public class InventoryUI : MonoBehaviour
{
    [Header("Data")]
    public Inventory inventory;

    [Header("Scene references")]
    public GameObject rootPanel;        // window root (hidden until opened)
    public RectTransform slotContainer; // gets a GridLayoutGroup; cells go here
    public Text titleLabel;             // optional header text
    public KeyCode toggleKey = KeyCode.I;

    [Header("Disabled while open")]
    public PlayerController playerController;
    public PlayerInteractor playerInteractor;

    [Header("Style")]
    public Vector2 cellSize = new Vector2(90, 90);

    readonly List<GameObject> cells = new List<GameObject>();
    Font font;
    bool isOpen;

    void Start()
    {
        if (inventory == null) inventory = Inventory.Instance;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        EnsureGrid();
        if (inventory != null) inventory.OnChanged += Refresh;
        if (rootPanel != null) rootPanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (inventory != null) inventory.OnChanged -= Refresh;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) Toggle();
    }

    void EnsureGrid()
    {
        if (slotContainer == null) return;
        var grid = slotContainer.GetComponent<GridLayoutGroup>();
        if (grid == null) grid = slotContainer.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = cellSize;
        grid.spacing = new Vector2(6, 6);
        grid.padding = new RectOffset(8, 8, 8, 8);
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        if (rootPanel != null) rootPanel.SetActive(isOpen);

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;
        if (playerController != null) playerController.enabled = !isOpen;
        if (playerInteractor != null) playerInteractor.enabled = !isOpen;

        if (isOpen) Refresh();
    }

    void Refresh()
    {
        if (inventory == null || slotContainer == null) return;
        if (titleLabel != null) titleLabel.text = "Inventory";

        while (cells.Count < inventory.slots.Count) cells.Add(CreateCell());

        for (int i = 0; i < cells.Count; i++)
        {
            bool used = i < inventory.slots.Count;
            cells[i].SetActive(used);
            if (!used) continue;

            var slot = inventory.slots[i];
            var icon = cells[i].transform.GetChild(0).GetComponent<Image>();
            var label = cells[i].transform.GetChild(1).GetComponent<Text>();

            if (slot.IsEmpty)
            {
                icon.enabled = false;
                label.text = "";
            }
            else
            {
                bool hasIcon = slot.item.icon != null;
                icon.enabled = hasIcon;
                if (hasIcon) icon.sprite = slot.item.icon;
                label.text = slot.count > 1 ? $"{slot.item.displayName}\nx{slot.count}" : slot.item.displayName;
            }
        }
    }

    GameObject CreateCell()
    {
        var cell = new GameObject("Slot", typeof(RectTransform), typeof(Image));
        cell.transform.SetParent(slotContainer, false);
        cell.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.4f);

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
        txt.font = font;
        txt.alignment = TextAnchor.LowerCenter;
        txt.color = Color.white;
        txt.resizeTextForBestFit = true;
        txt.resizeTextMinSize = 8;
        txt.resizeTextMaxSize = 15;
        txt.raycastTarget = false;

        return cell;
    }
}