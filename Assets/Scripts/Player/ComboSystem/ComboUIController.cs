using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComboUIController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _comboText;
    [SerializeField] TextMeshProUGUI _comboTextBackground;
    [SerializeField] Image _maskImage;
    [SerializeField] float _fillDamping;

    float _targetFillAmount = 0f;
    int _currentLevelIndex = 0;

    Sequence _currentPopSequence;
    Sequence _currentShakeSequence;

    void OnEnable()
    {
        if (ComboManager.Instance == null)
        {
            Debug.LogWarning("ComboManager is null");
            return;
        }

        ComboManager.Instance.OnComboChanged += UpdateUI;

        _maskImage.fillAmount = 0f;
        _comboText.text = " ";
        _comboTextBackground.text = " ";
    }

    void OnDisable()
    {
        if (ComboManager.Instance == null)
        {
            Debug.LogWarning("ComboManager is null");
            return;
        }

        ComboManager.Instance.OnComboChanged -= UpdateUI;
    }

    void UpdateUI(int levelIndex, float progress)
    {
        // 점수가 상승했다면 흔들리는 애니메이션
        if (_targetFillAmount < progress)
        {
            ShakeAnimation();
        }
        // 단계가 상승했다면 튀어오르는 애니메이션
        else if (_currentLevelIndex < levelIndex)
        {
            PopAnimation();
        }


        // 텍스트 갱신
        string text = ComboManager.Instance.GetLevelString(levelIndex);

        _targetFillAmount = progress;
        _comboText.text = text;
        _comboTextBackground.text = text;

        // Debug.Log("Combo UI Updated");
    }

    void PopAnimation()
    {
        if (_currentPopSequence != null && _currentPopSequence.IsActive())
        {
            _currentPopSequence.Kill();
        }
        transform.localScale = Vector3.one;
        _currentPopSequence = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(transform.DOScale(1.5f, 0.01f))
            .Append(transform.DOScale(1.0f, 0.5f)).SetEase(Ease.InCubic);
        _currentPopSequence.Play();
    }

    void ShakeAnimation()
    {
        if (_currentShakeSequence != null && _currentShakeSequence.IsActive())
        {
            _currentShakeSequence.Kill();
        }
        transform.rotation = Quaternion.identity;
        _currentShakeSequence = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(transform.DORotate(new Vector3(0f, 0f, -5f), 0.02f))
            .Append(transform.DORotate(new Vector3(0f, 0f, 5f), 0.02f))
            .Append(transform.DORotate(new Vector3(0f, 0f, 0f), 0.02f));
        _currentShakeSequence.Play();
    }

    void Update()
    {
        _maskImage.fillAmount = Mathf.Lerp(
            _maskImage.fillAmount, _targetFillAmount, _fillDamping * Time.deltaTime);
    }
}
