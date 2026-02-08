using UnityEngine;

public enum LasergunUpgradeType
{
    Unlock,

    Damage,
    Cooldown,
    Range,

    Transition,
    TransitionCount,
    TransitionRange,
}

[CreateAssetMenu(fileName = "NewLasergunUpgradeEffect", menuName = "Upgrades/Effects/Lasergun Upgrade Effect")]
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
                stat.cooldown *= value;
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
