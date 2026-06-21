using UnityEngine;

// Melee attack: a sphere swept just in front of the attacker. The first attack
// style on the combat seam. Create via Create > Combat > Melee Attack, then
// assign it to a weapon's Attack Behaviour (or to PlayerCombat's Unarmed slot).
[CreateAssetMenu(menuName = "Combat/Melee Attack", fileName = "NewMeleeAttack")]
public class MeleeAttackSO : AttackBehaviourSO
{
    [Header("Reach")]
    [Tooltip("How far in front of the attacker the hit sphere sits.")]
    public float range = 2.2f;
    [Tooltip("Radius of the hit sphere.")]
    public float radius = 1.2f;

    [Header("Impact")]
    public DamageType damageType = DamageType.Physical;
    [Tooltip("How far the hit shoves the target (world units).")]
    public float knockbackDistance = 0.6f;

    public override bool Perform(AttackContext ctx)
    {
        Vector3 center = ctx.origin.position + Vector3.up * 1.2f + ctx.facing * (range * 0.5f);
        bool hitAny = false;

        foreach (var col in Physics.OverlapSphere(center, radius, ctx.targetMask, QueryTriggerInteraction.Ignore))
        {
            if (col.transform.IsChildOf(ctx.origin)) continue;          // never hit self
            var target = col.GetComponentInParent<IDamageable>();
            if (target == null || !target.IsAlive) continue;

            target.TakeDamage(new DamageInfo
            {
                amount = ctx.damage,
                type = damageType,
                hitPoint = col.ClosestPoint(center),
                knockback = ctx.facing * knockbackDistance,
                source = ctx.attacker
            });
            hitAny = true;
        }
        return hitAny;
    }
}