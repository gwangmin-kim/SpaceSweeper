using System.Collections;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SpaceDebris : MonoBehaviour, IDamagable, IBlackholeAffectable, IMagneticStormAffectable
{
    Rigidbody2D _rigidbody;

    [Header("Status")]
    [SerializeField] float _maxHealth;
    [SerializeField] float _radius; // 대략적인 반지름 크기: 파괴 시 자원 파편이 생성되는 영역 반경을 결정
    [SerializeField] float _varianceRate; // 이 수치에 따라 크기/체력이 일정 범위 내에서 랜덤하게 생성
    public float Radius => _radius;

    [Header("Gimick-Overload")]
    [SerializeField] bool _isOverloaded;
    [SerializeField] bool _isOverloadableBySpawn;
    [SerializeField] float _overloadKnockbackIntensity;
    [SerializeField] float _overloadDestroyTime;
    [SerializeField] float _explosionRadius;
    [SerializeField] LayerMask _explosionTargetLayer;
    public bool IsOverloaded => _isOverloaded;
    public event System.Action<float, float> OnOverloadDestroyed;
    ContactFilter2D _explosionFilter;
    List<Collider2D> _explosionHitBuffer;

    [Header("Drop Settings")]
    [SerializeField] int _dropCount;

    [Header("Floating Settings")]
    [SerializeField] float _minDriftSpeed;
    [SerializeField] float _maxDriftSpeed;
    [SerializeField] float _maxRotationAnglePerSecond;

    [Header("Collision")]
    [SerializeField] float _knockbackFactor; // 밀려나는 정도: 작을 수록 적게 밀려남 (큰 폐기물은 이 값을 작게 설정하기)
    [SerializeField] float _velocityDampingTime; // 속도 감쇠 정도

    float _currentHealth;
    public event System.Action<float> OnHealthChanged;

    bool _isAffectable = true;
    public bool IsAffectable() => _isAffectable;

    // linear damping
    bool _isInteracted = false; // 처음에는 감쇠 없이 초기 설정된 속도로 이동, 플레이어에 의한 첫 충돌 발생 시 감쇠 적용
    Vector2 _dampingReference = Vector2.zero;

    // blackhole gimick
    Vector2 _externalVelocity = Vector2.zero;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        var spec = GameManager.Instance.CurrentData.debrisSpec;

        // 랜덤성 부여
        float variationRate = Random.Range(1f - _varianceRate, 1f + _varianceRate);
        transform.localScale *= variationRate * spec.sizeRate;
        _radius *= variationRate * spec.sizeRate;
        // _explosionRadius *= variationRate;

        _maxHealth = _maxHealth * variationRate * spec.healthRate;
        _dropCount = (int)(_dropCount * variationRate);

        _currentHealth = _maxHealth;
        OnHealthChanged?.Invoke(1f);

        // 과부하 설정
        if (_isOverloadableBySpawn && Random.value < spec.overloadChance)
        {
            ApplyOverload();
        }
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

    void ApplyExternalVelocity()
    {
        _rigidbody.linearVelocity += _externalVelocity * _knockbackFactor;
        _externalVelocity = Vector2.zero;
    }

    void ConstrainPosition()
    {
        if (InGameRoutineManager.Instance == null) return;

        Bounds mapBounds = InGameRoutineManager.Instance.CurrentMapBounds;
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

    float GetDropRate()
    {
        var data = GameManager.Instance.CurrentData;

        float dropRate = 1f;

        if (_isOverloaded)
        {
            dropRate *= data.debrisSpec.overloadDropRate;
        }

        return dropRate;
    }

    float GetValueRate()
    {
        var data = GameManager.Instance.CurrentData;

        float valueRate = data.debrisSpec.valueRate;

        return valueRate;
    }

    void DropAndDestroy()
    {
        _isAffectable = false;

        float dropRate = GetDropRate();
        float valueRate = GetValueRate();

        int dropCount = (int)(_dropCount * dropRate);

        for (int i = 0; i < dropCount; i++)
        {
            Vector2 spawnOffset = Random.insideUnitCircle * _radius;
            Vector2 spawnPosition = (Vector2)transform.position + spawnOffset;

            ResourceItem item = ResourcePoolManager.Instance.Get();
            item.transform.position = spawnPosition;
            item.InitDrop(valueRate, dropRate);
        }

        InGameRoutineManager.Instance.OnDebrisDestroy(gameObject);
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

        // this._isInteracted = true;
        // other._isInteracted = true;
    }

    void ApplyRandomRotation()
    {
        float rotationAnglePerSecond = Random.Range(-_maxRotationAnglePerSecond, _maxRotationAnglePerSecond);
        _rigidbody.angularVelocity = rotationAnglePerSecond;
    }

    void Explode()
    {
        int count = Physics2D.OverlapCircle(
                        transform.position, _explosionRadius, _explosionFilter, _explosionHitBuffer);

        for (int i = 0; i < count; i++)
        {
            var hit = _explosionHitBuffer[i];

            if (hit.gameObject.TryGetComponent<SpaceDebris>(out var debris))
            {
                var spec = GameManager.Instance.CurrentData.debrisSpec;
                float damage = _maxHealth * spec.overloadDamgeRate;

                Vector2 deltaPosition = debris.transform.position - transform.position;

                AttackInfo attackInfo = new AttackInfo
                {
                    source = AttackerType.Debris,
                    isCritical = false,
                    damage = damage,
                    direction = deltaPosition.normalized,
                    knockbackIntensity = _overloadKnockbackIntensity
                };

                debris.ApplyAttack(attackInfo);
            }
            else if (hit.gameObject.TryGetComponent<SpacePlayerController>(out var player))
            {
                Vector2 deltaPosition = player.transform.position - transform.position;
                player.ApplyKnockback(deltaPosition.normalized, _overloadKnockbackIntensity);
                SessionManager.Instance.ReceiveDamage();
            }
        }
    }

    IEnumerator OverloadExplosionRoutine()
    {
        var spec = GameManager.Instance.CurrentData.debrisSpec;
        _explosionRadius *= spec.overloadExplodeRange;
        float explosionScale = _explosionRadius / transform.localScale.x;
        Debug.Log($"explosion scale: {explosionScale}");
        OnOverloadDestroyed?.Invoke(explosionScale, _overloadDestroyTime);

        yield return new WaitForSeconds(_overloadDestroyTime);
        Explode();
        DropAndDestroy();
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
        if (_isOverloaded)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }

    public void InitMovement(Vector2 direction)
    {
        float floatingSpeed = Random.Range(_minDriftSpeed, _maxDriftSpeed);
        _rigidbody.linearVelocity = floatingSpeed * direction;
        ApplyRandomRotation();
    }

    public void ApplyAttack(AttackInfo attackInfo)
    {
        if (!_isAffectable) return;

        SessionManager.Instance.RecordAttack(attackInfo);
        ApplyKnockback(attackInfo.direction, attackInfo.knockbackIntensity);

        if (_isOverloaded)
        {
            _currentHealth = 0f;
            OnHealthChanged?.Invoke(0f);

            if (TryGetComponent<Collider2D>(out var collider))
            {
                collider.enabled = false;
            }

            if (_isAffectable)
            {
                _isAffectable = false;
                StartCoroutine(OverloadExplosionRoutine());
            }

            return;
        }

        _currentHealth -= attackInfo.damage;
        OnHealthChanged?.Invoke(_currentHealth / _maxHealth);

        if (_currentHealth <= 0 && _isAffectable)
        {
            _isAffectable = false;
            DropAndDestroy();
        }
    }

    public void ApplyKnockback(Vector2 direction, float intensity)
    {
        if (!_isAffectable) return;

        float knockbackSpeed = _isOverloaded ? _overloadKnockbackIntensity : intensity * _knockbackFactor;
        Vector2 knockbackVelocity = knockbackSpeed * direction;
        _rigidbody.linearVelocity = knockbackVelocity;

        _isInteracted = true;

        // 회전은 랜덤하게 재적용
        ApplyRandomRotation();
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
        if (_isOverloaded) return;

        _isOverloaded = true;

        _explosionFilter = new ContactFilter2D();
        _explosionFilter.SetLayerMask(_explosionTargetLayer);
        _explosionFilter.useTriggers = true;

        int hitCountPreset = 10;
        _explosionHitBuffer = new List<Collider2D>(hitCountPreset);
    }
}
