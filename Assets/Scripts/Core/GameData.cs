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
}

[System.Serializable]
public class ResourceSpec
{
    // 교환
    public float exchangeInterval;
    public BigDouble exchangeAmount;
    public BigDouble goldPerResource;
    // 사망 시 자원을 잃어버리는 비율 (1.0일 경우 100% 손실)
    public float lossRatio;
}

[System.Serializable]
public class DebrisSpec
{
    public float spawnRate;
    public float healthRate;
    public float dropRate;
    public float overloadDropRate;
}

[System.Serializable]
public class GimickSpec
{
    public MeteorData meteorData;
    public BlackholeData blackholeData;
    public MagneticStormData magneticStormData;
}

[System.Serializable]
public class GameData
{
    // resource
    public BigDouble resource;
    public BigDouble gold;

    // stage
    public LevelDefinition currentLevel;
    public LevelDefinition lastUnlockedLevel;

    // upgrade
    public List<string> unlockedUpgrades;
    public PlayerSpec playerSpec;
    public ResourceSpec resourceSpec;
    public DebrisSpec debrisSpec;
    public GimickSpec gimickSpec;

    // setting

}
