using UnityEngine;

public enum DebrisUpgradeType
{
    // 기본 스탯: 0-9
    Health = 0,
    Size = 1,

    // 스폰 및 드롭: 10-19
    SpawnCount = 10,
    DropValue = 11,
    OverloadDropInc = 12,

    // 보너스 효과: 20-29
    PlusOxygenRestoreAmount = 20,
    SetOxygenRestoreChance = 21,
}

[CreateAssetMenu(fileName = "NewDebrisUpgradeEffect", menuName = "Upgrades/Effects/Debris Upgrade")]
public class DebrisUpgradeEffect : UpgradeEffect
{
    public DebrisUpgradeType type;
    public float value;

    public override void Apply()
    {
        var spec = GameManager.Instance.CurrentData.debrisSpec;

        switch (type)
        {
            case DebrisUpgradeType.Health:
                spec.healthRate *= value;
                break;
            case DebrisUpgradeType.Size:
                spec.sizeRate *= value;
                break;

            case DebrisUpgradeType.SpawnCount:
                spec.spawnRate *= value;
                break;
            case DebrisUpgradeType.DropValue:
                spec.valueRate *= value;
                break;
            case DebrisUpgradeType.OverloadDropInc:
                spec.overloadDropRate *= value;
                break;

            case DebrisUpgradeType.PlusOxygenRestoreAmount:
                spec.oxygenRestoreAmount += value;
                break;
            case DebrisUpgradeType.SetOxygenRestoreChance:
                spec.oxygenRestoreChance = value;
                break;

            default:
                Debug.LogWarning("Invalid stat type for debris");
                break;
        }
    }
}
