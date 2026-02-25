using DG.Tweening;
using UnityEngine;

public class ShotgunAnimation : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Transform _shotgun;

    [Header("Attack")]
    [SerializeField] float _reboundAngle; // 반동 회전 각도
    [SerializeField] float _spinAngle; // 쿨타임 동안 회전 각도

    Sequence _currentSequence;

    public void AttackAnimation(float totalTime)
    {
        if (_currentSequence != null && _currentSequence.IsActive())
        {
            CancelAnimation();
        }

        _shotgun.localRotation = Quaternion.identity;

        float reboundTime = totalTime * 0.1f;
        float reboundRecoveryTime = totalTime * 0.1f;
        float spinRecoveryTime = totalTime * 0.1f;
        float spinTime = totalTime - reboundTime - reboundRecoveryTime - spinRecoveryTime;

        _currentSequence = DOTween.Sequence().SetLink(gameObject);

        // 반동
        _currentSequence.Append(_shotgun.DOLocalRotate(new Vector3(0f, 0f, _reboundAngle), reboundTime))
            .SetEase(Ease.OutQuint);
        _currentSequence.Append(_shotgun.DOLocalRotate(new Vector3(0f, 0f, 0f), reboundRecoveryTime))
            .SetEase(Ease.InOutSine);

        // 대기 중 회전
        _currentSequence.Append(_shotgun.DORotate(new Vector3(0f, 0f, _spinAngle), spinTime, RotateMode.LocalAxisAdd))
            .SetEase(Ease.InOutCubic);

        // 원위치 복귀
        _currentSequence.Append(_shotgun.DOLocalRotate(new Vector3(0f, 0f, 0f), reboundRecoveryTime))
            .SetEase(Ease.OutSine);
    }

    public void CancelAnimation()
    {
        _currentSequence?.Kill();
    }

    void OnDisable()
    {
        CancelAnimation();
    }
}
