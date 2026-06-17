using UnityEngine;

// Left-click melee. Hits enemies in a sphere in front of the player.
// Damage = baseDamage + Strength*strengthPerDamage + perk MeleeDamage bonus.
// Grants Combat XP per hit. Ignored while a menu or dialogue is open.
public class PlayerCombat : MonoBehaviour
{
    [Header("Progression")]
    public PlayerProgression progression;
    public SubskillSO combatSubskill;     // e.g. Combat
    public int xpPerHit = 8;

    [Header("Damage")]
    public int baseDamage = 8;
    public float strengthPerDamage = 2f;  // each Strength point adds this much damage

    [Header("Reach")]
    public float attackRange = 2.2f;
    public float attackRadius = 1.2f;
    public float attackCooldown = 0.5f;
    public KeyCode attackKey = KeyCode.Mouse0;

    float nextAttackTime;

    void Start()
    {
        if (progression == null) progression = PlayerProgression.Instance;
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
        int dmg = ComputeDamage();

        Vector3 center = transform.position + Vector3.up * 1.2f + transform.forward * (attackRange * 0.5f);
        bool hitSomething = false;

        foreach (var col in Physics.OverlapSphere(center, attackRadius))
        {
            if (col.transform.IsChildOf(transform)) continue;     // ignore self
            var enemy = col.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(dmg);
                hitSomething = true;
            }
        }

        if (hitSomething && progression != null && combatSubskill != null)
            progression.AddSubskillXP(combatSubskill, xpPerHit);
    }

    int ComputeDamage()
    {
        int strength = progression != null ? progression.GetAttribute(AttributeType.Strength) : 0;
        float perkBonus = progression != null ? progression.GetStat(StatType.MeleeDamage) : 0f;
        return Mathf.RoundToInt(baseDamage + strength * strengthPerDamage + perkBonus);
    }
}