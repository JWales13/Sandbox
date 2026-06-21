using UnityEngine;

// A floating damage number, built entirely in code (no prefab). Spawns at the
// hit point, drifts up, billboards to the camera, and fades out.
public class DamagePopup : MonoBehaviour
{
    public static void Spawn(Vector3 worldPos, int amount)
    {
        var go = new GameObject("DamagePopup");
        go.transform.position = worldPos + Vector3.up * 0.3f;

        var tm = go.AddComponent<TextMesh>();
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.font = font;
        tm.text = amount.ToString();
        tm.fontSize = 48;
        tm.characterSize = 0.12f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.white;
        go.GetComponent<MeshRenderer>().material = font.material;

        go.AddComponent<DamagePopup>();
    }

    [SerializeField] float lifetime = 0.7f;
    [SerializeField] float riseSpeed = 1.2f;

    TextMesh tm;
    Color startColor;
    float t;

    void Awake() { tm = GetComponent<TextMesh>(); }
    void Start() { if (tm != null) startColor = tm.color; }

    void Update()
    {
        t += Time.deltaTime;
        transform.position += Vector3.up * (riseSpeed * Time.deltaTime);

        if (Camera.main != null)
            transform.rotation = Camera.main.transform.rotation;   // face the camera

        if (tm != null)
        {
            float a = Mathf.Clamp01(1f - t / lifetime);
            tm.color = new Color(startColor.r, startColor.g, startColor.b, a);
        }

        if (t >= lifetime) Destroy(gameObject);
    }
}