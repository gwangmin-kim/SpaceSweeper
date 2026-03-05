using UnityEngine;
using UnityEngine.Pool;
// using BreakInfinity;

public class ResourceItem : MonoBehaviour
{
    public enum ResourceState
    {
        Spawning, // 드롭되어 튀어 나가는 중
        Idle, // 떠있는 상태로 대기
        Attracted // 플레이어에게 끌려가는 상태
    }

    [Header("State (Read Only)")]
    [SerializeField] ResourceState _currentState = ResourceState.Idle;

    [Header("Resource Status")]
    [SerializeField] double _defaultValue;
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
    [SerializeField] float _lootRadius;
    float _lootRadiusSqr;

    Vector2 _dropPosition = Vector2.zero; // 초기 드롭 시의 목표 지점, UnitCircle 내부에서 랜덤 결정 (normalize 안함) 이후 Factor와 곱해서 결정
    float _standbyTimer = 0f;

    Transform _target;
    float _attractTimer = 0f;

    Vector2 _linearVelocity = Vector2.zero;
    float _angularVelocity = 0f;

    // object pooling
    IObjectPool<ResourceItem> _managedPool;

    // update optimize
    float _updateInterval = 0.1f;
    float _updateTimer = 0f;

    // 풀 참조 설정
    public void SetPool(IObjectPool<ResourceItem> pool)
    {
        _managedPool = pool;
    }

    void Awake()
    {
        // set random rotation
        transform.Rotate(Vector3.forward, Random.Range(0f, 360f));

        _lootRadiusSqr = _lootRadius * _lootRadius;
    }

    void Start()
    {
        SessionManager.Instance.OnGlobalMagnetTriggered += OnGlobalMagnetTriggered;
    }

    void Update()
    {
        transform.position += (Vector3)_linearVelocity * Time.deltaTime;
        transform.Rotate(Vector3.forward, _angularVelocity * Time.deltaTime);

        _updateTimer += Time.deltaTime;

        switch (_currentState)
        {
            case ResourceState.Spawning:
                HandleSpawning(Time.deltaTime);
                break;
            case ResourceState.Idle:
                if (_updateTimer >= _updateInterval)
                    HandleIdle();
                break;
            case ResourceState.Attracted:
                if (_updateTimer >= _updateInterval)
                    HandleAttracted(_updateInterval);
                break;
        }

        if (_updateTimer > _updateInterval)
        {
            _updateTimer = 0f;
        }
    }

    // 폐기물에서 드롭된 직후
    void HandleSpawning(float deltaTime)
    {
        _standbyTimer -= deltaTime;
        transform.position = Vector2.Lerp(transform.position, _dropPosition, _explodeSpeedFactor * deltaTime);

        if (_standbyTimer <= 0f)
        {
            _currentState = ResourceState.Idle;
            _linearVelocity = Vector2.zero;
            _angularVelocity = Random.Range(-_maxRotationAnglePerSecond, _maxRotationAnglePerSecond);
        }
    }

    void HandleIdle()
    {
        ConstrainPosition();
    }

    void ConstrainPosition()
    {
        if (InGameRoutineManager.Instance == null) return;

        Bounds mapBounds = InGameRoutineManager.Instance.CurrentMapBounds;
        Vector2 currentPosition = transform.position;

        // 맵 반대편으로 텔레포트
        if (currentPosition.x < mapBounds.min.x)
        {
            currentPosition.x = mapBounds.max.x;
        }
        else if (currentPosition.x > mapBounds.max.x)
        {
            currentPosition.x = mapBounds.min.x;
        }

        if (currentPosition.y < mapBounds.min.y)
        {
            currentPosition.y = mapBounds.max.y;
        }
        else if (currentPosition.y > mapBounds.max.y)
        {
            currentPosition.y = mapBounds.min.y;
        }

        transform.position = currentPosition;
    }

    void HandleAttracted(float deltaTime)
    {
        // 충돌 검사
        Vector2 deltaPosition = _target.position - transform.position;
        float sqrDistance = deltaPosition.sqrMagnitude;

        if (sqrDistance < _lootRadiusSqr)
        {
            SessionManager.Instance.LootResource(_value);
            Deactivate();
        }

        _attractTimer += deltaTime;

        // 플레이어 쪽으로 유도
        Vector3 attractDirection = deltaPosition.normalized;
        float attractSpeed = Mathf.Lerp(_minAttractSpeed, _maxAttractSpeed, _attractTimer * _attractSpeedFactor);
        _linearVelocity = attractSpeed * attractDirection;
    }

    // 초기 맵과 함께 생성 시 호출
    public void InitFloating(Vector2 direction)
    {
        _value = _defaultValue;

        _target = null;
        _currentState = ResourceState.Idle;

        float floatingSpeed = Random.Range(_minDriftSpeed, _maxDriftSpeed);
        _linearVelocity = floatingSpeed * direction;
        _angularVelocity = Random.Range(-_maxRotationAnglePerSecond, _maxRotationAnglePerSecond);
    }

    // 폐기물 파괴로 생성 시 호출
    public void InitDrop(float valueRate, float intensity)
    {
        _value = _defaultValue * valueRate;

        _target = null;
        _currentState = ResourceState.Spawning;

        _standbyTimer = _standbyDuration;
        _dropPosition = (Vector2)transform.position + _explodeDistance * Random.insideUnitCircle * intensity;
    }

    void OnGlobalMagnetTriggered()
    {
        if (SpacePlayerController.Instance == null) return;
        Transform target = SpacePlayerController.Instance.transform;
        SetTarget(target);
    }

    public void SetTarget(Transform target)
    {
        if (target == null || _currentState == ResourceState.Attracted) return;

        _target = target;
        _currentState = ResourceState.Attracted;

        _attractTimer = 0f;
    }

    void Deactivate()
    {
        if (_managedPool != null)
        {
            _managedPool.Release(this); // 풀로 반환
        }
        else
        {
            Destroy(gameObject); // 혹시 풀이 없으면 그냥 파괴
        }
    }
}
