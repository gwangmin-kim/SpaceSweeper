using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager Instance { get; private set; }

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
    }

    public void SaveGame()
    {
        SaveSystem.Save(CurrentData);
    }

    public void LoadGame()
    {
        CurrentData = SaveSystem.Load();
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }
}
