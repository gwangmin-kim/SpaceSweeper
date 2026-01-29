using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MeteorData
{
    public float hitTime;
    public float hitRadius;

    public int damageToDamagable;
    public float knockbackFactor;
    public float damageToPlayer;
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

    float _hitTimer;
    float _hitTimeInverse;

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

        if (_hitTimer > 0)
        {
            _hitTimer -= Time.deltaTime;

            float t = Mathf.Clamp01(1f - _hitTimer * _hitTimeInverse);
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

        _hitTimer = _data.hitTime;
        _hitTimeInverse = 1f / _data.hitTime;

        _isHit = false;

        _filter = new ContactFilter2D();
        _filter.SetLayerMask(StageManager.Instance.GimickTargetLayer);
        _filter.useTriggers = true;

        _hitBuffer = new List<Collider2D>(10);

        InitPosition();
    }

    void InitPosition()
    {
        Camera cam = Camera.main;

        // 도착 지점: 화면 내 랜덤한 위치
        Vector2 randomViewportPos = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        _hitPosition = cam.ViewportToWorldPoint(randomViewportPos);

        // 시작 지점: 화면 중심에서 랜덤 방향으로 화면 밖까지 이동
        Vector2 camCenter = cam.transform.position;
        // 화면 대각선 길이 계산
        Vector2 screenBottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector2 screenTopRight = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
        float screenDiagonalRadius = Vector2.Distance(screenBottomLeft, screenTopRight) * 0.5f;
        // 화면 반지름 + 추가 여유 거리 만큼 떨어진 곳을 시작점으로 설정
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float totalDistance = screenDiagonalRadius + _spawnOffsetDistance;
        _startPosition = camCenter + (randomDir * totalDistance);

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

        int hitCount = Physics2D.OverlapCircle(_hitPosition, _data.hitRadius, _filter, _hitBuffer);

        for (int i = 0; i < hitCount; i++)
        {
            var hit = _hitBuffer[i];

            if (hit.gameObject.TryGetComponent<IDamagable>(out var damagable))
            {
                damagable.TakeDamage(_data.damageToDamagable);
                Vector2 knockbackDirection = (Vector2)hit.transform.position - _hitPosition;
                damagable.ApplyKnockback(knockbackDirection, _data.knockbackFactor);
            }
            else if (hit.CompareTag("Player"))
            {
                SessionManager.Instance.ReceiveDamage(_data.damageToPlayer);
            }
        }

        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_hitPosition, _data.hitRadius);
    }
}
