using UnityEngine;
using UnityEngine.UI;

// Put this on the Player. Casts a ray from the camera through the screen
// center (a crosshair), finds the nearest Interactable within range of the
// player, shows a prompt, and triggers it on the interact key. Works in FP and TP.
public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    public Camera viewCamera;             // the Main Camera (drag it in)
    public float interactRange = 3f;      // how close the object must be to the player
    public float maxScanDistance = 12f;   // how far the ray itself reaches
    public LayerMask interactMask = ~0;   // ~0 = everything
    public KeyCode interactKey = KeyCode.E;

    [Header("UI (optional)")]
    public Text promptText;               // a Legacy UI Text; leave empty to skip

    Interactable current;

    void Start()
    {
        if (viewCamera == null) viewCamera = Camera.main;
        HidePrompt();
    }

    void Update()
    {
        // Don't scan for new targets while a conversation is open, OR on the
        // same frame it just closed (so the closing key press isn't reused).
        if (DialogueUI.Instance != null && DialogueUI.Instance.BlockingInput)
        {
            current = null;
            HidePrompt();
            return;
        }

        FindInteractable();

        if (current != null && Input.GetKeyDown(interactKey))
            current.Interact(gameObject);
    }

    void FindInteractable()
    {
        Interactable found = null;

        if (viewCamera != null)
        {
            Ray ray = viewCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * maxScanDistance, Color.cyan);

            RaycastHit[] hits = Physics.RaycastAll(ray, maxScanDistance, interactMask, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                // Skip the player's own colliders (important in third person).
                if (hit.collider.transform.IsChildOf(transform)) continue;

                // First real object the crosshair hits: if it's too far, stop;
                // if it's an interactable in range, use it; otherwise it blocks the view.
                if (Vector3.Distance(transform.position, hit.point) > interactRange) break;
                found = hit.collider.GetComponentInParent<Interactable>();
                break;
            }
        }

        current = found;
        if (current != null) ShowPrompt(current.GetPrompt()); // refreshed live for dynamic prompts
        else HidePrompt();
    }

    void ShowPrompt(string text)
    {
        if (promptText == null) return;
        promptText.text = $"[{interactKey}] {text}";
        promptText.enabled = true;
    }

    void HidePrompt()
    {
        if (promptText != null) promptText.enabled = false;
    }
}
