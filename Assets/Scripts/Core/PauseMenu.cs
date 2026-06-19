using UnityEngine;
using UnityEngine.SceneManagement;

// In-game pause menu. Esc toggles pause: freezes time, frees the cursor, and
// disables player control. Buttons call Resume / SaveGame / QuitToMenu.
public class PauseMenu : MonoBehaviour
{
    [Header("References")]
    public GameObject pausePanel;
    public SaveManager saveManager;
    public string mainMenuScene = "MainMenu";

    [Header("Disabled while paused")]
    public PlayerController playerController;
    public PlayerInteractor playerInteractor;
    public PlayerCombat playerCombat;

    public KeyCode pauseKey = KeyCode.Escape;
    public bool IsPaused { get; private set; }

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void Update()
    {
        if (!Input.GetKeyDown(pauseKey)) return;

        if (IsPaused) { Resume(); return; }

        // Esc closes whatever is open first; only pauses if nothing else is.
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen) return;
        if (UIWindow.Current != null) { UIWindow.Current.Close(); return; }
        Pause();
    }

    public void Toggle()
    {
        if (IsPaused) Resume(); else Pause();
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
        Time.timeScale = 1f;            // MUST restore, or the menu scene is frozen too
        SceneManager.LoadScene(mainMenuScene);
    }

    void SetControl(bool on)
    {
        if (playerController != null) playerController.enabled = on;
        if (playerInteractor != null) playerInteractor.enabled = on;
        if (playerCombat != null) playerCombat.enabled = on;
    }
}