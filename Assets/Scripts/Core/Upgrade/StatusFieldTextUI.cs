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
    OxygenRestoreChance,
    OxygenRestoreAmount,
    MagnetRange,
    LossRatio,
    GoldPerResource,
    ExchangeAmount,

    // Weapon
    CurrentWeapon,
    AttackDamage,
    AttackSpeed,
    AttackRange,

    // Pickaxe
    Multihit,
    CriticalChance,
    CriticalDamage,

    // Debris
    DebrisHealth,
    DebrisSize,
    DebrisCount,
    DropValue,
    OverloadDrop,

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
        if (GameManager.Instance == null) return "";

        var data = GameManager.Instance.CurrentData;
        return _statType switch
        {
            StatType.MoveSpeed => data.playerSpec.moveStat.speed.ToString("0.##"),
            StatType.DampingTime => data.playerSpec.moveStat.dampingTime.ToString("0.##"),
            StatType.Dash => data.playerSpec.moveStat.isDashUnlocked ?
                LocalizationManager.Instance.GetLocalizedText("Unlocked") :
                LocalizationManager.Instance.GetLocalizedText("Locked"),
            StatType.DashDistance => data.playerSpec.moveStat.dashDistance.ToString("0.##"),
            StatType.DashCooldown => data.playerSpec.moveStat.dashCooldown.ToString("0.##"),
            StatType.BounceFactor => data.playerSpec.moveStat.bounceFactor.ToString("0.##"),
            StatType.KnockbackFactor => data.playerSpec.moveStat.knockbackFactor.ToString("0.##"),
            StatType.KnockbackTime => data.playerSpec.moveStat.knockbackDuration.ToString("0.##"),

            StatType.OxygenAmount => data.playerSpec.oxygenAmount.ToString("0.##"),
            StatType.MagnetRange => data.playerSpec.magnetRange.ToString("P0"),
            StatType.LossRatio => data.resourceSpec.lossRatio.ToString("P0"),
            StatType.GoldPerResource => BigDoubleFormatter.Format(data.resourceSpec.goldPerResource),
            StatType.ExchangeAmount => BigDoubleFormatter.Format(data.resourceSpec.exchangeAmount),

            StatType.CurrentWeapon => LocalizationManager.Instance.GetLocalizedText(data.playerSpec.currentWeapon.ToString()),
            StatType.AttackDamage => data.playerSpec.GetAttackDamageString(),
            StatType.AttackSpeed => data.playerSpec.GetAttackSpeedString(),
            StatType.AttackRange => data.playerSpec.GetAttackRangeString(),

            StatType.Multihit => data.playerSpec.pickaxeStat.hitCount >= 100 ?
                            LocalizationManager.Instance.GetLocalizedText("Infinity") :
                            data.playerSpec.pickaxeStat.hitCount.ToString(),
            StatType.CriticalChance => data.playerSpec.pickaxeStat.criticalChance.ToString("P0"),
            StatType.CriticalDamage => data.playerSpec.pickaxeStat.criticalChance.ToString("0.##"),

            StatType.DebrisHealth => data.debrisSpec.healthRate.ToString("P0"),
            StatType.DebrisSize => data.debrisSpec.sizeRate.ToString("P0"),
            StatType.DebrisCount => data.debrisSpec.spawnRate.ToString("P0"),
            StatType.DropValue => data.debrisSpec.valueRate.ToString("P0"),
            StatType.OverloadDrop => data.debrisSpec.overloadDropRate.ToString("P0"),

            StatType.OxygenRestoreChance => data.debrisSpec.oxygenRestoreChance.ToString("P0"),
            StatType.OxygenRestoreAmount => data.debrisSpec.oxygenRestoreAmount.ToString("0.##"),


            // ...
            _ => ""
        };
    }

    bool GetVisibility()
    {
        if (GameManager.Instance == null) return false;

        var data = GameManager.Instance.CurrentData;
        return _statType switch
        {
            StatType.DashCooldown or
            StatType.DashDistance => data.playerSpec.moveStat.isDashUnlocked,

            StatType.AttackDamage or
            StatType.AttackSpeed or
            StatType.AttackRange => data.playerSpec.currentWeapon != WeaponType.None,

            StatType.Multihit or
            StatType.CriticalChance => data.playerSpec.currentWeapon == WeaponType.Pickaxe,
            StatType.CriticalDamage => (data.playerSpec.currentWeapon == WeaponType.Pickaxe) &&
                (data.playerSpec.pickaxeStat.criticalChance > 0f),

            StatType.OxygenRestoreChance or
            StatType.OxygenRestoreAmount => data.debrisSpec.oxygenRestoreChance > 0f,

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
