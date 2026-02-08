using UnityEngine;

public enum PickaxeUpgradeType
{
    Unlock,

    Damage,
    Cooldown,
    Range,

    KnockbackIntensity,
    MultiHit,
    HitCount,
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
                stat.damage *= value;
                break;
            case PickaxeUpgradeType.Cooldown:
                stat.cooldown *= value;
                break;
            case PickaxeUpgradeType.Range:
                stat.rangeRate *= value;
                break;
            case PickaxeUpgradeType.KnockbackIntensity:
                stat.knockbackIntensity *= value;
                break;
            case PickaxeUpgradeType.MultiHit:
                stat.isMultiHitUnlocked = true;
                break;
            case PickaxeUpgradeType.HitCount:
                if (stat.hitCount < value)
                    stat.hitCount = (int)value;
                break;
            default:
                Debug.LogWarning("Invalid stat type for pickaxe");
                break;
        }
    }
}
