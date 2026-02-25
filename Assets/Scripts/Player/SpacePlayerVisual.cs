using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Spaceship Indicator")]
    [SerializeField] Transform _spaceship;
    [SerializeField] GameObject _spaceshipIndicator;
    [SerializeField] Transform _directionArray;
    [SerializeField] float _distanceThreshold;
    [SerializeField] TextMeshProUGUI _distanceText;

    [Header("Dash Indicator")]
    [SerializeField] CanvasGroup _dashIndicator;
    [SerializeField] Image _dashBar;
    Sequence _dashSequence;

    void Awake()
    {
        if (_playerController == null) _playerController = GetComponent<SpacePlayerController>();

        _visualRoot.localScale = Vector3.zero;
        _visualRoot.DOScale(1.0f, 0.5f);
        _dashIndicator.alpha = 0f;
    }

    void Start()
    {
        _playerController.OnDashStarted += ApplyDash;
    }

    void Update()
    {
        ApplyHead(Time.deltaTime);
        ApplyBody(Time.deltaTime);
        ApplyHand(Time.deltaTime);

        ShowSpaceshipDirection();
    }

    void ApplyHead(float deltaTime)
    {
        Vector2 headPosition = _headRoot.position;
        Vector2 aimPosition = _playerController.AimPosition;

        Vector2 direction = (aimPosition - headPosition).normalized;

        Vector2 headOffset = direction * _headAttractFactor;
        _head.localPosition = Vector2.Lerp(_head.localPosition, headOffset, _headDamping * deltaTime);

        Vector2 faceOffset = direction;
        faceOffset.x *= _faceAttractFactor.x;
        faceOffset.y *= (faceOffset.y >= 0f) ? _faceAttractFactor.y : 0.5f * _faceAttractFactor.y;

        _face.localPosition = Vector2.Lerp(_face.localPosition, faceOffset, _headDamping * deltaTime);
    }

    void ApplyBody(float deltaTime)
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

        _bodyPivot.localRotation = Quaternion.Lerp(_bodyPivot.localRotation, targetRotation, _bodyTiltDamping * deltaTime);
    }

    void ApplyHand(float deltaTime)
    {
        Vector2 handPosition = _handRoot.position;
        Vector2 aimPosition = _playerController.AimPosition;
        Vector2 diff = aimPosition - handPosition;

        // 조준 위치를 따라감
        Vector2 handOffset = Vector2.ClampMagnitude(diff, 1f) * _handAttractFactor;
        _handSocket.localPosition = Vector2.Lerp(_handSocket.localPosition, handOffset, _handDamping * deltaTime);

        Quaternion targetRotation;

        if (diff.sqrMagnitude < _aimThreshold * _aimThreshold)
        {
            // 파지
            targetRotation = Quaternion.Euler(0, 0, _restAngle);

            _handSocket.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            // 조준
            Vector2 direction = diff.normalized;
            targetRotation = Quaternion.FromToRotation(Vector3.right, direction);

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
        _handSocket.rotation = Quaternion.Lerp(_handSocket.rotation, targetRotation, _weaponRotationDamping * deltaTime);
    }

    void ShowSpaceshipDirection()
    {
        Vector2 deltaPosition = _spaceship.position - _visualRoot.position;
        float distance = deltaPosition.magnitude;

        if (distance > _distanceThreshold)
        {
            _spaceshipIndicator.SetActive(true);
            _spaceshipIndicator.transform.up = deltaPosition.normalized;

            _distanceText.text = distance.ToString("0.0");
            Rect rect = _distanceText.rectTransform.rect;
            float angle = Mathf.Deg2Rad * _spaceshipIndicator.transform.localRotation.eulerAngles.z;

            _distanceText.transform.position = _directionArray.position + 0.025f * new Vector3(-rect.width * Mathf.Sin(angle), rect.height * Mathf.Cos(angle), 0f);
            _distanceText.transform.rotation = Quaternion.identity;
        }
        else
        {
            _spaceshipIndicator.SetActive(false);
        }
    }

    void ApplyDash(float cooldown)
    {
        // 대시 시작 시 호출되는 이벤트 구독
        // 쿨타임 동안 부드럽게 차오르는 효과 (fillAmount 조절)
        // 다 차면 살짝 pop 튀어오르고 사라지는 느낌
        _dashSequence?.Kill();
        _dashBar.fillAmount = 0f;
        _dashIndicator.alpha = 1f;
        _dashIndicator.transform.localScale = Vector3.one;

        _dashSequence = DOTween.Sequence().SetLink(gameObject);
        _dashSequence.Append(_dashBar.DOFillAmount(1f, cooldown).SetEase(Ease.Linear));
        _dashSequence.Append(_dashIndicator.transform.DOScale(1.2f, 0.1f).SetEase(Ease.OutQuad));
        _dashSequence.Append(_dashIndicator.transform.DOScale(1.0f, 0.1f).SetEase(Ease.InQuad));
        _dashSequence.Append(_dashIndicator.DOFade(0f, 0.3f).SetDelay(0.2f));
    }
}
