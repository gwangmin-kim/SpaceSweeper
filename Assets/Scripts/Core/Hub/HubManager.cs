using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelState
{
    public LevelDefinition levelDefinition;
    public bool isUnlocked;
}

public class HubManager : MonoBehaviour
{
    // Singleton
    public static HubManager Instance { get; private set; }

    [Header("Contents")]
    [SerializeField] List<LevelDefinition> _levelDefinitions; // 인스펙터 확인, 수정용
    Dictionary<int, LevelState> _levelMaps; // 실제 사용용

    void Awake()
    {
        Instance = this;

        InitMap();
    }

    void InitMap()
    {
        _levelMaps = new Dictionary<int, LevelState>();

        foreach (var levelDefinition in _levelDefinitions)
        {
            var levelState = new LevelState();
            levelState.levelDefinition = levelDefinition;
            levelState.isUnlocked = false;
            _levelMaps.Add(levelDefinition.id, levelState);
        }
    }

    void Start()
    {
        InitLevelStates();
    }

    void InitLevelStates()
    {
        var lastUnlockedLevelID = GameManager.Instance.CurrentData.lastUnlockedLevel.id;

        foreach (var levelState in _levelMaps.Values)
        {
            levelState.isUnlocked = levelState.levelDefinition.id <= lastUnlockedLevelID;
        }
    }

    public void UnlockLevel(int id)
    {
        _levelMaps[id].isUnlocked = true;

        if (id > GameManager.Instance.CurrentData.lastUnlockedLevel.id)
        {
            GameManager.Instance.CurrentData.lastUnlockedLevel = _levelMaps[id].levelDefinition;
        }
    }

    public void SelectLevel(LevelDefinition levelDefinition)
    {
        var levelState = _levelMaps[levelDefinition.id];

        if (levelState.isUnlocked)
        {
            GameManager.Instance.CurrentData.currentLevel = levelState.levelDefinition;
        }
    }
}
