using System;
using UnityEngine;

// Player health. Max HP scales with the Vitality attribute. Recalculates
// whenever attributes change (e.g. you spend a point). Combat will call
// TakeDamage/Heal later.
public class PlayerHealth : MonoBehaviour, ISaveable, IDamageable
{
    public bool IsAlive => !IsDead;

    public string SaveId => "playerHealth";
    public string WriteState() => CurrentHealth.ToString();
    public void ReadState(string data) { if (int.TryParse(data, out var hp)) SetCurrent(hp); }

    public int baseHealth = 50;   // max HP = baseHealth + Stats MaxHealthBonus

    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }
    public event Action OnHealthChanged;
    public event Action OnDied;

    public static PlayerHealth Instance { get; private set; }

    void Awake() { Instance = this; }

    void Start()
    {
        if (Stats.Instance != null) Stats.Instance.OnChanged += Recalculate;
        Recalculate();
    }

    void OnDestroy()
    {
        if (Stats.Instance != null) Stats.Instance.OnChanged -= Recalculate;
    }

    void Recalculate()
    {
        int bonus = Stats.Instance != null ? Mathf.RoundToInt(Stats.Instance.Get(StatType.MaxHealthBonus)) : 0;
        int newMax = baseHealth + bonus;
        int delta = newMax - MaxHealth;
        MaxHealth = newMax;
        // Gaining max HP (from Vitality) heals you by the amount gained.
        CurrentHealth = Mathf.Clamp(CurrentHealth + Mathf.Max(0, delta), 1, MaxHealth);
        OnHealthChanged?.Invoke();
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        // Defense (via the Stats pipeline) reduces incoming damage.
        if (Stats.Instance != null)
        {
            float reduction = Mathf.Clamp(Stats.Instance.Get(StatType.DamageReduction), 0f, 0.95f);
            amount = Mathf.RoundToInt(amount * (1f - reduction));
        }

        CurrentHealth = Mathf.Clamp(CurrentHealth - Mathf.Max(0, amount), 0, MaxHealth);
        OnHealthChanged?.Invoke();

        if (CurrentHealth <= 0)
        {
            IsDead = true;
            OnDied?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Clamp(CurrentHealth + Mathf.Max(0, amount), 0, MaxHealth);
        OnHealthChanged?.Invoke();
    }

    // Bring the player back to life at full health (used on respawn).
    public void Revive()
    {
        IsDead = false;
        CurrentHealth = MaxHealth;
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