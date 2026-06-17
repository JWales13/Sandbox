using UnityEngine;
 
// Milestone 2: walk/run with animation + toggle first/third person.
// Attach to your Player object (Capsule with a CharacterController).
// Keys: WASD move, Shift run, Space jump, Mouse look, V toggle view, Esc free cursor.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float gravity = -20f;
    public float jumpHeight = 1.2f;
 
    [Header("Look")]
    public float mouseSensitivity = 2f;
    public float minPitch = -80f;
    public float maxPitch = 80f;
 
    [Header("Camera")]
    public Transform cameraPivot;       // empty at head height, child of Player
    public Transform playerCamera;      // Main Camera, child of cameraPivot
    public Vector3 firstPersonOffset = new Vector3(0f, 0f, 0.1f);
    public Vector3 thirdPersonOffset = new Vector3(0f, 0.4f, -3.5f);
    public KeyCode toggleViewKey = KeyCode.V;
 
    [Header("Character")]
    public Animator animator;           // the character model's Animator
    public GameObject characterModel;   // the character model root (to hide in first person)
    public float animationDamp = 0.1f;  // smooths the walk/run blend
 
    CharacterController controller;
    float pitch = 0f;
    float verticalVelocity = 0f;
    bool firstPerson = false;
    int speedHash;
 
    void Start()
    {
        controller = GetComponent<CharacterController>();
        speedHash = Animator.StringToHash("Speed");
        Cursor.lockState = CursorLockMode.Locked;
        ApplyCameraView();
    }
 
    void Update()
    {
        HandleLook();
        HandleMove();
 
        if (Input.GetKeyDown(toggleViewKey))
        {
            firstPerson = !firstPerson;
            ApplyCameraView();
        }
 
        if (Input.GetKeyDown(KeyCode.Escape))
            Cursor.lockState = CursorLockMode.None;
    }
 
    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
 
        transform.Rotate(Vector3.up * mouseX);
        pitch = Mathf.Clamp(pitch - mouseY, minPitch, maxPitch);
        cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }
 
    void HandleMove()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = (transform.right * h + transform.forward * v).normalized;
 
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        Vector3 velocity = move * speed;
 
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
 
        // Feed horizontal speed to the animator's blend tree.
        if (animator != null)
        {
            float planarSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
            animator.SetFloat(speedHash, planarSpeed, animationDamp, Time.deltaTime);
        }
    }
 
    void ApplyCameraView()
    {
        playerCamera.localPosition = firstPerson ? firstPersonOffset : thirdPersonOffset;
 
        // Hide the body in first person so the camera isn't inside the mesh.
        if (characterModel != null)
        {
            foreach (var r in characterModel.GetComponentsInChildren<Renderer>())
                r.enabled = !firstPerson;
        }
    }
}