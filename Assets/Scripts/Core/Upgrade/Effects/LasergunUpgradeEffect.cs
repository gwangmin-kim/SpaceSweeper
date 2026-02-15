using UnityEngine;

public enum LasergunUpgradeType
{
    Unlock = 0,

    Damage = 10,
    Cooldown = 11,
    Range = 12,

    Transition = 20,
    TransitionCount = 21,
    TransitionRange = 22,
}

[CreateAssetMenu(fileName = "NewLasergunUpgradeEffect", menuName = "Upgrades/Effects/Lasergun Upgrade")]
public class LasergunUpgradeEffect : UpgradeEffect
{
    public LasergunUpgradeType type;
    public float value;

    public override void Apply()
    {
        var stat = GameManager.Instance.CurrentData.playerSpec.lasergunStat;

        switch (type)
        {
            case LasergunUpgradeType.Unlock:
                var spec = GameManager.Instance.CurrentData.playerSpec;
                if (spec.currentWeapon == WeaponType.None ||
                    spec.currentWeapon == WeaponType.Pickaxe)
                    spec.currentWeapon = WeaponType.Shotgun;
                break;
            case LasergunUpgradeType.Damage:
                stat.damage *= value;
                break;
            case LasergunUpgradeType.Cooldown:
                stat.hitInterval *= value;
                break;
            case LasergunUpgradeType.Range:
                stat.range *= value;
                break;
            case LasergunUpgradeType.Transition:
                stat.isTransitionUnlocked = true;
                break;
            case LasergunUpgradeType.TransitionCount:
                if (stat.transitionCount < value)
                    stat.transitionCount = (int)value;
                break;
            case LasergunUpgradeType.TransitionRange:
                stat.transitionRange *= value;
                break;
            default:
                Debug.LogWarning("Invalid stat type for lasergun");
                break;
        }
    }
}
