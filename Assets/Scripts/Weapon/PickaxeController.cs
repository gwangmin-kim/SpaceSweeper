using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PickaxeStat
{
    public float attackDamage; // 타격 피해량
    public float attackSpeed; // 1초에 몇 번 휘두르는지
    public float attackRange; // 원형 공격 범위 계수

    public float criticalChance; // 치명타 공격 확률 (기본 0)
    public float criticalDamage; // 치명타 피해 배율 (기본 2)

    public int hitCount; // 한 번에 타격하는 대상 최대 개수
    public bool isMultiHitUnlocked; // 광역 공격 해금 여부
    public float knockbackIntensity; // 타격 시 타격 대상이 밀려나는 정도
}
public class PickaxeController : MonoBehaviour, IWeapon
{
    [Header("Attack Status")]
    [SerializeField] float _defaultRange;
    [SerializeField] PickaxeStat _stat;
    [SerializeField] Transform _attackOffset;
    [SerializeField] float _attackDelayRatio; // 0.0 ~ 1.0, 공격 딜레이 중 어느 시점에 실제 타격을 일으킬지 결정

    [Header("Visual")]
    [SerializeField] Transform _visualRoot;
    [SerializeField] PickaxeAnimation _animation;

    LayerMask TargetLayer => SpacePlayerController.Instance.TargetLayer;

    float _attackCooldown;
    float _attackCooldownTimer = 0f;
    bool IsAttackable => _attackCooldownTimer <= 0f;

    ContactFilter2D _filter;
    List<Collider2D> _hitBuffer = new List<Collider2D>(10);

    Coroutine _attackRoutine;

    void FixedUpdate()
    {
        if (_attackCooldownTimer > 0f) _attackCooldownTimer -= Time.fixedDeltaTime;
    }

    public void InitWeapon()
    {
        _stat = GameManager.Instance.CurrentData.playerSpec.pickaxeStat;

        _filter = new ContactFilter2D();
        _filter.SetLayerMask(TargetLayer);
        _filter.useTriggers = false;

        float scaleRatio = _stat.attackRange;
        _visualRoot.localScale = new Vector3(scaleRatio, scaleRatio, 1f);
    }

    public void Attack(Vector2 _)
    {
        if (!IsAttackable) return;

        _attackCooldown = 1f / _stat.attackSpeed;
        _attackCooldownTimer = _attackCooldown;

        float damage = _stat.attackDamage;
        if (Random.value < _stat.criticalChance)
        {
            damage *= _stat.criticalDamage;
        }

        _attackRoutine = StartCoroutine(AttackRoutine(damage));

        _animation.AttackAnimation(_attackCooldown, _attackDelayRatio * _attackCooldown);
    }

    void ProcessHit(Collider2D hit, float damage)
    {
        if (hit != null && hit.TryGetComponent<IDamagable>(out var component))
        {
            // Debug.Log($"hit detected: {hit}");

            component.TakeDamage(_stat.attackDamage);

            Vector2 knockbackDirection = (hit.transform.position - _attackOffset.position).normalized;
            component.ApplyKnockback(knockbackDirection, _stat.knockbackIntensity);
        }
    }

    IEnumerator AttackRoutine(float damage)
    {
        yield return new WaitForSeconds(_attackDelayRatio * _attackCooldown);

        float range = _defaultRange * _stat.attackRange;

        if (!_stat.isMultiHitUnlocked)
        {
            Collider2D hit = Physics2D.OverlapCircle(
                _attackOffset.position, range, TargetLayer);

            ProcessHit(hit, damage);
        }
        else
        {
            int count = Physics2D.OverlapCircle(
                _attackOffset.position, range, _filter, _hitBuffer);

            if (count > _stat.hitCount) count = _stat.hitCount;

            for (int i = 0; i < count; i++)
            {
                ProcessHit(_hitBuffer[i], damage);
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
        Gizmos.DrawWireSphere(_attackOffset.position, _defaultRange * _stat.attackRange);
    }
}
