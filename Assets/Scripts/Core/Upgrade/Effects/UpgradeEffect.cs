using System.Collections.Generic;
using UnityEngine;

public abstract class UpgradeEffect : ScriptableObject
{
    public List<StatType> affectedStatList;
    public abstract void Apply();
}
