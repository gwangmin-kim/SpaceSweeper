using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizeTextUI : MonoBehaviour
{
    [SerializeField] string _localizationKey;
    TextMeshProUGUI _text;

    void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        // 매니저의 언어 변경 이벤트 구독
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += RefreshText;
        }
        RefreshText();
    }

    void OnDisable()
    {
        // 메모리 누수 방지를 위한 구독 해제
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= RefreshText;
        }
    }

    public void SetLocalizationKey(string key)
    {
        _localizationKey = key;
    }

    // 실제 텍스트 갱신 로직
    public void RefreshText()
    {
        if (LocalizationManager.Instance == null || string.IsNullOrEmpty(_localizationKey)) return;
        _text.text = LocalizationManager.Instance.GetLocalizedText(_localizationKey);
    }
}
