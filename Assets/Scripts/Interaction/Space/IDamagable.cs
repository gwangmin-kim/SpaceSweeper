using UnityEngine;

public interface IDamagable
{
    void TakeDamage(int damage);
    void ApplyKnockback(Vector2 direction, float intensity);
}
