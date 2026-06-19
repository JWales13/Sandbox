using UnityEngine;

// Third-person + first-person controller.
// - The camera orbits the player on cameraPivot (yaw + pitch live on the pivot).
// - The player ROOT never rotates; movement is camera-relative.
// - The character MODEL turns to face the direction it's moving (third person),
//   or the camera's yaw (first person).
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float gravity = -20f;
    public float jumpHeight = 1.2f;
    public float turnSpeed = 720f;       // how fast the model turns toward movement (deg/sec)

    [Header("Look")]
    public float mouseSensitivity = 2f;
    public float minPitch = -70f;
    public float maxPitch = 80f;

    [Header("Camera")]
    public Transform cameraPivot;        // child at head height; this is what rotates
    public Transform playerCamera;       // Main Camera, child of cameraPivot
    public Vector3 firstPersonOffset = new Vector3(0f, 0f, 0.1f);
    public Vector3 thirdPersonOffset = new Vector3(0f, 0.4f, -3.5f);
    public KeyCode toggleViewKey = KeyCode.V;

    [Header("Character")]
    public Animator animator;
    public GameObject characterModel;    // rotated to face movement; hidden in first person
    public float animationDamp = 0.1f;

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

        if (Input.GetKeyDown(toggleViewKey)) { firstPerson = !firstPerson; ApplyCameraView(); }
    }

    void HandleLook()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * mouseSensitivity, minPitch, maxPitch);

        // The pivot owns both yaw and pitch, so the camera orbits without rotating the body.
        if (cameraPivot != null) cameraPivot.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMove()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(h, 0f, v);
        if (input.sqrMagnitude > 1f) input.Normalize();

        // Camera-relative movement, flattened to the horizontal plane.
        Vector3 moveDir = Quaternion.Euler(0f, yaw, 0f) * input;

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        if (Stats.Instance != null) speed *= Stats.Instance.Get(StatType.MoveSpeed);   // Agility + MoveSpeed perks
        Vector3 velocity = moveDir * speed;

        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
            if (Input.GetKeyDown(KeyCode.Space))
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