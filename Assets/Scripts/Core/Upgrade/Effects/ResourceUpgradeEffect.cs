using UnityEngine;

public enum ResourceUpgradeType
{
    SetExchangeInterval,
    ExchangeAmount,
    GoldPerResource,
    SetLossRatio,
}

[CreateAssetMenu(fileName = "NewResourceUpgradeEffect", menuName = "Upgrades/Effects/Resource Upgrade")]
public class ResourceUpgradeEffect : UpgradeEffect
{
    public ResourceUpgradeType type;
    public float value;

    public override void Apply()
    {
        var spec = GameManager.Instance.CurrentData.resourceSpec;

        switch (type)
        {
            case ResourceUpgradeType.SetExchangeInterval:
                if (spec.exchangeInterval > value)
                    spec.exchangeInterval = value;
                break;
            case ResourceUpgradeType.ExchangeAmount:
                spec.exchangeAmount *= value;
                break;
            case ResourceUpgradeType.GoldPerResource:
                spec.goldPerResource *= value;
                break;
            case ResourceUpgradeType.SetLossRatio:
                if (spec.lossRatio > value)
                    spec.lossRatio = value;
                break;
            default:
                Debug.LogWarning("Invalid stat type for resource");
                break;
        }
    }
}
