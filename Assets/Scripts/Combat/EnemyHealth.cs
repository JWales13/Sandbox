using UnityEngine;

// Health for an enemy / training dummy. Flashes white on hit, shows an overhead
// HP label, and respawns after a delay (or is destroyed if respawnSeconds <= 0).
public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 30;
    public float respawnSeconds = 4f;

    int current;
    float flashUntil;
    Renderer[] renderers;
    Collider[] colliders;
    Color baseColor;
    bool dead;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);
        if (renderers.Length > 0) baseColor = renderers[0].material.color;
        current = maxHealth;
    }

    void Update()
    {
        if (renderers.Length > 0 && !dead)
            renderers[0].material.color = Time.time < flashUntil ? Color.white : baseColor;
    }

    public void TakeDamage(int amount)
    {
        if (dead || amount <= 0) return;
        current = Mathf.Max(0, current - amount);
        flashUntil = Time.time + 0.1f;
        if (current <= 0) Die();
    }

    void Die()
    {
        dead = true;
        if (respawnSeconds <= 0f) { Destroy(gameObject); return; }
        SetVisible(false);
        Invoke(nameof(Respawn), respawnSeconds);
    }

    void Respawn()
    {
        current = maxHealth;
        dead = false;
        SetVisible(true);
    }

    void SetVisible(bool on)
    {
        foreach (var r in renderers) r.enabled = on;
        foreach (var c in colliders) c.enabled = on;
    }

    void OnGUI()
    {
        if (dead || Camera.main == null) return;
        Vector3 sp = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.2f);
        if (sp.z < 0) return; // behind camera
        var rect = new Rect(sp.x - 40, Screen.height - sp.y - 10, 80, 22);
        GUI.Label(rect, $"{current}/{maxHealth}",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } });
    }
}