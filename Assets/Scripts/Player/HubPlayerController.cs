using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class HubPlayerController : MonoBehaviour
{
    Rigidbody2D _rigidbody;

    [Header("Movement")]
    [SerializeField] float _moveSpeed;
    [SerializeField] float _moveDamping;
    [SerializeField] float _jumpHeight;

    [Header("Sprite")]
    [SerializeField] Transform _visualRoot;

    [Header("Custom Gravity")]
    [SerializeField] float _gravity;
    [SerializeField] float _terminalVerticalSpeed;

    [Header("Ground Check")]
    [SerializeField] Vector2 _groundCheckOffset;
    [SerializeField] float _groundCheckRadius;
    [SerializeField] LayerMask _environmentLayer;

    // input caching
    float _horizontalInput = 0f;
    float _verticalInput = 0f;

    // smooth moving
    float _currentHorizontalFactor = 0f;

    // vertical speed (jump, gravity)
    [SerializeField] bool _isGrounded = true;
    float _jumpSpeed;
    float _currentVerticalSpeed = 0f;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 0f;

        _jumpSpeed = Mathf.Sqrt(2 * _gravity * _jumpHeight);
    }

    void FixedUpdate()
    {
        CheckGround();
        ApplyGravity();
        UpdateHorizontalMoveFactor();
        Move();

        ApplyVisualDirection();
    }

    void CheckGround()
    {
        bool wasGrounded = _isGrounded;
        _isGrounded = Physics2D.OverlapCircle((Vector2)transform.position + _groundCheckOffset, _groundCheckRadius, _environmentLayer);

        if (_isGrounded && !wasGrounded && _currentVerticalSpeed < 0f)
        {
            _currentVerticalSpeed = 0f;
        }
    }

    void ApplyGravity()
    {
        if (_isGrounded) return;

        _currentVerticalSpeed -= _gravity * Time.fixedDeltaTime;

        if (_currentVerticalSpeed < -_terminalVerticalSpeed)
        {
            _currentVerticalSpeed = -_terminalVerticalSpeed;
        }
    }

    void UpdateHorizontalMoveFactor()
    {
        _currentHorizontalFactor = Mathf.Lerp(_currentHorizontalFactor, _horizontalInput, _moveDamping * Time.fixedDeltaTime);
    }

    void Move()
    {
        Vector2 moveVelocity = new Vector2(_currentHorizontalFactor * _moveSpeed, _currentVerticalSpeed);
        _rigidbody.linearVelocity = moveVelocity;
    }

    void Jump()
    {
        if (!_isGrounded) return;

        _currentVerticalSpeed = _jumpSpeed;
    }

    void Interact()
    {

    }

    void ApplyVisualDirection()
    {
        Vector3 localScale = _visualRoot.localScale;

        localScale.x = (_horizontalInput != 0) ? _horizontalInput : localScale.x;

        _visualRoot.localScale = localScale;
    }

    public void OnMove(InputValue inputValue)
    {
        Vector2 move = inputValue.Get<Vector2>();

        Debug.Log($"input detected: move[{move}]");

        _horizontalInput = move.x;
        _verticalInput = move.y;
    }

    public void OnJump(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            Debug.Log($"input detected: jump");
            Jump();
        }
    }

    public void OnInteract(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            Debug.Log($"input detected: interact");
            Interact();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere((Vector2)transform.position + _groundCheckOffset, _groundCheckRadius);
    }
}
