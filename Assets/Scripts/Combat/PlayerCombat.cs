using UnityEngine;

// Left-click melee. Hits enemies in a sphere in front of the player.
// Damage = baseDamage + Strength*strengthPerDamage + perk MeleeDamage bonus.
// Grants Combat XP per hit. Ignored while a menu or dialogue is open.
public class PlayerCombat : MonoBehaviour
{
    [Header("Progression")]
    [SerializeField] PlayerProgression progression;
    [SerializeField] SubskillSO combatSubskill;     // e.g. Combat
    [SerializeField] int xpPerHit = 8;

    [Header("Damage")]
    [SerializeField] int baseDamage = 8;   // total = baseDamage + Stats MeleeDamage (Strength + perks)

    [Header("Reach")]
    [SerializeField] float attackRange = 2.2f;
    [SerializeField] float attackRadius = 1.2f;
    [SerializeField] float attackCooldown = 0.6f;
    [SerializeField] KeyCode attackKey = KeyCode.Mouse0;

    [Header("Animation")]
    [SerializeField] Animator animator;                 // the character model's Animator
    [SerializeField] string attackTrigger = "Attack";
    [Tooltip("Delay before the hit registers, so damage lands mid-swing.")]
    [SerializeField] float hitDelay = 0.25f;

    [Header("Facing")]
    [SerializeField] PlayerController playerController;  // used for the attack direction

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

        if (Input.GetKeyDown(attackKey) && Time.time >= nextAttackTime)
            Attack();
    }

    void Attack()
    {
        nextAttackTime = Time.time + attackCooldown;
        if (animator != null) animator.SetTrigger(attackTrigger);
        Invoke(nameof(DealDamage), hitDelay);   // hit connects partway through the swing
    }

    void DealDamage()
    {
        int dmg = ComputeDamage();

        Vector3 facing = playerController != null ? playerController.FacingDirection : transform.forward;
        facing.y = 0f;
        if (facing.sqrMagnitude < 0.01f) facing = transform.forward;
        facing.Normalize();

        Vector3 center = transform.position + Vector3.up * 1.2f + facing * (attackRange * 0.5f);
        bool hitSomething = false;

        foreach (var col in Physics.OverlapSphere(center, attackRadius))
        {
            if (col.transform.IsChildOf(transform)) continue;     // ignore self
            var target = col.GetComponentInParent<IDamageable>();
            if (target != null && target.IsAlive)
            {
                target.TakeDamage(dmg);
                hitSomething = true;
            }
        }

        if (hitSomething && progression != null && combatSubskill != null)
            progression.AddSubskillXP(combatSubskill, xpPerHit);
    }

    int ComputeDamage()
    {
        float bonus = Stats.Instance != null ? Stats.Instance.Get(StatType.MeleeDamage) : 0f;
        return Mathf.RoundToInt(baseDamage + bonus);
    }
}