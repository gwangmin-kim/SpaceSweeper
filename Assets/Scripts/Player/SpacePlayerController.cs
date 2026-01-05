using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class SpacePlayerController : MonoBehaviour
{
    Rigidbody2D _rigidbody;

    [Header("Movement")]
    [SerializeField] float _moveSpeed;
    [SerializeField] float _moveDamping;

    [Header("Sprite")]
    [SerializeField] Transform _visualRoot;

    [Header("Dash")]
    [SerializeField] float _dashSpeed;
    [SerializeField] float _dashDistance;
    [SerializeField] float _dashCooldown;

    [Header("Collision")]
    [SerializeField] LayerMask _collisionLayer;
    [SerializeField] float _bounceFactor; // 넉백에 저항하는 정도를 결정
    [SerializeField] float _knockbackFactor; // 넉백에 저항하는 정도를 결정
    [SerializeField] float _knockbackDuration;

    // input caching
    Vector2 _moveInput = Vector2.zero;
    [SerializeField] Vector2 _aimPosition = Vector2.zero; // worldPosition

    // smooth moving
    Vector2 _currentMoveFactor = Vector2.zero;

    // dash
    Vector2 _dashDirection = Vector2.zero;
    float _dashDuration;
    float _dashTimer = 0f;
    float _dashCooldownTimer = 0f;
    bool IsDashing => _dashTimer > 0f;
    bool IsDashReady => _dashCooldownTimer <= 0f; // 해금 조건도 여기에 추가할 수도

    // knock back
    Vector2 _knockbackDirection = Vector2.zero;


    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 0f;

        // 현재는 고정이지만, 업그레이드 등으로 수치가 변경되면 재계산 필요
        _dashDuration = _dashDistance / _dashSpeed;
    }

    void FixedUpdate()
    {
        if (IsDashing)
        {
            Dash();
            _dashTimer -= Time.fixedDeltaTime;
        }
        else
        {
            Move();
            _dashCooldownTimer -= Time.fixedDeltaTime;
        }

        ApplyVisualDirection();
    }

    void Move()
    {
        _currentMoveFactor = Vector2.Lerp(_currentMoveFactor, _moveInput, _moveDamping * Time.fixedDeltaTime);
        _rigidbody.linearVelocity = _moveSpeed * _currentMoveFactor;
    }

    void Dash()
    {
        _rigidbody.linearVelocity = _dashSpeed * _dashDirection;
    }

    void StartDash()
    {
        // 추가 조건 검사 로직 필요 (해금 여부)
        if (IsDashing || !IsDashReady) return;

        _dashDirection = (_moveInput.sqrMagnitude > 0f) ? _moveInput : _currentMoveFactor.normalized;
        _currentMoveFactor = _dashDirection;

        _dashTimer = _dashDuration;
        _dashCooldownTimer = _dashCooldown;
    }

    void ApplyVisualDirection()
    {
        Vector3 localScale = _visualRoot.localScale;

        // 단순히 이동 방향을 바라보도록 구현하는 경우
        // localScale.x = Mathf.Sign(_currentMoveFactor.x);

        // 마우스 방향을 바라보도록 구현
        localScale.x = Mathf.Sign(_aimPosition.x - transform.position.x);

        _visualRoot.localScale = localScale;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & _collisionLayer) != 0)
        {
            // 법선 방향으로 튕겨남
            // 나중에 반사각이나 반대 방향으로 변경 고려
            Vector2 normal = collision.contacts[0].normal;

            _currentMoveFactor = _currentMoveFactor.magnitude * _bounceFactor * normal;

            // cancel dash state
            if (IsDashing)
            {
                _dashTimer = 0f;
            }
        }
    }

    public void OnMove(InputValue inputValue)
    {
        Vector2 move = inputValue.Get<Vector2>();

        Debug.Log($"input detected: move[{move}]");

        _moveInput = move;
    }

    public void OnDash(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            Debug.Log($"input detected: dash");

            StartDash();
        }
    }

    public void OnAim(InputValue inputValue)
    {
        Vector2 position = inputValue.Get<Vector2>();

        // Debug.Log($"input detected: aim[{position}]");

        _aimPosition = Camera.main.ScreenToWorldPoint(position);
    }

    public void OnAttack(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            Debug.Log($"input detected: attack");
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blueViolet;
        Gizmos.DrawLine(transform.position, _aimPosition);
    }
}
