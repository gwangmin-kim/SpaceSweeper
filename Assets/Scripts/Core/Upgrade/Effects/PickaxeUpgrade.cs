using UnityEngine;

public enum PickaxeUpgradeType
{
    Unlock,

    Damage,
    AttackSpeed,
    Range,

    KnockbackIntensity,
    MultiHit,
    SetHitCount,

    PlusCriticalChance,
    CriticalDamage,
}

[CreateAssetMenu(fileName = "NewPickaxeUpgradeEffect", menuName = "Upgrades/Effects/Pickaxe Upgrade Effect")]
public class PickaxeUpgradeEffect : UpgradeEffect
{
    public PickaxeUpgradeType type;
    public float value;

    public override void Apply()
    {
        var stat = GameManager.Instance.CurrentData.playerSpec.pickaxeStat;

        switch (type)
        {
            case PickaxeUpgradeType.Unlock:
                var spec = GameManager.Instance.CurrentData.playerSpec;
                if (spec.currentWeapon == WeaponType.None)
                    spec.currentWeapon = WeaponType.Pickaxe;
                break;
            case PickaxeUpgradeType.Damage:
                stat.attackDamage *= value;
                break;
            case PickaxeUpgradeType.AttackSpeed:
                stat.attackSpeed *= value;
                break;
            case PickaxeUpgradeType.Range:
                stat.attackRange *= value;
                break;
            case PickaxeUpgradeType.KnockbackIntensity:
                stat.knockbackIntensity *= value;
                break;
            case PickaxeUpgradeType.MultiHit:
                stat.isMultiHitUnlocked = true;
                break;
            case PickaxeUpgradeType.SetHitCount:
                if (stat.hitCount < value)
                    stat.hitCount = (int)value;
                break;
            case PickaxeUpgradeType.PlusCriticalChance:
                stat.criticalChance = Mathf.Clamp01(stat.criticalChance + value);
                break;
            case PickaxeUpgradeType.CriticalDamage:
                stat.criticalDamage *= value;
                break;
            default:
                Debug.LogWarning("Invalid stat type for pickaxe");
                break;
        }
    }
}
