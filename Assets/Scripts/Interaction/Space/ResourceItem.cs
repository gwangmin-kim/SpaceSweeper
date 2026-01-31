using UnityEngine;
// using BreakInfinity;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class ResourceItem : MonoBehaviour, IBlackholeAffectable, IMagneticStormAffectable
{
    public enum ResourceState
    {
        Spawning, // 드롭되어 튀어 나가는 중
        Idle, // 떠있는 상태로 대기
        Attracted // 플레이어에게 끌려가는 상태
    }

    Collider2D _collider;
    Rigidbody2D _rigidbody;

    [Header("State (Read Only)")]
    [SerializeField] ResourceState _currentState = ResourceState.Idle;

    [Header("Resource Status")]
    [SerializeField] double _value;

    [Header("Initialize Options")]
    [SerializeField] float _standbyDuration; // 생성 직후 곧바로 끌려가지 않고 일정 시간 대기
    [SerializeField] float _explodeDistance; // 초기 폭발 시 얼마나 멀리까지 날아가는지 결정
    [SerializeField] float _explodeSpeedFactor; // 초기 폭발 시 얼마나 빠르게 이동하는지 결정

    [Header("Floating Options")]
    [SerializeField] float _minDriftSpeed;
    [SerializeField] float _maxDriftSpeed;
    [SerializeField] float _maxRotationAnglePerSecond;

    [Header("Attract Options")]
    [SerializeField] float _minAttractSpeed; // 최소 유도 속력
    [SerializeField] float _maxAttractSpeed; // 최대 유도 속력
    [SerializeField] float _attractSpeedFactor; // 플레이어에게 얼마나 빠르게 이끌릴지 결정

    [Header("Collision")]
    [SerializeField] LayerMask _playerLayer;

    Vector2 _dropPosition = Vector2.zero; // 초기 드롭 시의 목표 지점, UnitCircle 내부에서 랜덤 결정 (normalize 안함) 이후 Factor와 곱해서 결정
    float _standbyTimer = 0f;

    Transform _target;
    float _attractTimer = 0f;

    // blackhole gimick
    Vector2 _externalVelocity = Vector2.zero;
    float _gimickLifeTimer = 0.5f;

    void Awake()
    {
        // set random rotation
        transform.Rotate(Vector3.forward, Random.Range(0f, 360f));

        _collider = GetComponent<Collider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    // ! 움직이는 콜라이더는 Rigidbody를 달아주는 것이 효율적이라고 함. (https://docs.unity3d.com/6000.3/Documentation/Manual/CollidersOverview.html)
    // ! transform.position을 직접 수정하는 것에서 Rigidbody 기반 속도 제어로 변경 고려
    void FixedUpdate()
    {
        switch (_currentState)
        {
            case ResourceState.Spawning:
                HandleSpawning();
                break;
            case ResourceState.Idle:
                HandleIdle();
                break;
            case ResourceState.Attracted:
                HandleAttracted();
                break;
        }

        ApplyExternalVelocity();

        if (_gimickLifeTimer <= 0f)
        {
            _collider.enabled = false;
            Destroy(gameObject);
        }
    }

    void ApplyExternalVelocity()
    {
        _rigidbody.linearVelocity += _externalVelocity;

        if (_externalVelocity.sqrMagnitude > 0f)
        {
            _gimickLifeTimer -= Time.fixedDeltaTime;
        }

        _externalVelocity = Vector2.zero;
    }

    // 폐기물에서 드롭된 직후
    void HandleSpawning()
    {
        _standbyTimer -= Time.fixedDeltaTime;
        _rigidbody.position = Vector2.Lerp(_rigidbody.position, _dropPosition, _explodeSpeedFactor * Time.fixedDeltaTime);

        if (_standbyTimer <= 0f)
        {
            InitFloating(Vector2.zero);
        }
    }

    void HandleIdle()
    {

    }

    void HandleAttracted()
    {
        _attractTimer += Time.fixedDeltaTime;

        // 플레이어 쪽으로 유도
        Vector2 deltaPosition = _target.position - transform.position;
        Vector3 attractDirection = deltaPosition.normalized;

        attractDirection.z = 0f;

        float attractSpeed = Mathf.Lerp(_minAttractSpeed, _maxAttractSpeed, _attractTimer * _attractSpeedFactor);

        _rigidbody.linearVelocity = attractSpeed * attractDirection;
    }

    // 초기 맵과 함께 생성 시 호출
    public void InitFloating(Vector2 direction)
    {
        _currentState = ResourceState.Idle;
        _collider.enabled = true;

        float floatingSpeed = Random.Range(_minDriftSpeed, _maxDriftSpeed);
        _rigidbody.linearVelocity = floatingSpeed * direction;
        _rigidbody.angularVelocity = Random.Range(-_maxRotationAnglePerSecond, _maxRotationAnglePerSecond);
    }

    // 폐기물 파괴로 생성 시 호출
    public void InitDrop()
    {
        _currentState = ResourceState.Spawning;
        _collider.enabled = false;

        _standbyTimer = _standbyDuration;
        _dropPosition = (Vector2)transform.position + _explodeDistance * Random.insideUnitCircle;
    }

    public void SetTarget(Transform target)
    {
        if (target == null || _currentState == ResourceState.Attracted) return;

        _target = target;
        _currentState = ResourceState.Attracted;

        _attractTimer = 0f;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & _playerLayer) != 0)
        {
            // 플레이어를 거치지 않고 직접 세션 매니저 호출
            SessionManager.Instance.LootResource(_value);

            Destroy(gameObject);
        }
    }

    public void ApplyBlackhole(Vector2 velocity)
    {
        _externalVelocity += velocity;
    }

    public void ApplyMagneticStorm(Vector2 velocity)
    {
        _externalVelocity += velocity;
    }
}
