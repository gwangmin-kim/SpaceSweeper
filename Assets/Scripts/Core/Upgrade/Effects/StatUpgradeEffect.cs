using UnityEngine;

public enum PlayerStatType
{
    MoveSpeed,
    DashSpeed,
    DashDistance,
    DashCooldown,
    BounceFactor,
    KnockbackFactor,
    KnockbackDuration,
    MagnetRange,
    OxygenAmount,
}

[CreateAssetMenu(fileName = "NewStatUpgradeEffect", menuName = "Upgrades/Effects/Stat Upgrade Effect")]
public class StatUpgradeEffect : UpgradeEffect
{

    public PlayerStatType statType;
    public float value;

    public override void Apply()
    {
        var spec = GameManager.Instance.CurrentData.playerSpec;

        switch (statType)
        {
            case PlayerStatType.MoveSpeed:
                if (spec.moveStat.speed < value)
                    spec.moveStat.speed = value;
                break;
            case PlayerStatType.DashSpeed:
                if (spec.moveStat.dashSpeed < value)
                    spec.moveStat.dashSpeed = value;
                break;
            case PlayerStatType.DashDistance:
                if (spec.moveStat.dashDistance < value)
                    spec.moveStat.dashDistance = value;
                break;
            case PlayerStatType.DashCooldown:
                if (spec.moveStat.dashCooldown > value)
                    spec.moveStat.dashCooldown = value;
                break;
            case PlayerStatType.BounceFactor:
                if (spec.moveStat.bounceFactor > value)
                    spec.moveStat.bounceFactor = value;
                break;
            case PlayerStatType.KnockbackFactor:
                if (spec.moveStat.knockbackFactor > value)
                    spec.moveStat.knockbackFactor = value;
                break;
            case PlayerStatType.KnockbackDuration:
                if (spec.moveStat.knockbackDuration > value)
                    spec.moveStat.knockbackDuration = value;
                break;
            case PlayerStatType.MagnetRange:
                if (spec.magnetRange < value)
                    spec.magnetRange = value;
                break;
            case PlayerStatType.OxygenAmount:
                if (spec.oxygenAmount < value)
                    spec.oxygenAmount = value;
                break;
            default:
                Debug.LogWarning("Unknown stat type");
                break;
        }

        GameManager.Instance.SaveGame();
    }
}
