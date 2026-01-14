using System.Collections.Generic;
using UnityEngine;

public class Pickaxe : MonoBehaviour, IWeapon
{
    [Header("Attack Status")]
    [SerializeField] int _attackDamage;
    [SerializeField] float _attackCooldown;
    [SerializeField] float _knockbackIntensity; // 타격 시 타격 대상이 밀려나는 정도

    // 부채꼴 형태 공격 범위 (기각)
    // [SerializeField] float _attackHalfAngle; // 예: 60이면 조준 방향 위아래로 60도 씩 총 120도 범위 타격
    // 원형 공격 범위
    [SerializeField] Transform _attackOffset;
    [SerializeField] float _attackRadius;

    [SerializeField] bool _isMultiHitUnlocked; // 광역 공격 해금 여부

    [Header("Collision")]
    [SerializeField] LayerMask _targetLayer;

    float _attackCooldownTimer = 0f;
    bool IsAttackable => _attackCooldownTimer <= 0f;

    ContactFilter2D _filter;
    List<Collider2D> _hitBuffer = new List<Collider2D>(10);

    void Awake()
    {
        _filter = new ContactFilter2D();
        _filter.SetLayerMask(_targetLayer);
        _filter.useTriggers = false;
    }

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

        _attackCooldownTimer = _attackCooldown;

        if (!_isMultiHitUnlocked)
        {
            // Collider2D hit = Physics2D.OverlapCircle((Vector2)transform.position, _attackRadius, _targetLayer);
            Collider2D hit = Physics2D.OverlapCircle(_attackOffset.position, _attackRadius, _targetLayer);

            if (hit != null && hit.TryGetComponent<IDamagable>(out var component))
            {
                // Debug.Log($"hit detected: {hit}");

                component.TakeDamage(_attackDamage);

                Vector2 knockbackDirection = (hit.transform.position - _attackOffset.position).normalized;
                component.ApplyKnockback(knockbackDirection, _knockbackIntensity);
            }

            // Vector2 hitDirection = (hit.transform.position - transform.position).normalized;
            // float angle = Vector2.Angle(hitDirection, aimDirection); // unsigned angle

            // if (angle <= _attackHalfAngle && hit.TryGetComponent<IDamagable>(out var component))
            // {
            //     component.TakeDamage(_attackDamage);
            //     component.ApplyKnockback(aimDirection, _knockbackIntensity);
            // }
        }
        else
        {
            int count = Physics2D.OverlapCircle(_attackOffset.position, _attackRadius, _filter, _hitBuffer);

            for (int i = 0; i < count; i++)
            {
                Collider2D hit = _hitBuffer[i];

                if (hit != null && hit.TryGetComponent<IDamagable>(out var component))
                {
                    // Debug.Log($"hit detected: {hit}");

                    component.TakeDamage(_attackDamage);

                    Vector2 knockbackDirection = (hit.transform.position - _attackOffset.position).normalized;
                    component.ApplyKnockback(knockbackDirection, _knockbackIntensity);
                }

                // Vector2 hitDirection = (hit.transform.position - transform.position).normalized;
                // float angle = Vector2.Angle(hitDirection, aimDirection); // unsigned angle

                // if (angle <= _attackHalfAngle && hit.TryGetComponent<IDamagable>(out var component))
                // {
                //     component.TakeDamage(_attackDamage);
                //     component.ApplyKnockback(aimDirection, _knockbackIntensity);
                // }
            }
        }

        Debug.DrawLine(transform.position, _attackRadius * aimDirection, Color.yellowGreen, 0.5f);
    }

    public void OnDrawGizmos()
    {
        // draw attack range
        Gizmos.color = Color.softRed;
        Gizmos.DrawWireSphere(_attackOffset.position, _attackRadius);
    }
}
