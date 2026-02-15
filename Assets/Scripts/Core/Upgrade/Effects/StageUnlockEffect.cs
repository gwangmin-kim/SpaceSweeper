using UnityEngine;

[CreateAssetMenu(fileName = "NewStageUnlockEffect", menuName = "Upgrades/Effects/Stage Unlock")]
public class StageUnlockEffect : UpgradeEffect
{
    public LevelDefinition level;

    public override void Apply()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.UnlockLevel(level.id);
    }
}
