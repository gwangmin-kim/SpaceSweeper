using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] float _velocityDampingTime; // 속도 감쇠 정도

    [Header("UI")]
    [SerializeField] SpriteRenderer _fillRenderer; // Health_Fill 오브젝트 연결
    [SerializeField] float _healthBarDampingTime;

    int _currentHealth;

    // HealthBar Shader
    float _currentHealthRatio = 1f;
    float _targetHealthRatio = 1f;
    float _currentVelocity = 0f;
    static readonly int _fillAmountID = Shader.PropertyToID("_FillAmount"); // 셰이더 프로퍼티 이름 (그래프 Blackboard에 만든 이름과 똑같아야 함)
    MaterialPropertyBlock _materialPropertyBlock;

    // linear damping
    bool _isInteracted = false; // 처음에는 감쇠 없이 초기 설정된 속도로 이동, 플레이어에 의한 첫 충돌 발생 시 감쇠 적용
    Vector2 _dampingReference = Vector2.zero;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        _currentHealth = _maxHealth;
        _currentHealthRatio = 1f;
        _targetHealthRatio = 1f;

        // 최적화를 위한 프로퍼티 블록 생성
        _materialPropertyBlock = new MaterialPropertyBlock();
    }

    void FixedUpdate()
    {
        if (_isInteracted)
        {
            // linear damping
            _rigidbody.linearVelocity = Vector2.SmoothDamp(
                _rigidbody.linearVelocity, Vector2.zero,
                ref _dampingReference, _velocityDampingTime);
        }
    }

    void Update()
    {
        SetHealthVisual();
    }

    void Die()
    {
        // 필요 시 확률 기반 드롭 카운트 배율 적용 (업그레이드 항목 고려)
        int dropCount = _dropCount;
        // 우선 곡괭이만 배율 업그레이드 적용
        if (GameManager.Instance.CurrentData.playerSpec.currentWeapon == WeaponType.Pickaxe)
        {
            dropCount = (int)(dropCount * GameManager.Instance.CurrentData.playerSpec.pickaxeStat.dropIncreseRate);
        }

        for (int i = 0; i < dropCount; i++)
        {
            Vector2 spawnOffset = Random.insideUnitCircle * _radius;
            Vector2 spawnPosition = (Vector2)transform.position + spawnOffset;

            Instantiate(_resourcePrefab, spawnPosition, Quaternion.identity);
        }

        StageManager.Instance.OnDebrisDestroy(gameObject);
    }

    void SetHealthVisual()
    {
        // UI
        _currentHealthRatio = Mathf.SmoothDamp(
            _currentHealthRatio,
            _targetHealthRatio,
            ref _currentVelocity,
            _healthBarDampingTime
        );

        _fillRenderer.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetFloat(_fillAmountID, _currentHealthRatio);
        _fillRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

    void HandleDebrisCollision(SpaceDebris other)
    {
        // 단순 속도 교환
        Vector2 thisVelocity = _rigidbody.linearVelocity;
        Vector2 otherVelocity = other._rigidbody.linearVelocity;

        _rigidbody.linearVelocity = otherVelocity;
        other._rigidbody.linearVelocity = thisVelocity;

        this.ApplyRandomRotation();
        other.ApplyRandomRotation();

        this._isInteracted = true;
        other._isInteracted = true;
    }

    void ApplyRandomRotation()
    {
        float rotationAnglePerSecond = Random.Range(-_maxRotationAnglePerSecond, _maxRotationAnglePerSecond);
        _rigidbody.angularVelocity = rotationAnglePerSecond;
    }

    public void InitMovement(Vector2 direction)
    {
        float floatingSpeed = Random.Range(_minDriftSpeed, _maxDriftSpeed);
        _rigidbody.linearVelocity = floatingSpeed * direction;
        ApplyRandomRotation();
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        _targetHealthRatio = Mathf.Clamp01(_currentHealth / (float)_maxHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void ApplyKnockback(Vector2 direction, float intensity)
    {
        Vector2 knockbackVelocity = intensity * _knockbackFactor * direction;
        _rigidbody.linearVelocity = knockbackVelocity;

        _isInteracted = true;

        // 회전은 랜덤하게 재적용
        ApplyRandomRotation();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.TryGetComponent<SpaceDebris>(out var other)) return;

        // 둘 중 한 쪽에서만 속도 교환 로직을 실행
        if (this.GetInstanceID() < other.GetInstanceID())
        {
            HandleDebrisCollision(other);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}
