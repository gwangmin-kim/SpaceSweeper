using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DebrisSpawnData
{
    public GameObject debrisPrefab;
    public int count; // 소환할 개수
}

[CreateAssetMenu(fileName = "NewLevelDefinition", menuName = "Stage/LevelDefinition")]
public class LevelDefinition : ScriptableObject
{
    [Header("Map Settings")]
    public GameObject mapPrefab;

    [Header("Spawn Settings")]
    public List<DebrisSpawnData> debrisList;

    [Header("Gimicks (Optional)")]
    public bool enableMeteors;
    public bool enableBlackholes;
}
