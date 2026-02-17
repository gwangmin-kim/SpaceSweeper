using UnityEngine;
using UnityEditor;
using System.IO;

// [System.Serializable]인 클래스를 인스펙터처럼 보여주기 위한 껍데기 SO
public class SaveDataContainer : ScriptableObject
{
    public GameData data;
}

public class SaveDataViewer : EditorWindow
{
    private SaveDataContainer _dataContainer;
    private Vector2 _scrollPosition;

    [MenuItem("Tools/Save Data Viewer")]
    public static void ShowWindow()
    {
        GetWindow<SaveDataViewer>("Save Data Viewer");
    }

    private void OnEnable()
    {
        if (_dataContainer == null)
            _dataContainer = ScriptableObject.CreateInstance<SaveDataContainer>();
    }

    private void OnDisable()
    {
        if (_dataContainer != null) DestroyImmediate(_dataContainer);
    }

    private void OnGUI()
    {
        GUILayout.Label("Save File Manager", EditorStyles.boldLabel);

        // SaveSystem에 정의된 실제 파일 이름을 참조해야 합니다.
        string savePath = Path.Combine(Application.persistentDataPath, SaveSystem.SAVE_FILE_NAME);
        EditorGUILayout.HelpBox($"Path: {savePath}", MessageType.Info);

        GUILayout.Space(5);

        // --- 상단 버튼 영역 ---
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Open Folder", GUILayout.Height(25), GUILayout.Width(100)))
        {
            if (File.Exists(savePath))
                EditorUtility.RevealInFinder(savePath);
            else
                EditorUtility.RevealInFinder(Application.persistentDataPath);
        }

        if (GUILayout.Button("Load / Refresh", GUILayout.Height(25)))
        {
            LoadSaveData(savePath);
            GUI.FocusControl(null);
        }

        GUI.enabled = (_dataContainer != null && _dataContainer.data != null);
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);

        if (GUILayout.Button("Save Changes to File", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("Warning",
                "현재 뷰어의 내용으로 세이브 파일을 덮어쓰시겠습니까?", "Yes", "Cancel"))
            {
                SaveToFile(savePath);
            }
        }

        GUI.backgroundColor = originalColor;
        GUI.enabled = true;

        GUILayout.EndHorizontal();

        GuiLine(1);

        // --- 데이터 표시 영역 (인스펙터 수정 버전) ---
        if (_dataContainer != null && _dataContainer.data != null)
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // [수정 핵심] SerializedObject를 매번 새로 생성하여 최신 상태를 반영합니다.
            SerializedObject so = new SerializedObject(_dataContainer);
            so.Update(); // 데이터 컨테이너의 값을 SerializedProperty로 강제 업데이트합니다.

            SerializedProperty dataProp = so.FindProperty("data");

            if (dataProp != null)
            {
                // 자식 필드들을 모두 펼쳐서 그립니다.
                EditorGUILayout.PropertyField(dataProp, true);

                // 인스펙터에서 수정된 값이 있다면 _dataContainer.data에 즉시 적용합니다.
                so.ApplyModifiedProperties();
            }
            else
            {
                EditorGUILayout.HelpBox("Cannot find 'data' property.", MessageType.Error);
            }

            EditorGUILayout.EndScrollView();
        }
        else
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label("Press 'Load' to inspect save data.", EditorStyles.centeredGreyMiniLabel);
            GUILayout.FlexibleSpace();
        }
    }

    private void LoadSaveData(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                // JSON 데이터가 유효한지 로그로 확인합니다.
                Debug.Log($"[SaveViewer] Raw JSON: {json}");

                GameData loadedData = JsonUtility.FromJson<GameData>(json);
                _dataContainer.data = loadedData;

                Debug.Log($"[SaveViewer] Loaded: {Path.GetFileName(path)}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Load Failed: {e.Message}");
                _dataContainer.data = null;
            }
        }
        else
        {
            Debug.LogWarning($"Save file not found: {path}");
            _dataContainer.data = null;
        }
    }

    private void SaveToFile(string path)
    {
        if (_dataContainer.data == null) return;

        try
        {
            GUI.FocusControl(null);
            string json = JsonUtility.ToJson(_dataContainer.data, true);
            File.WriteAllText(path, json);
            Debug.Log($"[SaveViewer] Saved changes to: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveViewer] Save Failed: {e.Message}");
        }
    }

    void GuiLine(int i_height = 1)
    {
        GUILayout.Space(10);
        Rect rect = EditorGUILayout.GetControlRect(false, i_height);
        rect.height = i_height;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        GUILayout.Space(10);
    }
}
