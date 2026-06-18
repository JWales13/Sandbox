using UnityEngine;

// The single home for turning attribute points into gameplay numbers.
// Systems query this instead of reading attributes directly, so changing how
// an attribute works (or adding a new one) is a one-file edit.
//
// Soft-cap formula for percentages:  value = max * points / (points + K)
//   - approaches 'max' but never reaches it
//   - each additional point matters a little less (diminishing returns)
public class AttributeEffects : MonoBehaviour
{
    public static AttributeEffects Instance { get; private set; }

    public PlayerProgression progression;

    [Header("Defense → damage reduction (fraction, soft-capped)")]
    public float defenseMaxReduction = 0.8f;   // ceiling: 80% of incoming damage
    public float defenseK = 50f;

    [Header("Agility → move-speed bonus (soft-capped)")]
    public float agilityMaxBonus = 0.4f;       // ceiling: +40% move speed
    public float agilityK = 20f;

    void Awake() { Instance = this; }

    void Start()
    {
        if (progression == null) progression = PlayerProgression.Instance;
    }

    int Points(AttributeType a) => progression != null ? progression.GetAttribute(a) : 0;

    // Fraction of incoming damage to ignore (0..defenseMaxReduction).
    public float DamageReduction()
    {
        int def = Points(AttributeType.Defense);
        return defenseMaxReduction * def / (def + defenseK);
    }

    // Multiplier applied to base move speed (1.0 = no bonus).
    public float MoveSpeedMultiplier()
    {
        int agi = Points(AttributeType.Agility);
        return 1f + agilityMaxBonus * agi / (agi + agilityK);
    }
}