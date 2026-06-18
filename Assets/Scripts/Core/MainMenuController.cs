using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Drives the main menu. Wire the three buttons' OnClick to NewGame / Continue / Quit.
public class MainMenuController : MonoBehaviour
{
    [Tooltip("Exact name of your gameplay scene (must be added to Build Settings).")]
    public string gameSceneName = "SampleScene";

    [Tooltip("Continue is disabled when no save file exists.")]
    public Button continueButton;

    string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    void Start()
    {
        // Menu uses the mouse.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (continueButton != null)
            continueButton.interactable = File.Exists(SavePath);
    }

    public void NewGame()
    {
        GameSession.LoadOnStart = false;   // start fresh
        SceneManager.LoadScene(gameSceneName);
    }

    public void Continue()
    {
        GameSession.LoadOnStart = true;    // load the save once the scene is up
        SceneManager.LoadScene(gameSceneName);
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit pressed (no effect in the editor).");
    }
}