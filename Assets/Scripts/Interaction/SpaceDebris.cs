using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SpaceDebris : MonoBehaviour, IDamagable
{
    Rigidbody2D _rigidbody;

    [Header("Status")]
    [SerializeField] int _maxHealth;
    [SerializeField] float _radius; // 대략적인 반지름 크기: 파괴 시 자원 파편이 생성되는 영역 반경을 결정

    [Header("Drop Settings")]
    [SerializeField] GameObject _resourcePrefab;
    [SerializeField] int _dropCount;

    [Header("Floating Settings")]
    [SerializeField] float _minDriftSpeed;
    [SerializeField] float _maxDriftSpeed;
    [SerializeField] float _maxRotationAnglePerSecond;

    [Header("Collision")]
    [SerializeField] float _knockbackFactor; // 밀려나는 정도: 작을 수록 적게 밀려남 (큰 폐기물은 이 값을 작게 설정하기)
    [SerializeField] float _linearDampingAfterCollision; // 자유롭게 떠다니다가, 첫 피격 이후부터는 감쇠 적용

    int _currentHealth;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        // set initial movement
        Vector2 floatingDirection = Random.insideUnitCircle.normalized;
        float floatingSpeed = Random.Range(_minDriftSpeed, _maxDriftSpeed);
        float rotationAnglePerSecond = Random.Range(-_maxRotationAnglePerSecond, _maxRotationAnglePerSecond);

        _rigidbody.linearVelocity = floatingSpeed * floatingDirection;
        _rigidbody.angularVelocity = rotationAnglePerSecond;
    }

    void Die()
    {
        // 필요 시 확률 기반 드롭 카운트 배율 적용 (업그레이드 항목 고려)

        for (int i = 0; i < _dropCount; i++)
        {
            Vector2 spawnOffset = Random.insideUnitCircle * _radius;
            Vector2 spawnPosition = (Vector2)transform.position + spawnOffset;

            Instantiate(_resourcePrefab, spawnPosition, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void ApplyKnockback(Vector2 direction, float intensity)
    {
        Vector2 knockbackVelocity = intensity * _knockbackFactor * direction;
        _rigidbody.linearVelocity = knockbackVelocity;

        _rigidbody.linearDamping = _linearDampingAfterCollision;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}
