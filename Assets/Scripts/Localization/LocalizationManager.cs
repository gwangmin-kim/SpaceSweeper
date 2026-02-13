using UnityEngine;
using System;

public class LocalizationManager : MonoBehaviour
{
    // Singleton
    public static LocalizationManager Instance { get; private set; }

    // 언어가 변경되었을 때 모든 UI에 알리기 위한 이벤트
    public event Action OnLanguageChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 언어 교체 함수
    public void SetLanguage(LanguageData newLanguage)
    {
        var currentSetting = GameManager.Instance.CurrentSetting;
        if (newLanguage == null || currentSetting.language == newLanguage) return;

        currentSetting.language = newLanguage;
        OnLanguageChanged?.Invoke(); // 구독 중인 모든 UI에 알림
    }

    // 키값을 통해 현재 언어에 맞는 텍스트 반환
    public string GetLocalizedText(string key)
    {
        var currentLanguage = GameManager.Instance.CurrentSetting.language;
        if (currentLanguage == null) return $"NO_LANG_DATA_{key}";
        return currentLanguage.GetText(key);
    }
}
