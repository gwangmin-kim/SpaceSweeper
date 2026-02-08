using UnityEngine;

public interface IDamagable
{
    void TakeDamage(float damage);
    void ApplyKnockback(Vector2 direction, float intensity);
}
