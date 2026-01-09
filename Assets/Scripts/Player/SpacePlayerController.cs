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

    [Header("Combat")]
    [SerializeField] Transform _weaponSocket;
    [SerializeField] float _attackCommandInterval; // 공격 키를 누르고 있을 때 공격 명령을 내리는 주기, 실제 공격 여부는 무기 오브젝트의 쿨다운으로 결정됨

    [Header("Collision")]
    [SerializeField] LayerMask _collisionLayer;
    [SerializeField] float _bounceFactor; // 벽에 '부딪쳤을 때' 튕겨나가는 정도
    [SerializeField] float _knockbackFactor; // 동적으로 움직이는 물체에 '맞았을 때' 튕겨나가는 정도
    [SerializeField] float _knockbackDuration; // 최소 넉백 시간

    // input caching
    Vector2 _moveInput = Vector2.zero;
    Vector2 _aimPosition = Vector2.zero; // worldPosition
    [SerializeField] bool _isAttackPressed = false;

    public Vector2 AimPosition => _aimPosition;

    // smooth moving
    Vector2 _currentMoveFactor = Vector2.zero;

    // dash
    Vector2 _dashDirection = Vector2.zero;
    float _dashDuration;
    float _dashTimer = 0f;
    float _dashCooldownTimer = 0f;
    bool IsDashing => _dashTimer > 0f;
    bool IsDashReady => _dashCooldownTimer <= 0f; // 해금 조건도 여기에 추가할 수도

    // combat
    [SerializeField] GameObject _currentWeapon;
    float _attackCommandTimer = 0f;
    bool IsAttackReady => _attackCommandTimer <= 0f;

    // knock back
    Vector2 _knockbackDirection = Vector2.zero;
    float _knockbackTimer = 0f;
    bool IsKnockbacking => _knockbackTimer > 0f;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 0f;
    }

    void Start()
    {
        // 매니저로부터 현재 상태를 받아와서 플레이어 상태 초기화

        // 현재는 고정이지만, 업그레이드 등으로 수치가 변경되면 재계산 필요
        _dashDuration = _dashDistance / _dashSpeed;
        _currentWeapon = _weaponSocket.GetChild(0).gameObject;
    }

    void FixedUpdate()
    {
        if (IsKnockbacking)
        {
            Knockback();
            _knockbackTimer -= Time.fixedDeltaTime;
        }
        if (IsDashing)
        {
            Dash();
            _dashTimer -= Time.fixedDeltaTime; // 0 이상이라는 조건이 이미 분기에 포함되어 있음
        }
        else
        {
            Move();
            if (_dashCooldownTimer > 0f) _dashCooldownTimer -= Time.fixedDeltaTime;
        }

        if (_isAttackPressed && IsAttackReady)
        {
            SendAttack();
        }
        if (_attackCommandTimer > 0f) _attackCommandTimer -= Time.fixedDeltaTime;

        ApplyVisual();
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

    void ApplyVisual()
    {
        // 좌우 방향 설정
        Vector3 localScale = _visualRoot.localScale;

        // 단순히 이동 방향을 바라보도록 구현하는 경우
        // localScale.x = Mathf.Sign(_currentMoveFactor.x);

        // 마우스 방향을 바라보도록 구현
        localScale.x = Mathf.Sign(_aimPosition.x - transform.position.x);

        _visualRoot.localScale = localScale;
    }

    void SendAttack()
    {
        if (_currentWeapon == null || !_currentWeapon.TryGetComponent<IWeapon>(out var weapon)) return;

        Vector2 aimDirection = (_aimPosition - (Vector2)transform.position).normalized;

        weapon.Attack(aimDirection);

        // Debug.Log($"[SendAttack] current weapon: {weapon}");
        // Debug.Log($"[SendAttack] aim direction: {aimDirection}");
    }

    void Knockback()
    {
        _rigidbody.linearVelocity = _knockbackFactor * _knockbackDirection;
    }

    public void StartKnockback(Vector2 direction, float intensity)
    {
        // 넉백 중이어도 새로 넉백 당하면 그 쪽에 맞춰 초기화 (no Guard)
        _knockbackTimer = _knockbackDuration;
        _knockbackDirection = intensity * direction; // not normalized
        _currentMoveFactor = direction;

        // cancel dash state
        if (IsDashing)
        {
            _dashTimer = 0f;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & _collisionLayer) != 0)
        {
            // 법선 방향으로 튕겨남
            // ? 나중에 반사각이나 반대 방향으로 변경 고려
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

        // Debug.Log($"input detected: move[{move}]");

        _moveInput = move;
    }

    public void OnDash(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            // Debug.Log($"input detected: dash");

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
        // Debug.Log($"input detected: attack");

        _isAttackPressed = inputValue.isPressed;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blueViolet;
        Gizmos.DrawLine(transform.position, _aimPosition);
    }
}
