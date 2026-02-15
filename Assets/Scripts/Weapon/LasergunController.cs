using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LasergunStat
{
    public float damage;
    public float hitInterval;
    public float range;
    public bool isTransitionUnlocked;
    public int transitionCount;
    public float transitionRange;
}

public class LasergunController : MonoBehaviour, IWeapon
{
    [Header("Attack Status")]
    [SerializeField] LasergunStat _stat;
    [SerializeField] float _detectHalfWidth; // 감지할 너비 (CircleCast의 반지름)
    [SerializeField] Transform _detectOrigin; // 감지를 시작할 위치 (총구 위치)

    LayerMask TargetLayer => SpacePlayerController.Instance.TargetLayer;
    // // ? idea: 플레이어에게 위협이 되는 물체가 있다면 우선적으로 타격하도록 구현하고 싶을 때 사용 (_targetLayer의 부분집합)
    // [SerializeField] LayerMask _firstCheckLayer;

    ContactFilter2D _filter;
    List<Collider2D> _overlapBuffer = new List<Collider2D>(10);
    HashSet<Transform> _currentTargets = new HashSet<Transform>(10);

    float _attackCooldownTimer = 0f;
    bool IsAttackable => _attackCooldownTimer <= 0f;

    void FixedUpdate()
    {
        if (_attackCooldownTimer > 0f) _attackCooldownTimer -= Time.fixedDeltaTime;
    }

    Transform GetNearestTarget(Vector2 from)
    {
        float minSqrDistance = Mathf.Infinity;
        Transform nearestTarget = null;

        int count = Physics2D.OverlapCircle(from, _stat.transitionRange, _filter, _overlapBuffer);

        for (int i = 0; i < count; i++)
        {
            Transform candidate = _overlapBuffer[i].transform;

            if (_currentTargets.Contains(candidate)) continue;

            float sqrDistance = ((Vector2)candidate.position - from).sqrMagnitude;

            if (minSqrDistance > sqrDistance)
            {
                minSqrDistance = sqrDistance;
                nearestTarget = candidate;
            }
        }

        return nearestTarget;
    }

    public void InitWeapon()
    {
        _stat = GameManager.Instance.CurrentData.playerSpec.lasergunStat;

        _filter = new ContactFilter2D();
        _filter.SetLayerMask(TargetLayer);
        _filter.useTriggers = false;
    }

    public void Attack(Vector2 aimDirection)
    {
        if (!IsAttackable) return;

        _attackCooldownTimer = _stat.hitInterval;

        _currentTargets.Clear();

        // first target
        RaycastHit2D hit = Physics2D.CircleCast(_detectOrigin.position, _detectHalfWidth, aimDirection, _stat.range, TargetLayer);

        Debug.DrawRay(_detectOrigin.position, aimDirection * _stat.range, Color.yellowGreen, 0.5f);

        if (!hit || !hit.transform.TryGetComponent<IDamagable>(out var component)) return;

        // Debug.Log($"hit detected: {hit}");

        component.TakeDamage(_stat.damage);
        _currentTargets.Add(hit.transform);

        Debug.DrawLine(_detectOrigin.position, hit.transform.position, Color.red, 0.5f);
        // Debug.Log($"[Hit 0] First Target: {hit.transform.name}");

        if (!_stat.isTransitionUnlocked) return;

        Transform currentOrigin = hit.transform;
        // transition
        for (int i = 0; i < _stat.transitionCount; i++)
        {
            Transform target = GetNearestTarget(currentOrigin.position);

            if (target == null) break;

            if (!target.TryGetComponent<IDamagable>(out component))
            {
                // 있어서는 안되는 경우 (해당 필터로 감지된 대상은 반드시 IDamagable이어야 함)
                Debug.LogError($"[Attack] {target} is not IDamagable");
                continue;
            }

            Debug.DrawLine(currentOrigin.position, target.position, Color.cyan, 0.5f);
            // Debug.Log($"[Hit {i + 1}] Transition Target: {target.name}");

            component.TakeDamage(_stat.damage);
            _currentTargets.Add(target);

            currentOrigin = target;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.softRed;
        Gizmos.DrawWireSphere(_detectOrigin.position, _stat.range);
        foreach (Transform target in _currentTargets)
        {
            // Debug.Log($"current target: {target}");
            if (target != null)
            {
                Gizmos.DrawWireSphere(target.position, _stat.range);
            }
        }
    }
}
