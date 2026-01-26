using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PickaxeStat
{
    public int damage;
    public float cooldown;
    public float range; // 원형 공격 범위
    public bool isMultiHitUnlocked; // 광역 공격 해금 여부
    public float knockbackIntensity; // 타격 시 타격 대상이 밀려나는 정도
    public float dropIncreseRate; // 자원 파편 드롭량 증가율
}

public class Pickaxe : MonoBehaviour, IWeapon
{
    [Header("Attack Status")]
    [SerializeField] PickaxeStat _stat;
    [SerializeField] Transform _attackOffset;

    LayerMask TargetLayer => SpacePlayerController.Instance.TargetLayer;

    float _attackCooldownTimer = 0f;
    bool IsAttackable => _attackCooldownTimer <= 0f;

    ContactFilter2D _filter;
    List<Collider2D> _hitBuffer = new List<Collider2D>(10);

    void FixedUpdate()
    {
        if (_attackCooldownTimer > 0f) _attackCooldownTimer -= Time.fixedDeltaTime;
    }

    public void Initialize()
    {
        _stat = GameManager.Instance.CurrentData.playerSpec.pickaxeStat;

        _filter = new ContactFilter2D();
        _filter.SetLayerMask(TargetLayer);
        _filter.useTriggers = false;
    }

    public void Attack(Vector2 aimDirection)
    {
        if (!IsAttackable) return;

        _attackCooldownTimer = _stat.cooldown;

        if (!_stat.isMultiHitUnlocked)
        {
            Collider2D hit = Physics2D.OverlapCircle(
                _attackOffset.position, _stat.range, TargetLayer);

            if (hit != null && hit.TryGetComponent<IDamagable>(out var component))
            {
                // Debug.Log($"hit detected: {hit}");

                component.TakeDamage(_stat.damage);

                Vector2 knockbackDirection = (hit.transform.position - _attackOffset.position).normalized;
                component.ApplyKnockback(knockbackDirection, _stat.knockbackIntensity);
            }
        }
        else
        {
            int count = Physics2D.OverlapCircle(
                _attackOffset.position, _stat.range, _filter, _hitBuffer);

            for (int i = 0; i < count; i++)
            {
                Collider2D hit = _hitBuffer[i];

                if (hit != null && hit.TryGetComponent<IDamagable>(out var component))
                {
                    // Debug.Log($"hit detected: {hit}");

                    component.TakeDamage(_stat.damage);

                    Vector2 knockbackDirection = (hit.transform.position - _attackOffset.position).normalized;
                    component.ApplyKnockback(knockbackDirection, _stat.knockbackIntensity);
                }
            }
        }

        Debug.DrawLine(transform.position, _stat.range * aimDirection, Color.yellowGreen, 0.5f);
    }

    public void OnDrawGizmos()
    {
        // draw attack range
        Gizmos.color = Color.softRed;
        Gizmos.DrawWireSphere(_attackOffset.position, _stat.range);
    }
}
