using UnityEngine;

public enum DebrisUpgradeType
{
    SpawnCount,
    Health,
    DropCount,
    OverloadDropInc,

    Size,
}

[CreateAssetMenu(fileName = "NewDebrisUpgradeEffect", menuName = "Upgrades/Effects/Debris Upgrade Effect")]
public class DebrisUpgradeEffect : UpgradeEffect
{
    public DebrisUpgradeType type;
    public float value;

    public override void Apply()
    {
        var spec = GameManager.Instance.CurrentData.debrisSpec;

        switch (type)
        {
            case DebrisUpgradeType.SpawnCount:
                spec.spawnRate *= value;
                break;
            case DebrisUpgradeType.Health:
                spec.healthRate *= value;
                break;
            case DebrisUpgradeType.DropCount:
                spec.dropRate *= value;
                break;
            case DebrisUpgradeType.OverloadDropInc:
                spec.overloadDropRate *= value;
                break;
            case DebrisUpgradeType.Size:
                spec.sizeRate *= value;
                break;
            default:
                Debug.LogWarning("Invalid stat type for debris");
                break;
        }
    }
}
