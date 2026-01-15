using UnityEngine;

[RequireComponent(typeof(HubPlayerController))]
public class HubPlayerVisual : MonoBehaviour
{
    [Header("Player Controller")]
    [SerializeField] HubPlayerController _playerController;

    [Header("Head Follow")]
    [SerializeField] Transform _headRoot;
    [SerializeField] Transform _head;
    [SerializeField] Transform _face;
    [SerializeField] Vector2 _headAttractFactor;
    [SerializeField] Vector2 _faceAttractFactor;
    [SerializeField] float _headDamping;

    void Awake()
    {
        if (_playerController == null) _playerController = GetComponent<HubPlayerController>();
    }

    void Update()
    {
        ApplyHead();
    }

    void ApplyHead()
    {
        Vector2 headPosition = _headRoot.position;
        Vector2 aimPosition = _playerController.AimPosition;

        Vector2 direction = (aimPosition - headPosition).normalized;

        Vector2 headOffset = direction * _headAttractFactor;
        _head.localPosition = Vector2.Lerp(_head.localPosition, headOffset, _headDamping * Time.deltaTime);

        Vector2 faceOffset = direction;
        faceOffset.x *= _faceAttractFactor.x;
        faceOffset.y *= (faceOffset.y >= 0f) ? _faceAttractFactor.y : 0.5f * _faceAttractFactor.y;

        _face.localPosition = Vector2.Lerp(_face.localPosition, faceOffset, _headDamping * Time.deltaTime);
    }
}
