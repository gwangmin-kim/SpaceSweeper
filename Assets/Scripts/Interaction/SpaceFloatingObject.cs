using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SpaceFloatingObject : MonoBehaviour, IDamagable
{
    Rigidbody2D _rigidbody;

    [Header("Status")]
    [SerializeField] int _maxHealth;

    [Header("Drop Settings")]
    [SerializeField] GameObject _resourcePrefab;
    [SerializeField] int _minDropCount;
    [SerializeField] int _maxDropCount;

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
}
