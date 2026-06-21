using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// In-game pause menu, built in code (themed). Esc pauses (freezes time, frees the
// cursor, disables control). Buttons: Resume / Save / Quit to Menu.
public class PauseMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] SaveManager saveManager;
    [SerializeField] string mainMenuScene = "MainMenu";
    [SerializeField] Transform uiRoot;   // Canvas; auto-found if empty

    [Header("Disabled while paused")]
    [SerializeField] PlayerController playerController;
    [SerializeField] PlayerInteractor playerInteractor;
    [SerializeField] PlayerCombat playerCombat;

    public bool IsPaused { get; private set; }

    GameObject pausePanel;

    void Start()
    {
        if (uiRoot == null)
        {
            var c = FindAnyObjectByType<Canvas>();
            if (c != null) uiRoot = c.transform;
        }
        BuildUI();
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void BuildUI()
    {
        if (uiRoot == null) return;

        // Full-screen container (no image, so it isn't restyled as a panel).
        pausePanel = new GameObject("PauseMenu", typeof(RectTransform));
        var prt = (RectTransform)pausePanel.transform;
        prt.SetParent(uiRoot, false);
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;

        // Centered themed window.
        var window = new GameObject("Window", typeof(RectTransform), typeof(Image));
        var wrt = (RectTransform)window.transform;
        wrt.SetParent(prt, false);
        UIBuilder.SizeWindow(window, new Vector2(0.34f, 0.30f), new Vector2(0.66f, 0.72f));

        UIBuilder.AnchoredLabel(window.transform, "Paused", 44, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(320, 56), true);

        Btn(window.transform, "Resume", 50, Resume);
        Btn(window.transform, "Save", -20, SaveGame);
        Btn(window.transform, "Quit to Menu", -90, QuitToMenu);
    }

    void Btn(Transform parent, string label, float posY, System.Action onClick)
    {
        var b = UIBuilder.Button(parent, label, onClick, 24);
        var rt = (RectTransform)b.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, posY);
        rt.sizeDelta = new Vector2(260, 56);
    }

    void Update()
    {
        if (GameInput.Instance == null || !GameInput.Instance.PausePressed) return;

        if (IsPaused) { Resume(); return; }

        if (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen) return;
        if (UIWindow.Current != null) { UIWindow.Current.Close(); return; }
        Pause();
    }

    void Pause()
    {
        IsPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SetControl(false);
    }

    public void Resume()
    {
        IsPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SetControl(true);
    }

    public void SaveGame()
    {
        if (saveManager != null) saveManager.Save();
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    void SetControl(bool on)
    {
        if (playerController != null) playerController.enabled = on;
        if (playerInteractor != null) playerInteractor.enabled = on;
        if (playerCombat != null) playerCombat.enabled = on;
    }
}