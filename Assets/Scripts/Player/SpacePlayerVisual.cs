using UnityEngine;

[RequireComponent(typeof(SpacePlayerController))]
public class SpacePlayerVisual : MonoBehaviour
{
    [Header("Player Controller")]
    [SerializeField] SpacePlayerController _playerController;
    [SerializeField] Transform _visualRoot;

    [Header("Head Follow")]
    [SerializeField] Transform _headRoot;
    [SerializeField] Transform _head;
    [SerializeField] Transform _face;
    [SerializeField] Vector2 _headAttractFactor;
    [SerializeField] Vector2 _faceAttractFactor;
    [SerializeField] float _headDamping;

    [Header("Body Tilt")]
    [SerializeField] Transform _bodyPivot;
    [SerializeField] float _moveTiltAngle;
    [SerializeField] float _dashTiltAngle;
    [SerializeField] float _knockbackTiltAngle;
    [SerializeField] float _bodyTiltDamping;

    [Header("Hand Socket")]
    [SerializeField] Transform _handRoot;
    [SerializeField] Transform _handSocket;
    [SerializeField] Vector2 _handAttractFactor;
    [SerializeField] float _handDamping;

    [Header("Weapon")]
    [SerializeField] float _restAngle; // 쉴 때 각도 (예: 45도 들고 있기)
    [SerializeField] float _aimThreshold; // 파지/조준 모드로 전환되는 거리
    [SerializeField] float _weaponRotationDamping; // 회전 속도

    void Awake()
    {
        _playerController = GetComponent<SpacePlayerController>();
    }

    void Update()
    {
        ApplyHead();
        ApplyBody();
        ApplyHand();
    }

    void ApplyHead()
    {
        Vector2 headPosition = _headRoot.position;
        Vector2 aimPosition = _playerController.AimPosition;

        Vector2 direction = (aimPosition - headPosition).normalized;

        Vector2 headOffset = direction * _headAttractFactor;
        _head.localPosition = Vector2.Lerp(_head.localPosition, headOffset, _handDamping * Time.deltaTime);

        Vector2 faceOffset = direction;
        faceOffset.x *= _faceAttractFactor.x;
        faceOffset.y *= (faceOffset.y >= 0f) ? _faceAttractFactor.y : 0.5f * _faceAttractFactor.y;

        _face.localPosition = Vector2.Lerp(_face.localPosition, faceOffset, _headDamping * Time.deltaTime);
    }

    void ApplyBody()
    {
        Quaternion targetRotation = Quaternion.identity;

        switch (_playerController.State)
        {
            case SpacePlayerController.PlayerState.Move:
                targetRotation = Quaternion.Euler(
                    0f, 0f, -_playerController.MoveInput.normalized.x * _moveTiltAngle);
                break;
            case SpacePlayerController.PlayerState.Dash:
                if (_playerController.CurrentVelocity.x != 0f)
                {
                    targetRotation = Quaternion.Euler(
                        0f, 0f, -Mathf.Sign(_playerController.CurrentVelocity.x) * _dashTiltAngle);
                }
                break;
            case SpacePlayerController.PlayerState.Knockback:
                if (_playerController.CurrentVelocity.x != 0f)
                {
                    targetRotation = Quaternion.Euler(
                        0f, 0f, -Mathf.Sign(_playerController.CurrentVelocity.x) * _knockbackTiltAngle);
                }
                break;
        }

        _bodyPivot.localRotation = Quaternion.Lerp(_bodyPivot.localRotation, targetRotation, _bodyTiltDamping * Time.deltaTime);
    }

    void ApplyHand()
    {
        Vector2 handPosition = _handRoot.position;
        Vector2 aimPosition = _playerController.AimPosition;
        Vector2 diff = aimPosition - handPosition;

        // 조준 위치를 따라감
        Vector2 handOffset = Vector2.ClampMagnitude(diff, 1f) * _handAttractFactor;
        _handSocket.localPosition = Vector2.Lerp(_handSocket.localPosition, handOffset, _handDamping * Time.deltaTime);

        Quaternion targetRotation;

        if (diff.sqrMagnitude < _aimThreshold * _aimThreshold)
        {
            targetRotation = Quaternion.Euler(0, 0, _restAngle);
        }
        else
        {
            Vector2 direction = diff.normalized;
            targetRotation = Quaternion.FromToRotation(Vector3.right, direction);
        }
        _handSocket.rotation = Quaternion.Lerp(_handSocket.rotation, targetRotation, _weaponRotationDamping * Time.deltaTime);

        // 왼쪽을 바라볼 때 무기 좌우 반전
        if (diff.x < 0f)
        {
            _handSocket.localScale = new Vector3(1, -1, 1);
        }
        else
        {
            _handSocket.localScale = new Vector3(1, 1, 1);
        }
    }
}
