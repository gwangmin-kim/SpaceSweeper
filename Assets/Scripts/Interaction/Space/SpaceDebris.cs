using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SpaceDebris : MonoBehaviour, IDamagable, IBlackholeAffectable, IMagneticStormAffectable
{
    Rigidbody2D _rigidbody;

    [Header("Status")]
    [SerializeField] int _maxHealth;
    [SerializeField] float _radius; // 대략적인 반지름 크기: 파괴 시 자원 파편이 생성되는 영역 반경을 결정
    [SerializeField] float _varianceRate; // 이 수치에 따라 크기/체력이 일정 범위 내에서 랜덤하게 생성

    [Header("Gimick")]
    [SerializeField] bool _isOverloaded = false;

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

    // blackhole gimick
    Vector2 _externalVelocity = Vector2.zero;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        // 최적화를 위한 프로퍼티 블록 생성
        _materialPropertyBlock = new MaterialPropertyBlock();

        // 랜덤성 부여
        float sizeRate = Random.Range(1f - _varianceRate, 1f + _varianceRate);
        transform.localScale *= sizeRate;
        _radius *= sizeRate;
        _maxHealth = (int)(_maxHealth * sizeRate);
        _dropCount = (int)(_dropCount * sizeRate);

        _currentHealth = _maxHealth;
        _currentHealthRatio = 1f;
        _targetHealthRatio = 1f;
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

        ApplyExternalVelocity();

        ConstrainPosition();
    }

    void Update()
    {
        SetHealthVisual();
    }

    void ApplyExternalVelocity()
    {
        _rigidbody.linearVelocity += _externalVelocity * _knockbackFactor;
        _externalVelocity = Vector2.zero;
    }

    void ConstrainPosition()
    {
        if (StageManager.Instance == null) return;

        Bounds mapBounds = StageManager.Instance.CurrentMapBounds;
        Vector2 currentPos = _rigidbody.position;
        Vector2 currentVel = _rigidbody.linearVelocity;

        float minX = mapBounds.min.x + _radius;
        float maxX = mapBounds.max.x - _radius;
        float minY = mapBounds.min.y + _radius;
        float maxY = mapBounds.max.y - _radius;

        bool isBounced = false;

        // 좌/우 벽 검사
        if (currentPos.x < minX)
        {
            currentPos.x = minX;
            if (currentVel.x < 0)
            {
                currentVel.x *= -1;
                isBounced = true;
            }
        }
        else if (currentPos.x > maxX)
        {
            currentPos.x = maxX;
            if (currentVel.x > 0)
            {
                currentVel.x *= -1;
                isBounced = true;
            }
        }

        // 상/하 벽 검사
        if (currentPos.y < minY)
        {
            currentPos.y = minY;
            if (currentVel.y < 0)
            {
                currentVel.y *= -1;
                isBounced = true;
            }
        }
        else if (currentPos.y > maxY)
        {
            currentPos.y = maxY;
            if (currentVel.y > 0)
            {
                currentVel.y *= -1;
                isBounced = true;
            }
        }

        if (isBounced)
        {
            _rigidbody.position = currentPos; // 위치 보정
            _rigidbody.linearVelocity = currentVel; // 속도 보정

            ApplyRandomRotation();
        }
    }

    void DropAndDestroy()
    {
        // 필요 시 확률 기반 드롭 카운트 배율 적용 (업그레이드 항목 고려)
        int dropCount = _dropCount;

        var data = GameManager.Instance.CurrentData;
        // 우선 곡괭이만 배율 업그레이드 적용
        if (data.playerSpec.currentWeapon == WeaponType.Pickaxe)
        {
            dropCount = (int)(dropCount * data.playerSpec.pickaxeStat.dropIncreseRate);
        }

        if (_isOverloaded)
        {
            dropCount = (int)(dropCount * data.gimickSpec.overloadDropRate);
        }

        for (int i = 0; i < dropCount; i++)
        {
            Vector2 spawnOffset = Random.insideUnitCircle * _radius;
            Vector2 spawnPosition = (Vector2)transform.position + spawnOffset;

            var resourceObject = Instantiate(_resourcePrefab, spawnPosition, Quaternion.identity);
            if (!resourceObject.TryGetComponent<ResourceItem>(out var resource))
            {
                Debug.LogWarning($"{resourceObject} is not a ResourceItem object");
                return;
            }
            resource.InitDrop();
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
        // 충돌 해결
        Transform myTransform = this.transform;
        Transform otherTransform = other.transform;

        Vector2 deltaPosition = myTransform.position - otherTransform.position;
        float distance = deltaPosition.magnitude;

        float radiusSum = this._radius + other._radius;
        float overlap = radiusSum - distance;

        // 겹쳐있다면 서로 반대 방향으로 절반씩 밀어냄
        // 만약 정가운데에 겹쳐서 생성됐다면 랜덤한 방향으로 밀어냄
        Vector2 direction = (distance == 0f) ? Random.insideUnitCircle.normalized : deltaPosition / distance;

        if (overlap > 0f)
        {
            // 정규화된 방향 벡터 * (겹친 만큼 / 2)
            Vector2 separationVector = 0.5f * overlap * direction;

            this._rigidbody.position += separationVector;
            other._rigidbody.position -= separationVector;
        }

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

        SessionManager.Instance.DealDamage(damage);

        if (_currentHealth <= 0 || _isOverloaded)
        {
            DropAndDestroy();
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

    public void ApplyBlackhole(Vector2 velocity)
    {
        _externalVelocity += velocity;
    }

    public void ApplyMagneticStorm(Vector2 velocity)
    {
        _externalVelocity += velocity;
        ApplyOverload();
    }

    public void ApplyOverload()
    {
        _isOverloaded = true;
    }
}
