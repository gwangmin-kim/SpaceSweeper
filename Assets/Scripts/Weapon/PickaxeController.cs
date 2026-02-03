using System.Collections;
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
}

public class PickaxeController : MonoBehaviour, IWeapon
{
    [Header("Attack Status")]
    [SerializeField] PickaxeStat _stat;
    [SerializeField] Transform _attackOffset;
    [SerializeField] float _attackDelayRatio; // 0.0 ~ 1.0, 공격 딜레이 중 어느 시점에 실제 타격을 일으킬지 결정

    [Header("Visual")]
    [SerializeField] Transform _visualRoot;
    [SerializeField] PickaxeAnimation _animation;

    LayerMask TargetLayer => SpacePlayerController.Instance.TargetLayer;

    float _attackCooldownTimer = 0f;
    bool IsAttackable => _attackCooldownTimer <= 0f;

    ContactFilter2D _filter;
    List<Collider2D> _hitBuffer = new List<Collider2D>(10);

    Coroutine _attackRoutine;

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

        float scaleRatio = _stat.range;
        _visualRoot.localScale = new Vector3(scaleRatio, scaleRatio, 1f);
    }

    public void Attack(Vector2 _)
    {
        if (!IsAttackable) return;

        _attackCooldownTimer = _stat.cooldown;

        _attackRoutine = StartCoroutine(AttackRoutine());

        _animation.AttackAnimation(_stat.cooldown, _attackDelayRatio * _stat.cooldown);
    }

    void ProcessHit(Collider2D hit)
    {
        if (hit != null && hit.TryGetComponent<IDamagable>(out var component))
        {
            // Debug.Log($"hit detected: {hit}");

            component.TakeDamage(_stat.damage);

            Vector2 knockbackDirection = (hit.transform.position - _attackOffset.position).normalized;
            component.ApplyKnockback(knockbackDirection, _stat.knockbackIntensity);
        }
    }

    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(_attackDelayRatio * _stat.cooldown);

        if (!_stat.isMultiHitUnlocked)
        {
            Collider2D hit = Physics2D.OverlapCircle(
                _attackOffset.position, _stat.range, TargetLayer);

            ProcessHit(hit);
        }
        else
        {
            int count = Physics2D.OverlapCircle(
                _attackOffset.position, _stat.range, _filter, _hitBuffer);

            for (int i = 0; i < count; i++)
            {
                ProcessHit(_hitBuffer[i]);
            }
        }
    }

    public void CancelAttack()
    {
        if (_attackRoutine != null) StopCoroutine(_attackRoutine);
        _animation.CancelAnimation();
    }

    void OnDisable()
    {
        if (_attackRoutine != null) StopCoroutine(_attackRoutine);
    }

    public void OnDrawGizmos()
    {
        // draw attack range
        Gizmos.color = Color.softRed;
        Gizmos.DrawWireSphere(_attackOffset.position, _stat.range);
    }
}
