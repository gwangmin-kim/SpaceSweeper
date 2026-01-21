// using System.Collections.Generic;
using UnityEngine;
using BreakInfinity;

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Upgrades/Upgrade Definition", order = 0)]
public class UpgradeDefinition : ScriptableObject
{
    [Header("ID")]
    public string id;

    [Header("Behavior")]
    public UpgradeEffect effect;

    [Header("Display")]
    public string upgradeName;
    public string description;
    public Sprite icon;

    [Header("Setting")]
    // 단순 해금 방식 (레벨 개념 없음)
    public string costString;
    public BigDouble Cost => BigDoubleFormatter.Parse(costString);

    [Header("Structure")]
    public UpgradeDefinition parent;
}
