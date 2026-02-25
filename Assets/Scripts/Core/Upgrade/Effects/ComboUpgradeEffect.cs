using UnityEngine;

public enum ComboUpgradeType
{
    Unlock = 0,

    PlusHoldTime = 10,

    SetMoveSpeedBouns = 20,
    SetAttackSpeedBonus = 21,
    SetPickaxeRangeBonus = 30,
}

[CreateAssetMenu(fileName = "NewComboUpgradeEffect", menuName = "Upgrades/Effects/Combo Upgrade")]
public class ComboUpgradeEffect : UpgradeEffect
{
    public ComboUpgradeType type;
    public float value;

    public override void Apply()
    {
        var spec = GameManager.Instance.CurrentData.playerSpec.comboSpec;

        switch (type)
        {
            case ComboUpgradeType.Unlock:
                spec.isUnlocked = true;
                break;

            case ComboUpgradeType.PlusHoldTime:
                spec.holdTime += value;
                break;

            case ComboUpgradeType.SetMoveSpeedBouns:
                spec.moveSpeedBonus = value;
                break;
            case ComboUpgradeType.SetAttackSpeedBonus:
                spec.attackSpeedBonus = value;
                break;
            case ComboUpgradeType.SetPickaxeRangeBonus:
                spec.pickaxeRangeBonus = value;
                break;

            default:
                Debug.LogWarning("Invalid stat type for combo");
                break;
        }
    }
}
