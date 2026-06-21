using System.Collections;
using UnityEngine;

// The attack DRIVER. On the Attack input it asks the equipped weapon for its
// AttackBehaviour (or falls back to Unarmed), plays the animation, and triggers
// the behaviour at the strike frame. It does NOT know melee from ranged from
// magic — that lives in the AttackBehaviour. Damage = baseDamage + Stats
// MeleeDamage (Strength + perks + equipment).
public class PlayerCombat : MonoBehaviour
{
    [Header("Progression")]
    [SerializeField] PlayerProgression progression;
    [SerializeField] SubskillSO combatSubskill;     // e.g. Combat
    [SerializeField] int xpPerHit = 8;

    [Header("Damage")]
    [SerializeField] int baseDamage = 8;            // + Stats MeleeDamage
    [SerializeField] LayerMask targetMask = ~0;     // what attacks can hit

    [Header("Attack styles")]
    [Tooltip("Used when no weapon is equipped (fists). Assign a Melee Attack asset.")]
    [SerializeField] AttackBehaviourSO unarmedAttack;

    [Header("References")]
    [SerializeField] Animator animator;                 // the character model's Animator
    [SerializeField] PlayerController playerController;  // for the attack direction

    float nextAttackTime;

    void Start()
    {
        if (progression == null) progression = PlayerProgression.Instance;
        if (playerController == null) playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        // Don't attack while a menu (cursor freed) or a conversation is open.
        if (Cursor.lockState != CursorLockMode.Locked) return;
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsOpen) return;

        if (GameInput.Instance != null && GameInput.Instance.AttackPressed && Time.time >= nextAttackTime)
            BeginAttack();
    }

    // The current attack style: the equipped weapon's behaviour, else unarmed.
    AttackBehaviourSO CurrentAttack()
    {
        var weapon = Equipment.Instance != null ? Equipment.Instance.CurrentWeapon : null;
        var behaviour = weapon != null ? weapon.attackBehaviour : null;
        return behaviour != null ? behaviour : unarmedAttack;
    }

    void BeginAttack()
    {
        var atk = CurrentAttack();
        if (atk == null) return;                       // unarmed not assigned yet

        nextAttackTime = Time.time + atk.cooldown;
        if (animator != null && !string.IsNullOrEmpty(atk.animationTrigger))
            animator.SetTrigger(atk.animationTrigger);

        StartCoroutine(StrikeAfter(atk));
    }

    // Wait for the swing to reach its impact frame, then resolve the hit.
    IEnumerator StrikeAfter(AttackBehaviourSO atk)
    {
        if (atk.windup > 0f) yield return new WaitForSeconds(atk.windup);

        var ctx = new AttackContext
        {
            attacker = gameObject,
            origin = transform,
            facing = Facing(),
            damage = ComputeDamage(),
            targetMask = targetMask
        };

        bool hit = atk.Perform(ctx);
        if (hit && progression != null && combatSubskill != null)
            progression.AddSubskillXP(combatSubskill, xpPerHit);
    }

    Vector3 Facing()
    {
        Vector3 f = playerController != null ? playerController.FacingDirection : transform.forward;
        f.y = 0f;
        if (f.sqrMagnitude < 0.01f) f = transform.forward;
        return f.normalized;
    }

    int ComputeDamage()
    {
        float bonus = Stats.Instance != null ? Stats.Instance.Get(StatType.MeleeDamage) : 0f;
        return Mathf.RoundToInt(baseDamage + bonus);
    }
}