using System.Collections;
using UnityEngine;

// Handles player death: freezes control, shows "YOU DIED", then after a delay
// teleports to the respawn point, revives to full health, and restores control.
public class PlayerRespawn : MonoBehaviour
{
    public PlayerHealth health;
    public Transform respawnPoint;
    public float respawnDelay = 2f;

    [Header("Disabled during death")]
    public PlayerController playerController;
    public PlayerInteractor playerInteractor;
    public PlayerCombat playerCombat;

    CharacterController controller;
    bool dying;

    void Start()
    {
        if (health == null) health = GetComponent<PlayerHealth>();
        controller = GetComponent<CharacterController>();
        if (health != null) health.OnDied += HandleDeath;
    }

    void OnDestroy()
    {
        if (health != null) health.OnDied -= HandleDeath;
    }

    void HandleDeath()
    {
        if (!dying) StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        dying = true;
        SetControl(false);

        yield return new WaitForSeconds(respawnDelay);

        if (respawnPoint != null)
        {
            if (controller != null) controller.enabled = false;   // CC fights direct moves
            transform.position = respawnPoint.position;
            if (controller != null) controller.enabled = true;
        }

        if (health != null) health.Revive();
        SetControl(true);
        dying = false;
    }

    void SetControl(bool on)
    {
        if (playerController != null) playerController.enabled = on;
        if (playerInteractor != null) playerInteractor.enabled = on;
        if (playerCombat != null) playerCombat.enabled = on;
    }

    void OnGUI()
    {
        if (!dying) return;
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 48,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.red }
        };
        GUI.Label(new Rect(0, Screen.height / 2 - 40, Screen.width, 80), "YOU DIED", style);
    }
}