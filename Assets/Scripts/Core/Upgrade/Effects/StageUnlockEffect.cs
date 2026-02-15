using UnityEngine;

[CreateAssetMenu(fileName = "NewStageUnlockEffect", menuName = "Upgrades/Effects/Stage Unlock")]
public class StageUnlockEffect : UpgradeEffect
{
    public LevelDefinition level;

    public override void Apply()
    {
        if (HubManager.Instance != null) HubManager.Instance.UnlockLevel(level.id);
    }
}
