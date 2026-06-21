using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Central input reader. Owns every gameplay action (keyboard/mouse AND gamepad)
// so no other script ever touches UnityEngine.Input. Actions are defined in code
// here — one file to paste, nothing to wire in the Inspector except dropping this
// component on a persistent object.
//
// Requires the Input System package, with Active Input Handling = "Both" (so the
// legacy keyboard menu toggles keep working during the migration).
public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    [Header("Look sensitivity")]
    [Tooltip("Multiplier on raw mouse delta (pixels). Tune to taste.")]
    [SerializeField] float mouseLookSensitivity = 0.05f;
    [Tooltip("Right-stick look speed in degrees per second.")]
    [SerializeField] float gamepadLookSpeed = 220f;

    // ---- Read this from gameplay scripts ----
    public Vector2 Move { get; private set; }
    public Vector2 LookDelta { get; private set; }   // already scaled: degrees to add THIS frame
    public bool SprintHeld { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool ToggleViewPressed { get; private set; }
    public bool PausePressed { get; private set; }
    public bool SubmitPressed { get; private set; }   // advance dialogue / confirm (click, Enter, South)

    // Radial menu: held while the button is down; events fire on the edges.
    public bool RadialHeld { get; private set; }
    public event Action RadialOpened;
    public event Action RadialClosed;
    public Vector2 RadialStick { get; private set; }   // left stick / mouse-free direction for the wheel

    InputAction move, look, sprint, jump, attack, interact, toggleView, pause, radial, submit;

    void Awake()
    {
        Instance = this;

        move = new InputAction("Move", InputActionType.Value);
        move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        move.AddBinding("<Gamepad>/leftStick");

        look = new InputAction("Look", InputActionType.Value);
        look.AddBinding("<Mouse>/delta");
        look.AddBinding("<Gamepad>/rightStick");

        sprint = new InputAction("Sprint", InputActionType.Button);
        sprint.AddBinding("<Keyboard>/leftShift");
        sprint.AddBinding("<Gamepad>/leftStickPress");

        jump = new InputAction("Jump", InputActionType.Button);
        jump.AddBinding("<Keyboard>/space");
        jump.AddBinding("<Gamepad>/buttonSouth");

        attack = new InputAction("Attack", InputActionType.Button);
        attack.AddBinding("<Mouse>/leftButton");
        attack.AddBinding("<Gamepad>/rightTrigger");

        interact = new InputAction("Interact", InputActionType.Button);
        interact.AddBinding("<Keyboard>/e");
        interact.AddBinding("<Gamepad>/buttonWest");

        toggleView = new InputAction("ToggleView", InputActionType.Button);
        toggleView.AddBinding("<Keyboard>/v");
        toggleView.AddBinding("<Gamepad>/buttonNorth");

        pause = new InputAction("Pause", InputActionType.Button);
        pause.AddBinding("<Keyboard>/escape");
        pause.AddBinding("<Gamepad>/start");

        // Hold to open the radial menu (controller: L shoulder; keyboard: Tab).
        radial = new InputAction("Radial", InputActionType.Button);
        radial.AddBinding("<Gamepad>/leftShoulder");
        radial.AddBinding("<Keyboard>/tab");

        // Confirm / advance dialogue (mouse click, Enter, or the South button).
        submit = new InputAction("Submit", InputActionType.Button);
        submit.AddBinding("<Mouse>/leftButton");
        submit.AddBinding("<Keyboard>/enter");
        submit.AddBinding("<Gamepad>/buttonSouth");
    }

    void OnEnable()
    {
        move.Enable(); look.Enable(); sprint.Enable(); jump.Enable(); attack.Enable();
        interact.Enable(); toggleView.Enable(); pause.Enable(); radial.Enable(); submit.Enable();
    }

    void OnDisable()
    {
        move.Disable(); look.Disable(); sprint.Disable(); jump.Disable(); attack.Disable();
        interact.Disable(); toggleView.Disable(); pause.Disable(); radial.Disable(); submit.Disable();
    }

    void OnDestroy()
    {
        move.Dispose(); look.Dispose(); sprint.Dispose(); jump.Dispose(); attack.Dispose();
        interact.Dispose(); toggleView.Dispose(); pause.Dispose(); radial.Dispose(); submit.Dispose();
    }

    void Update()
    {
        Move = move.ReadValue<Vector2>();

        // Mouse delta is per-frame pixels; right stick is a -1..1 value, so it
        // needs deltaTime + a speed. Detect which device produced the input.
        Vector2 rawLook = look.ReadValue<Vector2>();
        bool gamepadLook = look.activeControl != null && look.activeControl.device is Gamepad;
        LookDelta = gamepadLook
            ? rawLook * (gamepadLookSpeed * Time.unscaledDeltaTime)
            : rawLook * mouseLookSensitivity;

        SprintHeld = sprint.IsPressed();
        JumpPressed = jump.WasPressedThisFrame();
        AttackPressed = attack.WasPressedThisFrame();
        InteractPressed = interact.WasPressedThisFrame();
        ToggleViewPressed = toggleView.WasPressedThisFrame();
        PausePressed = pause.WasPressedThisFrame();
        SubmitPressed = submit.WasPressedThisFrame();

        RadialStick = move.ReadValue<Vector2>();   // left stick drives the wheel
        if (radial.WasPressedThisFrame()) { RadialHeld = true; RadialOpened?.Invoke(); }
        if (radial.WasReleasedThisFrame()) { RadialHeld = false; RadialClosed?.Invoke(); }
    }
}