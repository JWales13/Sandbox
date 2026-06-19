using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Code-built, themed main menu: New Game / Load Game / Quit, plus a 3-slot picker.
// Needs only a Canvas (assign uiRoot, or it auto-finds one) and the game scene name.
public class MainMenuController : MonoBehaviour
{
    [Tooltip("Exact name of the gameplay scene (must be in Build Settings).")]
    public string gameSceneName = "GameWorld";
    public string gameTitle = "Isekai Sandbox RPG";
    public Transform uiRoot;   // a Canvas; auto-found if empty

    GameObject mainPanel, slotPanel;
    bool slotNewGameMode;
    readonly Button[] slotButtons = new Button[SaveSlots.Count];
    readonly Text[] slotLabels = new Text[SaveSlots.Count];

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (uiRoot == null)
        {
            var c = FindAnyObjectByType<Canvas>();
            if (c != null) uiRoot = c.transform;
        }
        if (uiRoot == null) return;

        BuildMain();
        BuildSlots();
        ShowMain();
    }

    // ---- Main panel ----

    void BuildMain()
    {
        mainPanel = FullScreen("MainMenuPanel");

        UIBuilder.AnchoredLabel(mainPanel.transform, gameTitle, 54, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0, -140), new Vector2(1000, 90), true);

        MenuButton(mainPanel.transform, "New Game", 50, () => ShowSlots(true));
        MenuButton(mainPanel.transform, "Load Game", -30, () => ShowSlots(false));
        MenuButton(mainPanel.transform, "Quit", -110, Quit);
    }

    // ---- Slot picker ----

    void BuildSlots()
    {
        slotPanel = FullScreen("SlotPanel");

        UIBuilder.AnchoredLabel(slotPanel.transform, "Select Slot", 44, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0, -140), new Vector2(800, 70), true);

        for (int i = 0; i < SaveSlots.Count; i++)
        {
            int slot = i;
            var b = MenuButton(slotPanel.transform, $"Slot {i + 1}", 80 - i * 80, () => OnSlot(slot));
            ((RectTransform)b.transform).sizeDelta = new Vector2(460, 64);
            slotButtons[i] = b;
            slotLabels[i] = b.GetComponentInChildren<Text>();
        }

        MenuButton(slotPanel.transform, "Back", -200, ShowMain);
    }

    void ShowMain()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (slotPanel != null) slotPanel.SetActive(false);
    }

    void ShowSlots(bool newGame)
    {
        slotNewGameMode = newGame;
        for (int i = 0; i < SaveSlots.Count; i++)
        {
            slotLabels[i].text = $"Slot {i + 1}:  {SaveSlots.Summary(i)}";
            slotButtons[i].interactable = newGame || SaveSlots.Exists(i);  // Load: only occupied slots
        }
        if (mainPanel != null) mainPanel.SetActive(false);
        if (slotPanel != null) slotPanel.SetActive(true);
    }

    void OnSlot(int slot)
    {
        GameSession.CurrentSlot = slot;

        if (slotNewGameMode)
        {
            SaveSlots.Delete(slot);          // fresh start overwrites the slot
            GameSession.LoadOnStart = false;
        }
        else
        {
            if (!SaveSlots.Exists(slot)) return;
            GameSession.LoadOnStart = true;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    void Quit()
    {
        Application.Quit();
        Debug.Log("Quit pressed (no effect in the editor).");
    }

    // ---- helpers ----

    GameObject FullScreen(string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(uiRoot, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return go;
    }

    Button MenuButton(Transform parent, string label, float posY, System.Action onClick)
    {
        var b = UIBuilder.Button(parent, label, onClick, 28);
        var rt = (RectTransform)b.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, posY);
        rt.sizeDelta = new Vector2(360, 64);
        return b;
    }
}