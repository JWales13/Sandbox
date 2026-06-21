using UnityEngine;

// Everything the driver hands an attack so it can resolve itself, without the
// driver knowing whether it's a sword swing, an arrow, or a spell.
public struct AttackContext
{
    public GameObject attacker;    // the player (damage source)
    public Transform origin;       // usually the attacker's transform
    public Vector3 facing;         // normalized, horizontal aim/facing direction
    public int damage;             // already computed (base + stats) by the driver
    public LayerMask targetMask;   // what can be hit
}

// THE COMBAT SEAM. An attack style is a data asset that knows how to execute
// itself. The player's PlayerCombat driver just asks the equipped weapon/spell
// for its AttackBehaviour and calls Perform — it never branches on melee vs
// ranged vs magic. Add a new style = a new subclass + new assets, no driver
// changes (same pattern as quest objectives / stat sources).
public abstract class AttackBehaviourSO : ScriptableObject
{
    [Header("Timing")]
    [Tooltip("Animator trigger to fire on attack (e.g. 'Attack'). Blank = none.")]
    public string animationTrigger = "Attack";
    [Tooltip("Seconds from the button press to the hit landing — sync to the swing's impact frame.")]
    public float windup = 0.25f;
    [Tooltip("Seconds before another attack is allowed.")]
    public float cooldown = 0.6f;

    // Execute the attack now (the strike frame). Return true if it hit anything
    // (so the driver can grant XP). Implementations apply DamageInfo to targets.
    public abstract bool Perform(AttackContext ctx);
}