using UnityEngine;

public enum DebrisUpgradeType
{
    SpawnRate,
    Health,
    DropRate,
    OverloadDropRate,
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
            case DebrisUpgradeType.SpawnRate:
                spec.spawnRate *= value;
                break;
            case DebrisUpgradeType.Health:
                spec.healthRate *= value;
                break;
            case DebrisUpgradeType.DropRate:
                spec.dropRate *= value;
                break;
            case DebrisUpgradeType.OverloadDropRate:
                spec.overloadDropRate *= value;
                break;
            default:
                Debug.LogWarning("Invalid stat type for debris");
                break;
        }
    }
}
