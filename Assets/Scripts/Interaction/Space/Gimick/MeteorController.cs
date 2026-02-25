using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MeteorData
{
    public float delay;
    public float radius;

    public int damageToDamagable;
    public float knockbackFactor;
}

[RequireComponent(typeof(Rigidbody2D))]
public class MeteorController : MonoBehaviour
{
    Rigidbody2D _rigidbody;

    [Header("Spawn Settings")]
    [SerializeField] float _spawnOffsetDistance = 5.0f;

    MeteorData _data;

    Vector2 _startPosition;
    Vector2 _hitPosition;

    float _timer;
    float _delayInverse;

    bool _isHit = true;

    // collision
    ContactFilter2D _filter;
    List<Collider2D> _hitBuffer;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        var data = GameManager.Instance.CurrentData.gimickSpec.meteorData;
        InitMeteor(data);
    }

    void Update()
    {
        if (_isHit) return;

        if (_timer > 0)
        {
            _timer -= Time.deltaTime;

            float t = Mathf.Clamp01(1f - _timer * _delayInverse);
            _rigidbody.position = Vector2.Lerp(_startPosition, _hitPosition, t);
        }
        else
        {
            Hit();
        }
    }

    void InitMeteor(MeteorData data)
    {
        _data = data;

        _timer = _data.delay;
        _delayInverse = 1f / _data.delay;

        _isHit = false;

        _filter = new ContactFilter2D();
        _filter.SetLayerMask(InGameRoutineManager.Instance.GimickTargetLayer);
        _filter.useTriggers = true;

        _hitBuffer = new List<Collider2D>(10);

        InitPosition();
    }

    void InitPosition()
    {
        Camera camera = Camera.main;

        // 도착 지점: 화면 내 랜덤한 위치
        Vector2 randomViewportPosition = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        _hitPosition = camera.ViewportToWorldPoint(randomViewportPosition);

        // 시작 지점: 화면 중심에서 랜덤 방향으로 화면 밖까지 이동
        Vector2 cameraCenter = camera.transform.position;
        // 화면 대각선 길이 계산
        Vector2 screenBottomLeft = camera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector2 screenTopRight = camera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        float screenDiagonalRadius = Vector2.Distance(screenBottomLeft, screenTopRight) * 0.5f;
        // 화면 반지름 + 추가 여유 거리 만큼 떨어진 곳을 시작점으로 설정
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float totalDistance = screenDiagonalRadius + _spawnOffsetDistance;
        _startPosition = cameraCenter + (randomDirection * totalDistance);

        // 시작 위치로 즉시 이동
        _rigidbody.position = _startPosition;

        // 속력에 비례하여 초기 회전 속도 설정
        float speed = (_hitPosition - _startPosition).magnitude;
        _rigidbody.angularVelocity = speed;
    }

    void Hit()
    {
        if (_isHit) return;
        _isHit = true;

        int hitCount = Physics2D.OverlapCircle(_hitPosition, _data.radius, _filter, _hitBuffer);

        for (int i = 0; i < hitCount; i++)
        {
            var hit = _hitBuffer[i];

            if (hit.gameObject.TryGetComponent<IDamagable>(out var damagable))
            {
                Vector2 knockbackDirection = (Vector2)hit.transform.position - _hitPosition;

                AttackInfo attackInfo = new AttackInfo
                {
                    source = AttackerType.Player,
                    isCritical = false,
                    damage = _data.damageToDamagable,
                    direction = knockbackDirection,
                    knockbackIntensity = _data.knockbackFactor
                };

                damagable.ApplyAttack(attackInfo);
            }
            else if (hit.CompareTag("Player"))
            {
                SessionManager.Instance.ReceiveDamage();
                Vector2 knockbackDirection = (Vector2)hit.transform.position - _hitPosition;
                hit.GetComponent<SpacePlayerController>().ApplyKnockback(knockbackDirection, _data.knockbackFactor);
            }
        }

        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_hitPosition, _data.radius);
    }
}
