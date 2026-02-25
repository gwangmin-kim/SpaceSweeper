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
    [SerializeField] PickaxeAnimation _animation;

    LayerMask TargetLayer => SpacePlayerController.Instance.TargetLayer;

    float _attackCooldown;
    float _attackCooldownTimer = 0f;
    bool IsAttackable => _attackCooldownTimer <= 0f;

    // combo bonus
    float _bonusSpeedRate = 1f;
    float _bonusRangeRate = 1f;
    Vector3 _defaultScale;

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
        transform.localScale = new Vector3(scaleRatio, scaleRatio, 1f);
        _defaultScale = transform.localScale;
    }

    void SetComboBonus(int levelIndex, float _)
    {
        var spec = GameManager.Instance.CurrentData.playerSpec.comboSpec;
        _bonusSpeedRate = 1f + spec.attackSpeedBonus * levelIndex;
        _bonusRangeRate = 1f + spec.pickaxeRangeBonus * levelIndex;

        transform.localScale = _bonusRangeRate * _defaultScale;
    }

    public void Attack(Vector2 _)
    {
        if (!IsAttackable) return;

        _attackCooldown = 1f / (_stat.attackSpeed * _bonusSpeedRate);
        _attackCooldownTimer = _attackCooldown;

        float damage = _stat.attackDamage;
        bool isCritical = Random.value < _stat.criticalChance;
        if (isCritical) damage *= _stat.criticalDamage;

        _attackRoutine = StartCoroutine(AttackRoutine(damage, isCritical));

        _animation.AttackAnimation(_attackCooldown, _attackDelayRatio * _attackCooldown);
    }

    IEnumerator AttackRoutine(float damage, bool isCritical)
    {
        yield return new WaitForSeconds(_attackDelayRatio * _attackCooldown);

        float range = _defaultRange * _stat.attackRange * _bonusRangeRate;

        if (!_stat.isMultiHitUnlocked)
        {
            Collider2D hit = Physics2D.OverlapCircle(
                _attackOffset.position, range, TargetLayer);

            ProcessHit(hit, damage, isCritical);
        }
        else
        {
            int count = Physics2D.OverlapCircle(
                _attackOffset.position, range, _filter, _hitBuffer);

            if (count > _stat.hitCount) count = _stat.hitCount;

            for (int i = 0; i < count; i++)
            {
                ProcessHit(_hitBuffer[i], damage, isCritical);
            }
        }
    }

    void ProcessHit(Collider2D hit, float damage, bool isCritical)
    {
        if (hit != null && hit.TryGetComponent<IDamagable>(out var component))
        {
            // Debug.Log($"hit detected: {hit}");

            if (isCritical) damage *= _stat.criticalDamage;
            Vector2 knockbackDirection = (hit.transform.position - _attackOffset.position).normalized;

            AttackInfo attackInfo = new AttackInfo
            {
                source = AttackerType.Player,
                isCritical = isCritical,
                damage = damage,
                direction = knockbackDirection,
                knockbackIntensity = _stat.knockbackIntensity
            };

            component.ApplyAttack(attackInfo);
        }
    }

    public void CancelAttack()
    {
        if (_attackRoutine != null) StopCoroutine(_attackRoutine);
        _animation.CancelAnimation();
    }

    void OnEnable()
    {
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.OnComboChanged += SetComboBonus;
        }
    }


    void OnDisable()
    {
        if (_attackRoutine != null) StopCoroutine(_attackRoutine);

        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.OnComboChanged -= SetComboBonus;
        }
    }

    public void OnDrawGizmos()
    {
        // draw attack range
        Gizmos.color = Color.softRed;
        Gizmos.DrawWireSphere(_attackOffset.position, _defaultRange * _stat.attackRange * _bonusRangeRate);
    }
}
