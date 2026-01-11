using System.Collections.Generic;
using UnityEngine;

public class Lasergun : MonoBehaviour, IWeapon
{
    [Header("Attack Status")]
    [SerializeField] int _attackDamage;
    [SerializeField] float _attackInterval;
    [SerializeField] float _attackDistance; // 첫 공격 대상 탐지 최대 사거리
    [SerializeField] float _detectHalfWidth; // 감지할 너비 (CircleCast의 반지름)
    [SerializeField] Transform _detectOrigin; // 감지를 시작할 위치 (총구 위치)

    [Header("Transition Settings")]
    [SerializeField] bool _isTransitionUnlocked;
    [SerializeField] int _maxTransitionCount;
    [SerializeField] float _transitionRadius;
    // [SerializeField] float _transitionBreakDistance; // 전이 연결 후 이 거리를 넘어서면 새 전이 계산

    [Header("Collision")]
    [SerializeField] LayerMask _targetLayer;
    // // ? idea: 플레이어에게 위협이 되는 물체가 있다면 우선적으로 타격하도록 구현하고 싶을 때 사용 (_targetLayer의 부분집합)
    // [SerializeField] LayerMask _firstCheckLayer;

    ContactFilter2D _filter;
    List<Collider2D> _overlapBuffer = new List<Collider2D>(10);
    HashSet<Transform> _currentTargets = new HashSet<Transform>(10);

    float _attackCooldownTimer = 0f;
    bool IsAttackable => _attackCooldownTimer <= 0f;

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

    Transform GetNearestTarget(Vector2 from)
    {
        float minSqrDistance = Mathf.Infinity;
        Transform nearestTarget = null;

        int count = Physics2D.OverlapCircle(from, _transitionRadius, _filter, _overlapBuffer);

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

    public void Initialize()
    {

    }

    public void Attack(Vector2 aimDirection)
    {
        if (!IsAttackable) return;

        _attackCooldownTimer = _attackInterval;

        _currentTargets.Clear();

        // first target
        RaycastHit2D hit = Physics2D.CircleCast(_detectOrigin.position, _detectHalfWidth, aimDirection, _attackDistance, _targetLayer);

        Debug.DrawRay(_detectOrigin.position, aimDirection * _attackDistance, Color.yellowGreen, 0.5f);

        if (!hit || !hit.transform.TryGetComponent<IDamagable>(out var component)) return;

        // Debug.Log($"hit detected: {hit}");

        component.TakeDamage(_attackDamage);
        _currentTargets.Add(hit.transform);

        Debug.DrawLine(_detectOrigin.position, hit.transform.position, Color.red, 0.5f);
        // Debug.Log($"[Hit 0] First Target: {hit.transform.name}");

        if (!_isTransitionUnlocked) return;

        Transform currentOrigin = hit.transform;
        // transition
        for (int i = 0; i < _maxTransitionCount; i++)
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

            component.TakeDamage(_attackDamage);
            _currentTargets.Add(target);

            currentOrigin = target;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.softRed;
        Gizmos.DrawWireSphere(_detectOrigin.position, _attackDistance);
        foreach (Transform target in _currentTargets)
        {
            // Debug.Log($"current target: {target}");
            if (target != null)
            {
                Gizmos.DrawWireSphere(target.position, _transitionRadius);
            }
        }
    }
}
