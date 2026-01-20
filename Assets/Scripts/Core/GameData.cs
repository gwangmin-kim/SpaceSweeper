using System.Collections.Generic;
using BreakInfinity;

// [System.Serializable]
// public class UpgradeData
// {
//     public string id;
//     public int level;

//     public UpgradeData(string id, int level)
//     {
//         this.id = id;
//         this.level = level;
//     }
// }

[System.Serializable]
public class GameData
{
    // resource
    public BigDouble resource;
    public BigDouble gold;

    // upgrade
    // 용도를 고려하면 HashSet이 더 적합하지만, HashSet은 json 형식으로 저장하기 어려움
    public List<string> unlockedUpgrades;
    // public List<UpgradeData> upgradeStates;

    // setting

    public GameData()
    {
        resource = 0;
        gold = 0;

        unlockedUpgrades = new List<string>();
        // upgradeStates = new List<UpgradeData>();
    }
}
