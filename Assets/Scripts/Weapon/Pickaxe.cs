using UnityEngine;

public class Pickaxe : MonoBehaviour, IWeapon
{
    [Header("Attack Status")]
    [SerializeField] int _attackDamage;
    [SerializeField] float _attackCooldown;
    [SerializeField] float _knockbackIntensity;

    // 부채꼴 형태 공격 범위
    [SerializeField] float _attackHalfAngle; // 예: 60이면 조준 방향 위아래로 60도 씩 총 120도 범위 타격
    [SerializeField] float _attackRadius;

    [SerializeField] bool _isMultiHitUnlocked;

    [Header("Collision")]
    [SerializeField] LayerMask _damagableLayer;
    [SerializeField] LayerMask _firstCheckLayer; // ? idea: 플레이어에게 위협이 되는 물체가 있다면 우선적으로 타격하도록 구현하고 싶을 때 사용 (_damagableLayer의 부분집합)

    float _attackCooldownTimer = 0f;
    bool IsAttackable => _attackCooldownTimer <= 0f;

    void FixedUpdate()
    {
        if (_attackCooldownTimer > 0f) _attackCooldownTimer -= Time.fixedDeltaTime;
    }

    public void Initialize()
    {

    }

    public void Attack(Vector2 aimDirection)
    {
        if (!IsAttackable) return;

        if (!_isMultiHitUnlocked)
        {
            Collider2D hit = Physics2D.OverlapCircle((Vector2)transform.position, _attackRadius, _damagableLayer);

            if (hit == null) return;

            // Debug.Log($"hit detected: {hit}");

            Vector2 hitDirection = (hit.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(hitDirection, aimDirection); // unsigned angle

            if (angle <= _attackHalfAngle && hit.TryGetComponent<IDamagable>(out var component))
            {
                component.TakeDamage(_attackDamage);
                component.ApplyKnockback(aimDirection, _knockbackIntensity);
            }
        }
        else
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll((Vector2)transform.position, _attackRadius, _damagableLayer);

            foreach (Collider2D hit in hits)
            {
                if (hit == null) continue;

                Vector2 hitDirection = (hit.transform.position - transform.position).normalized;
                float angle = Vector2.Angle(hitDirection, aimDirection); // unsigned angle

                if (angle <= _attackHalfAngle && hit.TryGetComponent<IDamagable>(out var component))
                {
                    component.TakeDamage(_attackDamage);
                    component.ApplyKnockback(aimDirection, _knockbackIntensity);
                }
            }
        }

        _attackCooldownTimer = _attackCooldown;
    }

    public void OnDrawGizmos()
    {
        // draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRadius);
    }
}
