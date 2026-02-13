using UnityEngine;
using System.IO;

public static class SaveSystem
{
    public const string SAVE_FILE_NAME = "SaveData.json";
    public const string SETTING_FILE_NAME = "SettingData.json";

    public static string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    public static string SettingFilePath => Path.Combine(Application.persistentDataPath, SETTING_FILE_NAME);

    public static void Save<T>(T data, string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(path, json);
        Debug.Log($"[{typeof(T).Name}] saved at: {path}");
    }

    public static T Load<T>(string fileName) where T : class
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (!File.Exists(path))
        {
            Debug.Log($"[{typeof(T).Name}] no file found at: {path}");
            return null;
        }

        string json = File.ReadAllText(path);
        T data = JsonUtility.FromJson<T>(json);
        Debug.Log($"[{typeof(T).Name}] loaded from: {path}");
        return data;
    }
}
