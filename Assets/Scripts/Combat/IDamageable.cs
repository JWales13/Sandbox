// Anything that can take damage (the player, enemies, breakable props, etc.).
// Combat targets this interface, so new damageable things "just work" — implement
// it and they can be hit.
public interface IDamageable
{
    bool IsAlive { get; }
    void TakeDamage(int amount);
}