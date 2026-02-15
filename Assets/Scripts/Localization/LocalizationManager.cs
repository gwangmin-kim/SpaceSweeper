using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LanguageEntry
{
    public LanguageType type;
    public LanguageData data;
}

public class LocalizationManager : MonoBehaviour
{
    // Singleton
    public static LocalizationManager Instance { get; private set; }

    // language list
    [SerializeField] List<LanguageEntry> _languageList;
    Dictionary<LanguageType, LanguageData> _languageMap;

    // 언어가 변경되었을 때 모든 UI에 알리기 위한 이벤트
    public event System.Action OnLanguageChanged;

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

        InitMap();
    }

    void InitMap()
    {
        _languageMap = new Dictionary<LanguageType, LanguageData>();

        foreach (var langEntry in _languageList)
        {
            _languageMap[langEntry.type] = langEntry.data;
        }
    }

    // 언어 교체 함수
    public void SetLanguage(LanguageType languageType)
    {
        if (!_languageMap.ContainsKey(languageType)) return;

        var newLanguage = _languageMap[languageType];
        var currentSetting = GameManager.Instance.CurrentSetting;

        if (newLanguage == null || currentSetting.languageType == newLanguage.language) return;

        currentSetting.languageType = newLanguage.language;
        OnLanguageChanged?.Invoke(); // 구독 중인 모든 UI에 알림
    }

    // 키값을 통해 현재 언어에 맞는 텍스트 반환
    public string GetLocalizedText(string key)
    {
        var currentLanguageType = GameManager.Instance.CurrentSetting.languageType;
        if (!_languageMap.ContainsKey(currentLanguageType) || _languageMap[currentLanguageType] == null) return $"NO_LANG_DATA_{key}";
        return _languageMap[currentLanguageType].GetText(key);
    }
}
