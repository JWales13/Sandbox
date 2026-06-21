using UnityEngine;

// Simple enemy brain: wanders near its spawn, chases the player when in sight,
// and attacks on contact. Moves by translating + snapping to the ground (no NavMesh).
// Pairs with EnemyHealth on the same object.
public class EnemyAI : MonoBehaviour
{
    [Header("Senses")]
    [SerializeField] float sightRange = 12f;
    [SerializeField] float attackRange = 2f;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 2.5f;
    [SerializeField] float turnSpeed = 8f;
    [Tooltip("Half the capsule's height, so it sits on the ground.")]
    [SerializeField] float groundOffset = 1f;

    [Header("Attack")]
    [SerializeField] int attackDamage = 5;
    [SerializeField] float attackCooldown = 1.5f;

    [Header("Wander")]
    [SerializeField] float wanderRadius = 6f;
    [SerializeField] float wanderInterval = 4f;

    Transform player;
    PlayerHealth playerHealth;
    EnemyHealth selfHealth;
    Vector3 home, wanderTarget;
    float wanderTimer, nextAttackTime;

    void Start()
    {
        selfHealth = GetComponent<EnemyHealth>();
        playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null) player = playerHealth.transform;
        home = transform.position;
        PickWander();
    }

    void Update()
    {
        if (selfHealth != null && !selfHealth.IsAlive) return;   // dead: do nothing

        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= sightRange)
            {
                FaceToward(player.position);
                if (dist > attackRange) MoveToward(player.position, moveSpeed);
                else TryAttack();
            }
            else Wander();
        }
        else Wander();

        GroundSnap();
    }

    void TryAttack()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;
        if (playerHealth != null) playerHealth.TakeDamage(DamageInfo.Simple(attackDamage, gameObject));
    }

    void Wander()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f) PickWander();
        if (Vector3.Distance(transform.position, wanderTarget) > 0.5f)
        {
            FaceToward(wanderTarget);
            MoveToward(wanderTarget, moveSpeed * 0.5f);
        }
    }

    void PickWander()
    {
        wanderTimer = wanderInterval;
        Vector2 r = Random.insideUnitCircle * wanderRadius;
        wanderTarget = home + new Vector3(r.x, 0f, r.y);
    }

    void FaceToward(Vector3 target)
    {
        Vector3 to = target - transform.position; to.y = 0f;
        if (to.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(to), turnSpeed * Time.deltaTime);
    }

    void MoveToward(Vector3 target, float speed)
    {
        Vector3 to = target - transform.position; to.y = 0f;
        if (to.sqrMagnitude > 0.01f)
            transform.position += to.normalized * speed * Time.deltaTime;
    }

    // Keep the enemy on the ground without physics: raycast down, ignore self and player.
    void GroundSnap()
    {
        Vector3 origin = transform.position + Vector3.up * 3f;
        float bestDist = Mathf.Infinity;
        float groundY = transform.position.y;

        foreach (var h in Physics.RaycastAll(origin, Vector3.down, 12f))
        {
            if (h.collider.transform.IsChildOf(transform)) continue;            // ignore self
            if (h.collider.GetComponentInParent<PlayerHealth>() != null) continue; // ignore player
            if (h.distance < bestDist) { bestDist = h.distance; groundY = h.point.y; }
        }

        if (bestDist < Mathf.Infinity)
        {
            var p = transform.position;
            p.y = groundY + groundOffset;
            transform.position = p;
        }
    }
}