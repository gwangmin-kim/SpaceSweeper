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
    public float oxygenLossRatio; // 피격 시 잃는 산소 비율

    [Header("Combat: Combo")]
    public ComboSpec comboSpec;

    [Header("Weapon")]
    public WeaponType currentWeapon;
    public PickaxeStat pickaxeStat;
    public ShotgunStat shotgunStat;
    public LasergunStat lasergunStat;

    public string GetAttackDamageString()
    {
        return currentWeapon switch
        {
            WeaponType.Pickaxe => pickaxeStat.attackDamage.ToString(),
            WeaponType.Shotgun => $"{shotgunStat.bulletData.damage}×{shotgunStat.bulletCount}",
            WeaponType.Lasergun => lasergunStat.damage.ToString(),
            _ => "0"
        };
    }

    public string GetAttackSpeedString()
    {
        return currentWeapon switch
        {
            WeaponType.Pickaxe => pickaxeStat.attackSpeed.ToString(),
            WeaponType.Shotgun => shotgunStat.attackSpeed.ToString(),
            WeaponType.Lasergun => (1f / lasergunStat.attackSpeed).ToString(),
            _ => "0"
        };
    }

    public string GetAttackRangeString()
    {
        return currentWeapon switch
        {
            WeaponType.Pickaxe => pickaxeStat.attackRange.ToString(),
            WeaponType.Shotgun => (shotgunStat.bulletData.speed * shotgunStat.bulletData.duration).ToString(),
            WeaponType.Lasergun => lasergunStat.range.ToString(),
            _ => "0"
        };
    }
}

[System.Serializable]
public class ComboSpec
{
    public bool isUnlocked;
    public float holdTime;
    public float scoreRate;

    public float moveSpeedBonus;
    public float attackSpeedBonus;

    public float pickaxeRangeBonus;
    public float shotgunBulletBonus;
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

    public float overloadChance;
    public float overloadDropRate;
    public float overloadDamgeRate; // 최대 체력 비례 대미지, 예: 5.0이라면 최대 체력의 5배 대미지
    public float overloadExplodeRange;

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
    public bool hasUnlockedNextLevel; // 2스테이지 해금 (맵 변경 안내)
}

[System.Serializable]
public class RecordData
{
    public BigDouble bestLootAmount; // 단일 세션 최고 획득량
    public BigDouble bestExplorationTime; // 단일 세션 최장 탐사 시간
    public BigDouble bestDamageInflicted; // 단일 세션 최고 가한 피해량
}

[System.Serializable]
public class GameData
{
    // resource
    public BigDouble resource;
    public BigDouble gold;

    // stage id
    public int currentLevelID;
    public int lastUnlockedLevelID;

    // upgrade
    public List<string> unlockedUpgrades;
    public PlayerSpec playerSpec;
    public ResourceSpec resourceSpec;
    public DebrisSpec debrisSpec;
    public GimickSpec gimickSpec;

    // event history
    public EventData eventData;
}
