using DG.Tweening;
using UnityEngine;

public class PickaxeAnimation : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Transform _pickaxe;
    [SerializeField] Transform _rangeIndicator;

    [Header("Idle")]
    [SerializeField] float _rotateSpeed;

    [Header("Attack")]
    [SerializeField] float _upAngle; // 들어 올릴 때 각도
    [SerializeField] float _downAngle; // 내려 찍을 때 각도

    float _currentAngle = 0f;

    Sequence _currentSequence; // 기존 애니메이션이 실행 중이라면 취소하기 위해 변수 저장

    void Update()
    {
        _currentAngle += Time.deltaTime * _rotateSpeed % 360f;
        _rangeIndicator.rotation = Quaternion.Euler(0f, 0f, _currentAngle);
    }

    public void AttackAnimation(float totalTime, float attackTiming)
    {
        // 이전 애니메이션이 돌고 있다면 강제 종료 (중복 실행 방지)
        if (_currentSequence != null && _currentSequence.IsActive())
        {
            _currentSequence.Kill();
        }

        _currentSequence = DOTween.Sequence();

        float windUpTime = attackTiming * 0.4f;
        float strikeTime = attackTiming * 0.6f;
        float recoveryTime = totalTime - attackTiming;

        // 들어올리기
        _currentSequence.Append(_pickaxe.DOLocalRotate(new Vector3(0f, 0f, _upAngle), windUpTime)
            .SetEase(Ease.InOutCubic));

        // 내려 찍기
        _currentSequence.Append(_pickaxe.DOLocalRotate(new Vector3(0f, 0f, _downAngle), strikeTime)
            .SetEase(Ease.InBack));

        // 원위치 복귀
        _currentSequence.Append(_pickaxe.DOLocalRotate(Vector3.zero, recoveryTime)
            .SetEase(Ease.OutSine));

        // 실행
        _currentSequence.Play();
    }

    public void CancelAnimation()
    {
        _currentSequence?.Kill();
    }

    // 오브젝트 파괴 시 트윈 안전하게 종료
    void OnDisable()
    {
        CancelAnimation();
    }
}
