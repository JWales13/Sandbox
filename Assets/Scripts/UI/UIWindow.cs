using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Base for full-screen menu windows. Handles the shared dance: show/hide the
// panel, free/lock the cursor, disable/enable player control, and ensure only
// one window is open at a time. Subclasses implement Build() (construct the UI
// once) and may override OnOpened/OnClosed (e.g. to refresh contents).
public abstract class UIWindow : MonoBehaviour
{
    [Header("Window")]
    [SerializeField] protected GameObject panel;   // protected: subclasses build into it
    [SerializeField] PlayerController playerController;
    [SerializeField] PlayerInteractor playerInteractor;
    [SerializeField] PlayerCombat playerCombat;

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
        SelectFirst();                 // give the controller something to navigate from
    }

    // Focus the first button/selectable so the gamepad stick/d-pad can navigate.
    void SelectFirst()
    {
        if (EventSystem.current == null || panel == null) return;
        var first = panel.GetComponentInChildren<Selectable>(false);
        EventSystem.current.SetSelectedGameObject(first != null ? first.gameObject : null);
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        if (Current == this) Current = null;
        if (panel != null) panel.SetActive(false);
        SetCursor(false);
        SetControl(true);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
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