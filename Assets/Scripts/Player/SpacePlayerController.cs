using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class PlayerMoveStat
{
    [Header("Movement")]
    public float speed;
    public float dampingTime;

    [Header("Dash")]
    public bool isDashUnlocked;
    public float dashSpeed;
    public float dashDistance;
    public float dashCooldown;

    [Header("Bounce & Knockback")]
    public float bounceFactor; // 벽에 '부딪쳤을 때' 튕겨나가는 정도
    public float knockbackFactor; // 동적으로 움직이는 물체에 '맞았을 때' 튕겨나가는 정도
    public float knockbackDuration; // 최소 넉백 시간
}

[RequireComponent(typeof(Rigidbody2D))]
public class SpacePlayerController : MonoBehaviour
{
    // Singleton
    public static SpacePlayerController Instance { get; private set; }

    Rigidbody2D _rigidbody;

    [Header("Movement")]
    [SerializeField] PlayerMoveStat _moveStat;

    [Header("Collision")]
    [SerializeField] LayerMask _collisionLayer;

    [Header("Combat")]
    [SerializeField] Transform _weaponSocket;
    [SerializeField] float _attackCommandInterval; // 공격 키를 누르고 있을 때 공격 명령을 내리는 주기, 실제 공격 여부는 무기 오브젝트의 쿨다운으로 결정됨
    [SerializeField] LayerMask _targetLayer;

    [Header("Sprite")]
    [SerializeField] Transform _visualRoot;

    // input caching
    Vector2 _moveInput = Vector2.zero;
    Vector2 _aimPosition = Vector2.zero; // worldPosition
    [SerializeField] bool _isAttackPressed = false;

    // smooth moving
    Vector2 _currentVelocity = Vector2.zero;
    Vector2 _currentVelocityReference = Vector2.zero;

    // dash
    bool _isDashUnlocked = false;
    Vector2 _dashDirection = Vector2.zero;
    float _dashDuration;
    float _dashTimer = 0f;
    float _dashCooldownTimer = 0f;
    bool IsDashing => _dashTimer > 0f;
    bool IsDashReady => _dashCooldownTimer <= 0f;

    // combat
    [SerializeField] GameObject _currentWeapon;
    float _attackCommandTimer = 0f;
    bool IsAttackReady => _attackCommandTimer <= 0f;
    public LayerMask TargetLayer => _targetLayer;

    // knock back
    float _knockbackTimer = 0f;

    // need for visual control
    public PlayerState State => _state;
    public Vector2 AimPosition => _aimPosition;
    public Vector2 MoveInput => _moveInput;
    public Vector2 CurrentVelocity => _currentVelocity;

    public enum PlayerState
    {
        Move, // Idle or Move
        Dash,
        Knockback,
    }

    PlayerState _state = PlayerState.Move;

    public void GetPlayerSpec()
    {
        // 매니저로부터 현재 상태를 받아와서 플레이어 상태 초기화
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager is not initialized");
            return;
        }

        // move status
        _moveStat = GameManager.Instance.CurrentData.playerSpec.moveStat;
        _dashDuration = _moveStat.dashDistance / _moveStat.dashSpeed;
        _isDashUnlocked = GameManager.Instance.CurrentData.playerSpec.moveStat.isDashUnlocked;

        // weapon
        var weaponPrefab = WeaponManager.Instance.GetCurrentWeapon();
        if (weaponPrefab != null)
        {
            _currentWeapon = Instantiate(weaponPrefab, _weaponSocket);
            _currentWeapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _currentWeapon.GetComponent<IWeapon>()?.Initialize();
        }
    }

    void Awake()
    {
        Instance = this;

        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 0f;
    }

    void Start()
    {
        GetPlayerSpec();
    }

    void FixedUpdate()
    {
        // move
        Move();

        // attack
        if (_isAttackPressed && IsAttackReady)
        {
            SendAttack();
        }
        if (_attackCommandTimer > 0f) _attackCommandTimer -= Time.fixedDeltaTime;
    }

    void SetState(PlayerState state)
    {
        if (_state == state) return;
        _state = state;

        switch (state)
        {
            case PlayerState.Move:
                _dashTimer = 0f;
                _knockbackTimer = 0f;
                break;
            case PlayerState.Dash:
                _dashTimer = _dashDuration;
                _dashCooldownTimer = _moveStat.dashCooldown;
                _knockbackTimer = 0f;
                break;
            case PlayerState.Knockback:
                _knockbackTimer = _moveStat.knockbackDuration;
                _dashTimer = 0f;
                break;
        }
    }

    void Move()
    {
        switch (_state)
        {
            case PlayerState.Move:
                Vector2 targetVelocity = _moveStat.speed * _moveInput;
                _currentVelocity = Vector2.SmoothDamp(
                    _currentVelocity, targetVelocity,
                    ref _currentVelocityReference, _moveStat.dampingTime);

                break;
            case PlayerState.Dash:
                _dashTimer -= Time.fixedDeltaTime;

                if (_dashTimer <= 0f)
                {
                    SetState(PlayerState.Move);
                }

                break;
            case PlayerState.Knockback:
                _knockbackTimer -= Time.fixedDeltaTime;

                if (_knockbackTimer <= 0f)
                {
                    SetState(PlayerState.Move);
                }

                break;
        }

        if (_state != PlayerState.Dash && _dashCooldownTimer > 0f)
            _dashCooldownTimer -= Time.fixedDeltaTime;

        _rigidbody.linearVelocity = _currentVelocity;
    }

    void StartDash()
    {
        // 추가 조건 검사 로직 필요 (해금 여부)
        if (IsDashing || !IsDashReady) return;

        _dashDirection = (_moveInput.sqrMagnitude > 0f) ? _moveInput : _currentVelocity.normalized;
        _currentVelocity = _moveStat.dashSpeed * _dashDirection;

        SetState(PlayerState.Dash);
    }

    public void StartKnockback(Vector2 direction, float intensity)
    {
        // 넉백 중이어도 새로 넉백 당하면 그 쪽에 맞춰 초기화 (no Guard)
        _currentVelocity = _moveStat.knockbackFactor * intensity * direction;

        SetState(PlayerState.Knockback);
    }

    void SendAttack()
    {
        if (_currentWeapon == null ||
            !_currentWeapon.TryGetComponent<IWeapon>(out var weapon)) return;

        Vector2 aimDirection = (_aimPosition - (Vector2)transform.position).normalized;

        weapon.Attack(aimDirection);

        // Debug.Log($"[SendAttack] current weapon: {weapon}");
        // Debug.Log($"[SendAttack] aim direction: {aimDirection}");
    }

    void SendReturn()
    {
        // 우주선 귀환 시도
        if (SessionManager.Instance != null) SessionManager.Instance.TryReturn();
    }

    void SendCancel()
    {
        if (SessionManager.Instance != null) SessionManager.Instance.Cancel();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & _collisionLayer) != 0)
        {
            // 법선 방향으로 튕겨남
            // ? 나중에 반사각이나 반대 방향으로 변경 고려
            Vector2 normal = collision.contacts[0].normal;

            _currentVelocity = _currentVelocity.magnitude * _moveStat.bounceFactor * normal;

            // cancel dash or knockback state
            SetState(PlayerState.Move);
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
        if (_isDashUnlocked && inputValue.isPressed)
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

    public void OnInteract(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            // Debug.Log($"input detected: interact");
            SendReturn();
        }
    }

    public void OnCancel(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            // Debug.Log($"input detected: cancel");
            SendCancel();
        }
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
