using UnityEngine;

public enum PlayerStatType
{
    // 이동
    PlusMoveSpeed,
    MoveDampingTime,

    // 대시
    UnlockDash,
    DashSpeed,
    DashDistance,
    MinusDashCooldown,

    // 넉백
    BounceFactor,
    KnockbackFactor,
    KnockbackDuration,

    // 기타
    MagnetRange,
    PlusOxygenAmount,
}

[CreateAssetMenu(fileName = "NewStatUpgradeEffect", menuName = "Upgrades/Effects/Stat Upgrade")]
public class StatUpgradeEffect : UpgradeEffect
{

    public PlayerStatType statType;
    public float value;

    public override void Apply()
    {
        var spec = GameManager.Instance.CurrentData.playerSpec;

        switch (statType)
        {
            case PlayerStatType.PlusMoveSpeed:
                spec.moveStat.speed += value;
                break;
            case PlayerStatType.MoveDampingTime:
                spec.moveStat.dampingTime *= value;
                break;
            case PlayerStatType.UnlockDash:
                spec.moveStat.isDashUnlocked = true;
                break;
            case PlayerStatType.DashSpeed:
                spec.moveStat.dashSpeedFactor *= value;
                break;
            case PlayerStatType.DashDistance:
                spec.moveStat.dashDistance *= value;
                break;
            case PlayerStatType.MinusDashCooldown:
                spec.moveStat.dashCooldown -= value;
                if (spec.moveStat.dashCooldown < 0f) spec.moveStat.dashCooldown = 0f;
                break;
            case PlayerStatType.BounceFactor:
                spec.moveStat.bounceFactor *= value;
                break;
            case PlayerStatType.KnockbackFactor:
                spec.moveStat.knockbackFactor *= value;
                break;
            case PlayerStatType.KnockbackDuration:
                spec.moveStat.knockbackDuration *= value;
                break;
            case PlayerStatType.MagnetRange:
                spec.magnetRange *= value;
                break;
            case PlayerStatType.PlusOxygenAmount:
                spec.oxygenAmount += value;
                break;
            default:
                Debug.LogWarning("Unknown stat type");
                break;
        }

        GameManager.Instance.SaveGame();
    }
}
