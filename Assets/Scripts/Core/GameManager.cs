using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager Instance { get; private set; }

    [Header("Default Data")]
    [SerializeField] DefaultGameDataSO _defaultDataTemplate;

    public GameData CurrentData;

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
#if UNITY_EDITOR
        CreateNewGameData();
#endif
    }

#if UNITY_EDITOR
    void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (UnityEngine.InputSystem.Keyboard.current.lKey.wasPressedThisFrame)
            {
                LoadGame();
            }
        }
    }
#endif

    public void SaveGame()
    {
        SaveSystem.Save(CurrentData);
    }

    public void LoadGame()
    {
        var loadedData = SaveSystem.Load();

        if (loadedData != null)
        {
            CurrentData = loadedData;
        }
        else
        {
            CreateNewGameData();
        }
    }

    void CreateNewGameData()
    {
        if (_defaultDataTemplate == null)
        {
            Debug.LogError("No default data template found");
            CurrentData = new GameData();
            return;
        }

        // deep copy
        // 원본 템플릿이 변하면 안됨
        string json = JsonUtility.ToJson(_defaultDataTemplate.data);
        CurrentData = JsonUtility.FromJson<GameData>(json);
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }
}
