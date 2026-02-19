using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelState
{
    public LevelDefinition levelDefinition;
    public bool isUnlocked;
}

public class LevelManager : MonoBehaviour
{
    // Singleton
    public static LevelManager Instance { get; private set; }

    [Header("Contents")]
    [SerializeField] List<LevelDefinition> _levelDefinitions; // 인스펙터 확인, 수정용
    Dictionary<int, LevelState> _levelMaps; // 실제 사용용

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InitMap();
    }

    void InitMap()
    {
        _levelMaps = new Dictionary<int, LevelState>();

        foreach (var levelDefinition in _levelDefinitions)
        {
            var levelState = new LevelState
            {
                levelDefinition = levelDefinition,
                isUnlocked = false
            };
            _levelMaps.Add(levelDefinition.id, levelState);
        }
    }

    void Start()
    {
        InitLevelStates();
    }

    void InitLevelStates()
    {
        var lastUnlockedLevelID = GameManager.Instance.CurrentData.lastUnlockedLevelID;

        foreach (var levelState in _levelMaps.Values)
        {
            levelState.isUnlocked = levelState.levelDefinition.id <= lastUnlockedLevelID;
        }
    }

    public void UnlockLevel(int id)
    {
        if (!_levelMaps.ContainsKey(id)) return;

        _levelMaps[id].isUnlocked = true;

        if (id > GameManager.Instance.CurrentData.lastUnlockedLevelID)
        {
            GameManager.Instance.CurrentData.lastUnlockedLevelID = id;
        }
    }

    public void SelectLevel(int id)
    {
        if (!_levelMaps.ContainsKey(id)) return;
        var levelState = _levelMaps[id];

        if (!levelState.isUnlocked)
        {
            Debug.LogWarning($"{levelState.levelDefinition.name} is not unlocked");
        }
        GameManager.Instance.CurrentData.currentLevelID = levelState.levelDefinition.id;
    }

    public void SelectLevel(LevelDefinition levelDefinition)
    {
        SelectLevel(levelDefinition.id);
    }

    public LevelDefinition GetLevel(int id)
    {
        if (!_levelMaps.ContainsKey(id) || !_levelMaps[id].isUnlocked) return null;
        return _levelMaps[id].levelDefinition;
    }
}
