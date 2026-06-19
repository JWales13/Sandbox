using UnityEngine;

// Base for full-screen menu windows. Handles the shared dance: show/hide the
// panel, free/lock the cursor, disable/enable player control, and ensure only
// one window is open at a time. Subclasses implement Build() (construct the UI
// once) and may override OnOpened/OnClosed (e.g. to refresh contents).
public abstract class UIWindow : MonoBehaviour
{
    [Header("Window")]
    public GameObject panel;
    public PlayerController playerController;
    public PlayerInteractor playerInteractor;
    public PlayerCombat playerCombat;

    public static UIWindow Current { get; private set; }   // the currently open window, if any
    public bool IsOpen { get; private set; }

    bool built;

    protected virtual void Start()
    {
        EnsureBuilt();
        if (panel != null) panel.SetActive(false);
    }

    void EnsureBuilt()
    {
        if (built) return;
        built = true;
        Build();
    }

    protected abstract void Build();          // construct the static UI (once)
    protected virtual void OnOpened() { }     // e.g. refresh contents
    protected virtual void OnClosed() { }

    public void Open()
    {
        if (IsOpen) return;
        if (Current != null && Current != this) Current.Close();   // only one at a time

        EnsureBuilt();
        IsOpen = true;
        Current = this;
        if (panel != null) panel.SetActive(true);
        SetCursor(true);
        SetControl(false);
        OnOpened();
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        if (Current == this) Current = null;
        if (panel != null) panel.SetActive(false);
        SetCursor(false);
        SetControl(true);
        OnClosed();
    }

    public void Toggle()
    {
        if (IsOpen) Close(); else Open();
    }

    void SetCursor(bool free)
    {
        Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = free;
    }

    void SetControl(bool on)
    {
        if (playerController != null) playerController.enabled = on;
        if (playerInteractor != null) playerInteractor.enabled = on;
        if (playerCombat != null) playerCombat.enabled = on;
    }
}