using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LocalizationEntry
{
    public string key;
    [TextArea] public string value;
}

[CreateAssetMenu(fileName = "LangData", menuName = "Localization/Language Data")]
public class LanguageData : ScriptableObject
{
    public List<LocalizationEntry> entries = new List<LocalizationEntry>();

    private Dictionary<string, string> _cache;

    public string GetText(string key)
    {
        if (_cache == null || _cache.Count == 0)
        {
            _cache = new Dictionary<string, string>();
            foreach (var entry in entries) _cache[entry.key] = entry.value;
        }

        return _cache.TryGetValue(key, out string val) ? val : $"MISSING_{key}";
    }
}
