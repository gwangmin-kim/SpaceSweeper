using TMPro;
using UnityEngine;
using BreakInfinity;

public enum StatType
{
    // Move Status
    MoveSpeed,
    DampingTime,
    Dash,
    DashDistance,
    DashCooldown,
    BounceFactor,
    KnockbackFactor,
    KnockbackTime,

    // Exploration Status
    OxygenAmount,
    MagnetRange,
    LossRatio,
    GoldPerResource,
    ExchangeAmount,

    // Weapon
    CurrentWeapon,
    AttackDamage,
    AttackSpeed,
    AttackRange,

    // ... CSV 항목에 맞춰 추가
}

public class StatusFieldTextUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] TextMeshProUGUI _field;
    [SerializeField] TextMeshProUGUI _value;

    [Header("Value Type")]
    [SerializeField] StatType _statType;

    void OnValidate()
    {
#if UNITY_EDITOR
        if (_field != null && _field.TryGetComponent<LocalizeTextUI>(out var component))
        {
            component.SetLocalizationKey(name);
        }
        if (System.Enum.TryParse(gameObject.name, out StatType result))
        {
            _statType = result;
        }
        _field.text = name;
        _value.text = "0";
#endif
    }

    string GetValueFromData()
    {
        var data = GameManager.Instance.CurrentData;
        return _statType switch
        {
            StatType.MoveSpeed => data.playerSpec.moveStat.speed.ToString(),
            StatType.DampingTime => data.playerSpec.moveStat.dampingTime.ToString(),
            StatType.Dash => data.playerSpec.moveStat.isDashUnlocked ? "Unlocked" : "Locked",
            StatType.DashDistance => data.playerSpec.moveStat.dashDistance.ToString(),
            StatType.DashCooldown => data.playerSpec.moveStat.dashCooldown.ToString(),
            StatType.BounceFactor => data.playerSpec.moveStat.bounceFactor.ToString(),
            StatType.KnockbackFactor => data.playerSpec.moveStat.knockbackFactor.ToString(),
            StatType.KnockbackTime => data.playerSpec.moveStat.knockbackDuration.ToString(),

            StatType.OxygenAmount => data.playerSpec.oxygenAmount.ToString(),
            StatType.MagnetRange => data.playerSpec.magnetRange.ToString(),
            StatType.LossRatio => data.resourceSpec.lossRatio.ToString(),
            StatType.GoldPerResource => data.resourceSpec.goldPerResource.ToString(),
            StatType.ExchangeAmount => data.resourceSpec.exchangeAmount.ToString(),

            StatType.CurrentWeapon => data.playerSpec.currentWeapon.ToString(),
            StatType.AttackDamage => data.playerSpec.GetAttackDamageString(),
            StatType.AttackSpeed => data.playerSpec.GetAttackSpeedString(),
            StatType.AttackRange => data.playerSpec.GetAttackRangeString(),


            _ => "0"
        };
    }

    public void UpdateValue()
    {
        _value.text = GetValueFromData();
    }
}
