using UnityEngine;
using UnityEngine.UI;

// Drives a simple dialogue panel: shows a speaker name + one line at a time,
// advancing on E or left-click, closing after the last line, on Esc, or when
// the player walks away from the speaker.
public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("References")]
    public GameObject panel;     // the dialogue panel root (starts hidden)
    public Text nameText;        // speaker name label
    public Text lineText;        // the current line of dialogue

    [Header("Behaviour")]
    public float autoCloseDistance = 4f;  // end the talk if player gets this far away

    public bool IsOpen { get; private set; }

    // True while open OR on the exact frame it closed, so other scripts don't
    // act on the same key press that closed the dialogue.
    public bool BlockingInput => IsOpen || Time.frameCount == closedFrame;

    string[] lines;
    int index;
    bool openedThisFrame;
    int closedFrame = -1;
    Transform speaker;
    Transform player;

    void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
    }

    void Start()
    {
        var interactor = FindAnyObjectByType<PlayerInteractor>();
        if (interactor != null) player = interactor.transform;
    }

    void Update()
    {
        if (!IsOpen) return;

        // Auto-close if the player walks away from the speaker.
        if (speaker != null && player != null &&
            Vector3.Distance(player.position, speaker.position) > autoCloseDistance)
        {
            Close();
            return;
        }

        // Ignore the same key press that opened the dialogue this frame.
        if (openedThisFrame) { openedThisFrame = false; return; }

        var gi = GameInput.Instance;
        if (gi == null) return;

        if (gi.PausePressed) { Close(); return; }                 // Esc / + closes
        if (gi.InteractPressed || gi.SubmitPressed) Advance();     // E / Y / click / Enter / B
    }

    public void StartDialogue(string speakerName, string[] dialogueLines, Transform speakerTransform = null)
    {
        // Guard: don't restart a conversation that's already running.
        if (IsOpen) return;
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        lines = dialogueLines;
        index = 0;
        speaker = speakerTransform;
        IsOpen = true;
        openedThisFrame = true;

        if (panel != null) panel.SetActive(true);
        if (nameText != null) nameText.text = speakerName;
        ShowLine();
    }

    void Advance()
    {
        index++;
        if (index >= lines.Length) { Close(); return; }
        ShowLine();
    }

    void ShowLine()
    {
        if (lineText != null) lineText.text = lines[index];
    }

    void Close()
    {
        IsOpen = false;
        closedFrame = Time.frameCount;
        if (panel != null) panel.SetActive(false);
    }
}