using UnityEngine;
using UnityEditor;
using System.IO;

// [System.Serializable]인 클래스를 인스펙터처럼 보여주기 위한 껍데기 SO
public class SaveDataContainer : ScriptableObject
{
    // 실제 게임 데이터 데이터
    public GameData data;
}

public class SaveDataViewer : EditorWindow
{
    private SaveDataContainer _dataContainer;
    private Editor _editor;
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
        // 윈도우 닫을 때 임시 객체 정리
        if (_editor != null) DestroyImmediate(_editor);
        if (_dataContainer != null) DestroyImmediate(_dataContainer);
    }

    private void OnGUI()
    {
        GUILayout.Label("Save File Manager", EditorStyles.boldLabel);

        string savePath = Path.Combine(Application.persistentDataPath, SaveSystem.SAVE_FILE_NAME);
        EditorGUILayout.HelpBox($"Path: {savePath}", MessageType.Info);

        GUILayout.Space(5);

        // --- 상단 버튼 영역 ---
        GUILayout.BeginHorizontal();

        // 1. 폴더 열기
        if (GUILayout.Button("Open Folder", GUILayout.Height(25), GUILayout.Width(100)))
        {
            if (File.Exists(savePath))
                EditorUtility.RevealInFinder(savePath);
            else
                EditorUtility.RevealInFinder(Application.persistentDataPath);
        }

        // 2. 불러오기 (새로고침)
        if (GUILayout.Button("Load / Refresh", GUILayout.Height(25)))
        {
            LoadSaveData(savePath);
            GUI.FocusControl(null); // 입력 필드 포커스 해제 (값 적용 확실히 하기 위해)
        }

        // 3. [추가됨] 저장하기 버튼
        // 데이터가 로드된 상태에서만 활성화
        GUI.enabled = (_dataContainer != null && _dataContainer.data != null);
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(1f, 0.6f, 0.6f); // 붉은색 계열로 강조 (주의 표시)

        if (GUILayout.Button("Save Changes to File", GUILayout.Height(25)))
        {
            // 실수 방지용 확인 팝업
            if (EditorUtility.DisplayDialog("Warning",
                "현재 뷰어의 내용으로 세이브 파일을 덮어쓰시겠습니까?\n(이 작업은 되돌릴 수 없습니다.)",
                "Yes, Overwrite", "Cancel"))
            {
                SaveToFile(savePath);
            }
        }

        GUI.backgroundColor = originalColor;
        GUI.enabled = true; // GUI 활성 상태 복구

        GUILayout.EndHorizontal();
        // -----------------------

        GuiLine(1); // 구분선

        // --- 데이터 표시 영역 (인스펙터) ---
        if (_dataContainer != null && _dataContainer.data != null)
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // 중요: 인스펙터 그리기 (여기서 값을 수정하면 _dataContainer.data가 즉시 변경됨)
            if (_editor == null)
                _editor = Editor.CreateEditor(_dataContainer);

            // "Data" 필드 하위만 바로 보여주기 위해 약간의 트릭 사용
            SerializedObject so = new SerializedObject(_dataContainer);
            SerializedProperty dataProp = so.FindProperty("data");
            EditorGUILayout.PropertyField(dataProp, true); // true는 자식까지 펼쳐서 보여준다는 의미
            so.ApplyModifiedProperties(); // 변경 사항 적용

            EditorGUILayout.EndScrollView();
        }
        else
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label("Press 'Load' to inspect save data.", EditorStyles.centeredGreyMiniLabel);
            GUILayout.FlexibleSpace();
        }
    }

    // --- 파일 로드 로직 ---
    private void LoadSaveData(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);

                // [중요] 실제 게임의 SaveSystem에서 사용하는 방식과 동일하게 파싱해야 합니다.
                // 만약 Newtonsoft.Json을 사용한다면:
                // GameData loadedData = Newtonsoft.Json.JsonConvert.DeserializeObject<GameData>(json);
                GameData loadedData = JsonUtility.FromJson<GameData>(json);

                _dataContainer.data = loadedData;

                // 에디터 갱신
                if (_editor != null) DestroyImmediate(_editor);
                // OnGUI에서 새로 생성됨

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

    // --- [추가됨] 파일 저장 로직 ---
    private void SaveToFile(string path)
    {
        if (_dataContainer.data == null) return;

        try
        {
            GUI.FocusControl(null); // 혹시 입력 중인 필드가 있다면 반영

            // 1. 현재 데이터를 JSON으로 직렬화
            // [중요] 실제 게임의 SaveSystem과 동일한 직렬화 방식을 사용하세요!
            // 예: string json = Newtonsoft.Json.JsonConvert.SerializeObject(_dataContainer.data, Newtonsoft.Json.Formatting.Indented);
            string json = JsonUtility.ToJson(_dataContainer.data, true); // true: 보기 좋게 들여쓰기

            // 2. 파일 쓰기
            File.WriteAllText(path, json);

            Debug.Log($"[SaveViewer] Saved changes to: {path}");
            // 성공 알림 (선택 사항)
            // EditorUtility.DisplayDialog("Success", "File saved successfully!", "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveViewer] Save Failed: {e.Message}");
            EditorUtility.DisplayDialog("Error", $"Failed to save:\n{e.Message}", "OK");
        }
    }

    // 유틸: 구분선 그리기
    void GuiLine(int i_height = 1)
    {
        GUILayout.Space(10);
        Rect rect = EditorGUILayout.GetControlRect(false, i_height);
        rect.height = i_height;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        GUILayout.Space(10);
    }
}
