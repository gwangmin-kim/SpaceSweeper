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
    public float lossRatio; // 사망 시 자원을 잃어버리는 비율 (1.0일 경우 100% 손실)
}

[System.Serializable]
public class GameData
{
    // resource
    public BigDouble resource;
    public BigDouble gold;

    // upgrade
    // 용도를 고려하면 HashSet이 더 적합하지만, HashSet은 json 형식으로 저장하기 어려움
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
