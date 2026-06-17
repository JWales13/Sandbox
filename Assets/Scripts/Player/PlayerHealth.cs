using System;
using UnityEngine;

// Player health. Max HP scales with the Vitality attribute. Recalculates
// whenever attributes change (e.g. you spend a point). Combat will call
// TakeDamage/Heal later.
public class PlayerHealth : MonoBehaviour
{
    public PlayerProgression progression;
    public int baseHealth = 50;
    public int healthPerVitality = 10;

    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public event Action OnHealthChanged;

    void Start()
    {
        if (progression == null) progression = PlayerProgression.Instance;
        if (progression != null) progression.OnChanged += Recalculate;
        Recalculate();
    }

    void OnDestroy()
    {
        if (progression != null) progression.OnChanged -= Recalculate;
    }

    void Recalculate()
    {
        int vit = progression != null ? progression.GetAttribute(AttributeType.Vitality) : 0;
        int newMax = baseHealth + healthPerVitality * vit;
        int delta = newMax - MaxHealth;
        MaxHealth = newMax;
        // Gaining max HP (from Vitality) heals you by the amount gained.
        CurrentHealth = Mathf.Clamp(CurrentHealth + Mathf.Max(0, delta), 1, MaxHealth);
        OnHealthChanged?.Invoke();
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth - Mathf.Max(0, amount), 0, MaxHealth);
        OnHealthChanged?.Invoke();
        // (Death handling comes with combat.)
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + Mathf.Max(0, amount), 0, MaxHealth);
        OnHealthChanged?.Invoke();
    }

    // Used by save/load to restore a specific HP value (after max is recalculated).
    public void SetCurrent(int hp)
    {
        if (hp < 0) return; // -1 = leave at full
        CurrentHealth = Mathf.Clamp(hp, 0, MaxHealth);
        OnHealthChanged?.Invoke();
    }
}