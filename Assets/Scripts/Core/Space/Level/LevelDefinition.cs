using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ResourceSpawnData
{
    public GameObject resourcePrefab;
    public int count; // 소환할 개수
}

[System.Serializable]
public struct DebrisSpawnData
{
    public GameObject debrisPrefab;
    public int count; // 소환할 개수
}

[System.Serializable]
public struct StageGimick
{
    public bool isEnabled;
    public GameObject GimickPrefab;
    public float minInterval;
    public float maxInterval;
    public float probability;
}

[CreateAssetMenu(fileName = "NewLevelDefinition", menuName = "Stage/LevelDefinition")]
public class LevelDefinition : ScriptableObject
{
    [Header("Map Settings")]
    public GameObject mapPrefab;

    [Header("Spawn Settings")]
    public ResourceSpawnData resourceSpawnData;
    public List<DebrisSpawnData> debrisList;

    [Header("Gimicks (Optional)")]
    public List<StageGimick> gimickList;
}
