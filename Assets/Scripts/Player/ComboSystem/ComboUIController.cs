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

    Sequence _currentSequence;

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

    void UpdateUI(string text, float progress)
    {
        // 상승 중이라면 흔들리는 애니메이션
        if (_targetFillAmount < progress)
        {
            ShakeAnimation();
        }

        _targetFillAmount = progress;
        _comboText.text = text;
        _comboTextBackground.text = text;

        // Debug.Log("Combo UI Updated");
    }

    void ShakeAnimation()
    {
        if (_currentSequence != null && _currentSequence.IsActive())
        {
            _currentSequence.Kill();
        }
        transform.rotation = Quaternion.identity;
        _currentSequence = DOTween.Sequence();
        _currentSequence.Append(transform.DORotate(new Vector3(0f, 0f, 5f), 0.02f));
        _currentSequence.Append(transform.DORotate(new Vector3(0f, 0f, -5f), 0.02f));
        _currentSequence.Append(transform.DORotate(new Vector3(0f, 0f, 0f), 0.02f));
    }

    void Update()
    {
        _maskImage.fillAmount = Mathf.Lerp(
            _maskImage.fillAmount, _targetFillAmount, _fillDamping * Time.deltaTime);
    }
}
