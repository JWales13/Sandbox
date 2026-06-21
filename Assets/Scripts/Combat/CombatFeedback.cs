using System.Collections;
using UnityEngine;

// Centralized "juice." Anything that takes damage reports here, so every attack
// style gets the same impact treatment: a brief hit-stop (freeze frame),
// floating damage numbers, and optional sound/VFX. Decoupled from how the hit
// was produced. Put one of these on a persistent object (e.g. GameSystems).
public class CombatFeedback : MonoBehaviour
{
    public static CombatFeedback Instance { get; private set; }

    [Header("Hit-stop (impact freeze)")]
    [Tooltip("How long the freeze lasts (real seconds).")]
    [SerializeField] float hitStopDuration = 0.06f;
    [Tooltip("Time scale during the freeze (0 = full stop).")]
    [SerializeField, Range(0f, 1f)] float hitStopScale = 0.05f;

    [Header("Damage numbers")]
    [SerializeField] bool showDamageNumbers = true;

    [Header("Optional — leave empty to skip")]
    [SerializeField] AudioClip hitSound;
    [SerializeField] GameObject hitVfx;     // a particle prefab; auto-destroyed after 2s

    bool freezing;

    void Awake() { Instance = this; }

    // Called by IDamageable implementers when they take a hit.
    public void Report(DamageInfo info)
    {
        if (hitStopDuration > 0f) StartCoroutine(HitStop());

        // Point-based effects only when we know where the hit landed.
        if (info.hitPoint != Vector3.zero)
        {
            if (showDamageNumbers) DamagePopup.Spawn(info.hitPoint, info.amount);
            if (hitSound != null) AudioSource.PlayClipAtPoint(hitSound, info.hitPoint);
            if (hitVfx != null) Destroy(Instantiate(hitVfx, info.hitPoint, Quaternion.identity), 2f);
        }
    }

    IEnumerator HitStop()
    {
        if (freezing) yield break;        // don't stack freezes
        if (Time.timeScale == 0f) yield break;  // never override a real pause
        freezing = true;

        float previous = Time.timeScale;
        Time.timeScale = hitStopScale;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        if (Time.timeScale != 0f) Time.timeScale = previous;   // unless something paused mid-freeze

        freezing = false;
    }
}