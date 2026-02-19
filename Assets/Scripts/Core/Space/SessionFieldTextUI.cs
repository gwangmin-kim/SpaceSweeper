using TMPro;
using UnityEngine;

public enum SessionInfoType
{
    TimeSpent,
    LootAmount,
    LossAmount,
    DamageReceived,
    DamageDealt,
    DestroyCount,
    OxygenRestored,
}

public class SessionFieldTextUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] TextMeshProUGUI _field;
    [SerializeField] TextMeshProUGUI _value;

    [Header("Value Type")]
    [SerializeField] SessionInfoType _infoType;

    void OnValidate()
    {
#if UNITY_EDITOR
        if (_field != null && _field.TryGetComponent<LocalizeTextUI>(out var component))
        {
            component.SetLocalizationKey(name);
        }
        if (System.Enum.TryParse(gameObject.name, out SessionInfoType result))
        {
            _infoType = result;
        }
        _field.text = name;
        _value.text = "0";
#endif
    }

    string GetValueFromData()
    {
        if (SessionManager.Instance == null) return "NO_SESSIONMANAGER";

        var info = SessionManager.Instance.Information;
        return _infoType switch
        {
            SessionInfoType.TimeSpent => info.timer.ToString("F2"),
            SessionInfoType.LootAmount => BigDoubleFormatter.Format(info.lootAmount),
            SessionInfoType.LossAmount => BigDoubleFormatter.Format(info.lossAmount),
            SessionInfoType.DamageReceived => info.oxygenLost.ToString("F2"),
            SessionInfoType.DamageDealt => info.damageDealt.ToString("F2"),
            SessionInfoType.DestroyCount => info.destroyCount.ToString(),
            SessionInfoType.OxygenRestored => info.oxygenRestored.ToString("F2"),

            // ...

            _ => "NOT_IMPLEMENTED"
        };
    }

    bool GetVisibility()
    {
        if (SessionManager.Instance == null) return false;

        var info = SessionManager.Instance.Information;
        return _infoType switch
        {
            SessionInfoType.LossAmount => !info.isSuccessful,
            SessionInfoType.DamageReceived => info.oxygenLost > 0f,
            SessionInfoType.DamageDealt => info.damageDealt > 0f,
            SessionInfoType.DestroyCount => info.destroyCount > 0,
            SessionInfoType.OxygenRestored => info.oxygenRestored > 0f,

            // ...

            _ => true
        };
    }

    public void UpdateValue()
    {
        bool visibility = GetVisibility();

        if (visibility)
            _value.text = GetValueFromData();

        gameObject.SetActive(visibility);
    }
}
