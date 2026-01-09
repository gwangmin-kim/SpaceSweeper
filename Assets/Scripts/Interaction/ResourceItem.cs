using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ResourceItem : MonoBehaviour
{
    Collider2D _collider;

    [Header("Resource Status")]
    [SerializeField] int _value;

    [Header("Initialize Options")]
    [SerializeField] float _standbyDuration; // 생성 직후 곧바로 끌려가지 않고 일정 시간 대기
    [SerializeField] float _explodeDistance; // 초기 폭발 시 얼마나 멀리까지 날아가는지 결정
    [SerializeField] float _explodeSpeedFactor; // 초기 폭발 시 얼마나 빠르게 이동하는지 결정

    [Header("Floating Options")]
    [SerializeField] float _maxRotationAnglePerSecond;

    [Header("Attract Options")]
    [SerializeField] float _minAttractSpeed; // 최소 유도 속력
    [SerializeField] float _maxAttractSpeed; // 최대 유도 속력
    [SerializeField] float _attractSpeedFactor; // 플레이어에게 얼마나 빠르게 이끌릴지 결정

    [Header("Collision")]
    [SerializeField] LayerMask _playerLayer;
    [SerializeField] LayerMask _resourceLayer;

    public int Value => _value;

    Transform _target;
    float _initialInverseSqrDistance;

    Vector2 _explodeTargetPosition = Vector2.zero; // 초기 폭발 시의 목표 지점, UnitCircle 내부에서 랜덤 결정 (normalize 안함) 이후 Factor와 곱해서 결정
    float _standbyTimer = 0f;

    float _rotationAnglePerSecond;


    void Awake()
    {
        // set random rotation
        transform.Rotate(Vector3.forward, Random.Range(0f, 360f));

        _collider = GetComponent<Collider2D>();
        _collider.enabled = false;

        _standbyTimer = _standbyDuration;
        _explodeTargetPosition = (Vector2)transform.position + _explodeDistance * Random.insideUnitCircle;

        _rotationAnglePerSecond = Random.Range(-_maxRotationAnglePerSecond, _maxRotationAnglePerSecond);
    }

    // ! 움직이는 콜라이더는 Rigidbody를 달아주는 것이 효율적이라고 함. (https://docs.unity3d.com/6000.3/Documentation/Manual/CollidersOverview.html)
    // ! transform.position을 직접 수정하는 것에서 Rigidbody 기반 속도 제어로 변경 고려
    void Update()
    {
        if (_standbyTimer > 0f)
        {
            // 생성 직후
            _standbyTimer -= Time.deltaTime;
            transform.position = Vector2.Lerp(transform.position, _explodeTargetPosition, _explodeSpeedFactor * Time.deltaTime);
        }
        else if (_target == null)
        {
            if (!_collider.enabled) _collider.enabled = true;

            // 대기 상태
            transform.Rotate(Vector3.forward, _rotationAnglePerSecond * Time.deltaTime);
        }
        else
        {
            // 플레이어 쪽으로 유도
            Vector2 deltaPosition = _target.position - transform.position;
            float sqrDistance = deltaPosition.sqrMagnitude;
            Vector3 attractDirection = deltaPosition.normalized;

            attractDirection.z = 0f;

            // float distanceFactor = 1f / deltaPosition.sqrMagnitude; // 가까울 수록 빠르게 끌려옴
            // float attractSpeed = Mathf.Min(_minAttractSpeed, distanceFactor * _attractSpeedFactor);

            float attractSpeed = _attractSpeedFactor * Mathf.Lerp(_minAttractSpeed, _maxAttractSpeed, 1f - sqrDistance * _initialInverseSqrDistance);

            transform.position += attractSpeed * Time.deltaTime * attractDirection;
        }
    }

    public void SetTarget(Transform target)
    {
        _target = target;

        Vector2 deltaPosition = _target.position - transform.position;
        _initialInverseSqrDistance = 1f / deltaPosition.sqrMagnitude;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & _playerLayer) != 0)
        {
            // 플레이어를 거치지 않고 직접 리소스 매니저 호출
            // ResourceManager.Instance.EarnResources(item.Value);

            Destroy(gameObject);
        }
    }
}
