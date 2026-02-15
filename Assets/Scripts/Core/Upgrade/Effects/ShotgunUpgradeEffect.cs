using UnityEngine;

public enum ShotgunUpgradeType
{
    Unlock = 0,

    Damage = 10,
    Cooldown = 11,
    Range = 12,

    BulletCount = 20,
    SpreadAngle = 21,
    ReboundIntensity = 22,
    Penetration = 23,
}

[CreateAssetMenu(fileName = "NewShotgunUpgradeEffect", menuName = "Upgrades/Effects/Shotgun Upgrade")]
public class ShotgunUpgradeEffect : UpgradeEffect
{
    public ShotgunUpgradeType type;
    public float value;

    public override void Apply()
    {
        var stat = GameManager.Instance.CurrentData.playerSpec.shotgunStat;

        switch (type)
        {
            case ShotgunUpgradeType.Unlock:
                var spec = GameManager.Instance.CurrentData.playerSpec;
                if (spec.currentWeapon == WeaponType.None ||
                    spec.currentWeapon == WeaponType.Pickaxe)
                    spec.currentWeapon = WeaponType.Shotgun;
                break;
            case ShotgunUpgradeType.Damage:
                stat.bulletData.damage *= value;
                break;
            case ShotgunUpgradeType.Cooldown:
                stat.cooldown *= value;
                break;
            case ShotgunUpgradeType.Range:
                stat.bulletData.speed *= value;
                break;
            case ShotgunUpgradeType.BulletCount:
                stat.bulletCount += (int)value;
                break;
            case ShotgunUpgradeType.SpreadAngle:
                if (stat.spreadAngle > value)
                    stat.spreadAngle = value;
                break;
            case ShotgunUpgradeType.ReboundIntensity:
                stat.reboundIntensity *= value;
                break;
            case ShotgunUpgradeType.Penetration:
                stat.bulletData.isPenetrationUnlocked = true;
                break;
            default:
                Debug.LogWarning("Invalid stat type for shotgun");
                break;
        }
    }
}
