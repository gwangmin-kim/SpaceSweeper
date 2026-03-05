using UnityEngine;

public enum BonusPickaxeUpgradeType
{
    Unlock = 0,

    PlusCount = 10,
    AttackRadius = 11,
    RotationAngle = 12,
}

[CreateAssetMenu(fileName = "NewBonusPickaxeUpgradeEffect", menuName = "Upgrades/Effects/Pickaxe Upgrade/Bonus")]
public class BonusPickaxeUpgradeEffect : UpgradeEffect
{
    public BonusPickaxeUpgradeType type;
    public float value;

    public override void Apply()
    {
        var stat = GameManager.Instance.CurrentData.playerSpec.bonusPickaxeStat;

        switch (type)
        {
            case BonusPickaxeUpgradeType.Unlock:
                stat.unlocked = true;
                break;

            case BonusPickaxeUpgradeType.PlusCount:
                stat.count += (int)value;
                break;
            case BonusPickaxeUpgradeType.AttackRadius:
                stat.attackRadius *= value;
                break;
            case BonusPickaxeUpgradeType.RotationAngle:
                stat.rotationAnglePerSecond *= value;
                break;

            default:
                Debug.LogWarning("Invalid stat type for bonus pickaxe");
                break;
        }
    }
}
