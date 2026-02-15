using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager Instance { get; private set; }

    [Header("Default Data")]
    [SerializeField] DefaultGameDataSO _defaultDataTemplate;
    [SerializeField] DefaultSettingSO _defaultSettingTemplate;

    public GameData CurrentData;
    public SettingData CurrentSetting;

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

        LoadSetting();

#if UNITY_EDITOR
        CreateNewGameData();
#endif
    }

    // #if UNITY_EDITOR
    //     void Update()
    //     {
    //         if (UnityEngine.InputSystem.Keyboard.current != null)
    //         {
    //             if (UnityEngine.InputSystem.Keyboard.current.lKey.wasPressedThisFrame)
    //             {
    //                 LoadGame();
    //             }
    //         }
    //     }
    // #endif

    public void SaveGame()
    {
        SaveSystem.Save(CurrentData, SaveSystem.SAVE_FILE_NAME);
    }

    public void LoadGame()
    {
        var loadedData = SaveSystem.Load<GameData>(SaveSystem.SAVE_FILE_NAME);

        if (loadedData != null)
        {
            CurrentData = loadedData;
        }
        else
        {
            CreateNewGameData();
        }
    }

    public void SaveSetting()
    {
        SaveSystem.Save(CurrentSetting, SaveSystem.SETTING_FILE_NAME);
    }

    public void LoadSetting()
    {
        var loadedSetting = SaveSystem.Load<SettingData>(SaveSystem.SETTING_FILE_NAME);

        if (loadedSetting != null)
        {
            CurrentSetting = loadedSetting;
        }
        else
        {
            CreateNewSetting();
        }
    }

    public void CreateNewGameData()
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

    public void CreateNewSetting()
    {
        if (_defaultSettingTemplate == null)
        {
            Debug.LogError("No default setting template found");
            CurrentSetting = new SettingData();
            return;
        }

        // deep copy
        // 원본 템플릿이 변하면 안됨
        string json = JsonUtility.ToJson(_defaultSettingTemplate.data);
        CurrentSetting = JsonUtility.FromJson<SettingData>(json);
    }

    void OnApplicationQuit()
    {
        SaveGame();
        SaveSetting();
    }
}
