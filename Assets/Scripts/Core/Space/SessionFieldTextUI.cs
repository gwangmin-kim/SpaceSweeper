using TMPro;
using UnityEngine;

public enum SessionInfoType
{
    LootAmount,
    LossAmount,

    TimeSpent,
    OxygenRestored,

    DamageDealt,
    CriticalDamageDealt,
    ExplosionDamage,

    DamageReceived,
    DestroyCount,
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
            SessionInfoType.LootAmount => BigDoubleFormatter.Format(info.lootAmount),
            SessionInfoType.LossAmount => BigDoubleFormatter.Format(info.lossAmount),

            SessionInfoType.TimeSpent => info.timer.ToString("0.0"),
            SessionInfoType.OxygenRestored => info.oxygenRestored.ToString("0.0"),

            SessionInfoType.DamageDealt => info.damageDealt.ToString("0.#"),
            SessionInfoType.CriticalDamageDealt => info.criticalDamageDealt.ToString("0.#"),
            SessionInfoType.ExplosionDamage => info.damageByDebris.ToString("0.#"),

            SessionInfoType.DamageReceived => info.oxygenLost.ToString("0.#"),
            SessionInfoType.DestroyCount => info.destroyCount.ToString(),


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

            SessionInfoType.OxygenRestored => info.oxygenRestored > 0f,

            SessionInfoType.DamageDealt => info.damageDealt > 0f,
            SessionInfoType.CriticalDamageDealt => info.criticalDamageDealt > 0f,
            SessionInfoType.ExplosionDamage => info.damageByDebris > 0f,

            SessionInfoType.DamageReceived => info.oxygenLost > 0f,
            SessionInfoType.DestroyCount => info.destroyCount > 0,

            // ...

            _ => true
        };
    }

    public void UpdateValue()
    {
        bool visibility = GetVisibility();
        // Debug.Log($"{gameObject.name} visibility: {visibility}");

        if (visibility)
            _value.text = GetValueFromData();

        gameObject.SetActive(visibility);
    }
}
