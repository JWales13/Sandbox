using UnityEngine;

// Third-person + first-person controller.
// - The camera orbits the player on cameraPivot (yaw + pitch live on the pivot).
// - The player ROOT never rotates; movement is camera-relative.
// - The character MODEL turns to face the direction it's moving (third person),
//   or the camera's yaw (first person).
// All input comes from GameInput (keyboard/mouse + gamepad), never UnityEngine.Input.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float runSpeed = 7f;
    [SerializeField] float gravity = -20f;
    [SerializeField] float jumpHeight = 1.2f;
    [SerializeField] float turnSpeed = 720f;       // how fast the model turns toward movement (deg/sec)

    [Header("Look")]
    [SerializeField] float minPitch = -70f;
    [SerializeField] float maxPitch = 80f;

    [Header("Camera")]
    [SerializeField] Transform cameraPivot;        // child at head height; this is what rotates
    [SerializeField] Transform playerCamera;       // Main Camera, child of cameraPivot
    [SerializeField] Vector3 firstPersonOffset = new Vector3(0f, 0f, 0.1f);
    [SerializeField] Vector3 thirdPersonOffset = new Vector3(0f, 0.4f, -3.5f);

    [Header("Character")]
    [SerializeField] Animator animator;
    [SerializeField] GameObject characterModel;    // rotated to face movement; hidden in first person
    [SerializeField] float animationDamp = 0.1f;

    CharacterController controller;
    float yaw, pitch;
    float verticalVelocity;
    bool firstPerson = false;
    int speedHash;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        speedHash = Animator.StringToHash("Speed");
        yaw = transform.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
        ApplyCameraView();
    }

    void Update()
    {
        HandleLook();
        HandleMove();

        if (GameInput.Instance != null && GameInput.Instance.ToggleViewPressed)
        {
            firstPerson = !firstPerson;
            ApplyCameraView();
        }
    }

    void HandleLook()
    {
        Vector2 look = GameInput.Instance != null ? GameInput.Instance.LookDelta : Vector2.zero;
        yaw += look.x;
        pitch = Mathf.Clamp(pitch - look.y, minPitch, maxPitch);

        // The pivot owns both yaw and pitch, so the camera orbits without rotating the body.
        if (cameraPivot != null) cameraPivot.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMove()
    {
        Vector2 m = GameInput.Instance != null ? GameInput.Instance.Move : Vector2.zero;
        Vector3 input = new Vector3(m.x, 0f, m.y);
        if (input.sqrMagnitude > 1f) input.Normalize();

        // Camera-relative movement, flattened to the horizontal plane.
        Vector3 moveDir = Quaternion.Euler(0f, yaw, 0f) * input;

        bool sprint = GameInput.Instance != null && GameInput.Instance.SprintHeld;
        float speed = sprint ? runSpeed : walkSpeed;
        if (Stats.Instance != null) speed *= Stats.Instance.Get(StatType.MoveSpeed);   // Agility + MoveSpeed perks
        Vector3 velocity = moveDir * speed;

        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
            if (GameInput.Instance != null && GameInput.Instance.JumpPressed)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        // Turn the character model.
        if (characterModel != null)
        {
            if (firstPerson)
            {
                characterModel.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            }
            else if (moveDir.sqrMagnitude > 0.001f)
            {
                Quaternion target = Quaternion.LookRotation(new Vector3(moveDir.x, 0f, moveDir.z));
                characterModel.transform.rotation = Quaternion.RotateTowards(
                    characterModel.transform.rotation, target, turnSpeed * Time.deltaTime);
            }
        }

        if (animator != null)
        {
            float planar = new Vector3(velocity.x, 0f, velocity.z).magnitude;
            animator.SetFloat(speedHash, planar, animationDamp, Time.deltaTime);
        }
    }

    void ApplyCameraView()
    {
        if (playerCamera != null)
            playerCamera.localPosition = firstPerson ? firstPersonOffset : thirdPersonOffset;

        if (characterModel != null)
            foreach (var r in characterModel.GetComponentsInChildren<Renderer>())
                r.enabled = !firstPerson;
    }

    // The direction the character is currently facing (used by combat, etc.).
    public Vector3 FacingDirection =>
        characterModel != null ? characterModel.transform.forward : transform.forward;
}