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
    public float sizeRate;
    public float healthRate;
    public float valueRate;
    public float overloadDropRate;

    public float oxygenRestoreAmount; // 파괴 시 일정 확률로 충전되는 산소량
    public float oxygenRestoreChance; // 산소 재충전 확률
}

[System.Serializable]
public class GimickSpec
{
    public MeteorData meteorData;
    public BlackholeData blackholeData;
    public MagneticStormData magneticStormData;
}

[System.Serializable]
public class EventData
{
    public bool hasUnlockedOnce; // 최초 업그레이드 안내
    public bool hasExploredOnce; // 최초 탐사 (자원 획득 안내)
    public bool hasExchangedOnce; // 최초 자원 교환 안내
    public bool hasUsedWeaponOnce; // 최초 무기 획득 후 탐사 (공격 안내)
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

    // event history
    public EventData eventData;
}
