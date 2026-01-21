using UnityEngine;

public enum WeaponStatType
{
    Unlock,

    Damage,
    Cooldown,
    Range,

    PickaxeKnockback,
    PickaxeDropIncrese,
    PickaxeMultiHit,

    ShotgunBulletCount,
    ShotgunSpread,
    ShotgunRebound,
    ShotgunPenetration,

    LasergunTransition,
    LasergunTransitionCount,
    LasergunTransitionRange,
}

[CreateAssetMenu(fileName = "NewWeaponUpgradeEffect", menuName = "Upgrades/Effects/Weapon Upgrade Effect")]
public class WeaponUpgradeEffect : UpgradeEffect
{
    public WeaponType weaponType;
    public WeaponStatType statType;
    public float value;

    public override void Apply()
    {
        if (statType == WeaponStatType.Unlock)
        {
            GameManager.Instance.CurrentData.playerSpec.currentWeapon = weaponType;
            return;
        }

        switch (weaponType)
        {
            case WeaponType.Pickaxe:
                ApplyPickaxe();
                break;
            case WeaponType.Shotgun:
                ApplyShotgun();
                break;
            case WeaponType.Lasergun:
                ApplyLasergun();
                break;
        }
    }

    private void ApplyPickaxe()
    {
        var stat = GameManager.Instance.CurrentData.playerSpec.pickaxeStat;

        switch (statType)
        {
            case WeaponStatType.Damage:
                if (stat.damage < value)
                    stat.damage = (int)value;
                break;
            case WeaponStatType.Cooldown:
                if (stat.cooldown > value)
                    stat.cooldown = value;
                break;
            case WeaponStatType.Range:
                if (stat.range < value)
                    stat.range = value;
                break;
            case WeaponStatType.PickaxeKnockback:
                if (stat.knockbackIntensity > value)
                    stat.knockbackIntensity = value;
                break;
            case WeaponStatType.PickaxeDropIncrese:
                if (stat.dropIncreseRate < value)
                    stat.dropIncreseRate = value;
                break;
            case WeaponStatType.PickaxeMultiHit:
                stat.isMultiHitUnlocked = true;
                break;
            default:
                Debug.LogWarning("Invalid stat type for pickaxe");
                break;
        }
    }

    private void ApplyShotgun()
    {
        var stat = GameManager.Instance.CurrentData.playerSpec.shotgunStat;

        switch (statType)
        {
            case WeaponStatType.Damage:
                if (stat.bulletData.damage < value)
                    stat.bulletData.damage = (int)value;
                break;
            case WeaponStatType.Cooldown:
                if (stat.cooldown > value)
                    stat.cooldown = value;
                break;
            case WeaponStatType.Range:
                if (stat.bulletData.speed < value)
                    stat.bulletData.speed = value;
                break;
            case WeaponStatType.ShotgunBulletCount:
                if (stat.bulletCount < value)
                    stat.bulletCount = (int)value;
                break;
            case WeaponStatType.ShotgunSpread:
                if (stat.spreadAngle > value)
                    stat.spreadAngle = value;
                break;
            case WeaponStatType.ShotgunRebound:
                if (stat.reboundIntensity > value)
                    stat.reboundIntensity = value;
                break;
            case WeaponStatType.ShotgunPenetration:
                stat.bulletData.isPenetrationUnlocked = true;
                break;
            default:
                Debug.LogWarning("Invalid stat type for shotgun");
                break;
        }
    }

    private void ApplyLasergun()
    {
        var stat = GameManager.Instance.CurrentData.playerSpec.lasergunStat;

        switch (statType)
        {
            case WeaponStatType.Damage:
                if (stat.damage < value)
                    stat.damage = (int)value;
                break;
            case WeaponStatType.Cooldown:
                if (stat.cooldown > value)
                    stat.cooldown = value;
                break;
            case WeaponStatType.Range:
                if (stat.range < value)
                    stat.range = value;
                break;
            case WeaponStatType.LasergunTransition:
                stat.isTransitionUnlocked = true;
                break;
            case WeaponStatType.LasergunTransitionCount:
                if (stat.transitionCount < value)
                    stat.transitionCount = (int)value;
                break;
            case WeaponStatType.LasergunTransitionRange:
                if (stat.transitionRange < value)
                    stat.transitionRange = value;
                break;
            default:
                Debug.LogWarning("Invalid stat type for lasergun");
                break;
        }
    }
}
