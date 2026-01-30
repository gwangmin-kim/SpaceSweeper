using System.Collections.Generic;
using UnityEngine;
using BreakInfinity;

public enum WeaponType
{
    None,
    Pickaxe,
    Shotgun,
    Lasergun,
}

[System.Serializable]
public class PlayerSpec
{
    [Header("Player")]
    public PlayerMoveStat moveStat;
    public float magnetRange;
    public float oxygenAmount;

    [Header("Weapon")]
    public WeaponType currentWeapon;
    public PickaxeStat pickaxeStat;
    public ShotgunStat shotgunStat;
    public LasergunStat lasergunStat;

    [Header("Resource")]
    public float resourceLossRatio; // 사망 시 자원을 잃어버리는 비율 (1.0일 경우 100% 손실)
}

[System.Serializable]
public class GimickSpec
{
    public MeteorData meteorData;
    public BlackholeData blackholeData;
}

[System.Serializable]
public class GameData
{
    // resource
    public BigDouble resource;
    public BigDouble gold;

    // stage
    public LevelDefinition currentLevel;
    public GimickSpec gimickSpec;

    // upgrade
    public List<string> unlockedUpgrades;

    // setting

    // player specification
    public PlayerSpec playerSpec;

    public GameData()
    {
        resource = 0;
        gold = 0;

        unlockedUpgrades = new List<string>();
    }
}
