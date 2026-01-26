using UnityEngine;
using System.IO;

public static class SaveSystem
{
    static string _saveFileName = "SaveData.json";
    public static string SaveFilePath => Path.Combine(Application.persistentDataPath, _saveFileName);

    public static void Save(GameData data)
    {
        string json = JsonUtility.ToJson(data, true); // true는 가독성 향상 (나중에 제거)

        string path = SaveFilePath;

        File.WriteAllText(path, json);

        Debug.Log($"Data saved at: {path}");
    }

    public static GameData Load()
    {
        string path = SaveFilePath;

        if (!File.Exists(path))
        {
            Debug.Log($"No save data found at: {path}");
            return new GameData();
        }
        else
        {
            string json = File.ReadAllText(path);
            GameData data = JsonUtility.FromJson<GameData>(json);

            Debug.Log($"Data loaded from: {path}");
            return data;
        }
    }
}
