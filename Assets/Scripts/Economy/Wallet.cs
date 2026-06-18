using System;
using UnityEngine;

// The player's money. Other systems use Wallet.Instance.
public class Wallet : MonoBehaviour
{
    public static Wallet Instance { get; private set; }

    public int coins = 50;            // starting coins (tune in inspector)
    public event Action OnChanged;

    void Awake() { Instance = this; }

    public bool CanAfford(int amount) => coins >= amount;

    public void Add(int amount)
    {
        coins += Mathf.Max(0, amount);
        OnChanged?.Invoke();
    }

    public bool Spend(int amount)
    {
        if (coins < amount) return false;
        coins -= amount;
        OnChanged?.Invoke();
        return true;
    }

    // For save/load.
    public void SetCoins(int value)
    {
        coins = Mathf.Max(0, value);
        OnChanged?.Invoke();
    }
}