using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class LocalizeImporter
{
    private const string CSV_PATH = "Assets/Resources/Localization/LocalizationTable.csv";
    private const string SO_FOLDER_PATH = "Assets/Resources/Localization";

    [MenuItem("Tools/Localization/Import Multi-Language CSV")]
    public static void ImportMultiLanguageCSV()
    {
        if (!File.Exists(CSV_PATH))
        {
            Debug.LogError($"CSV 파일을 찾을 수 없습니다: {CSV_PATH}");
            return;
        }

        // 1. CSV 데이터 읽기
        string[] lines = File.ReadAllLines(CSV_PATH);
        if (lines.Length < 1) return;

        // 2. 헤더 분석 (Key, KO, EN, JP...)
        string[] headers = ParseCSVLine(lines[0]);
        // 헤더 인덱스 맵 생성 (언어 이름 -> 컬럼 인덱스)
        Dictionary<string, int> languageColumnMap = new Dictionary<string, int>();
        for (int i = 1; i < headers.Length; i++) // 0번인 Key는 제외
        {
            languageColumnMap.Add(headers[i].Trim(), i);
        }

        // 3. 프로젝트 내 모든 LanguageData 에셋 로드
        string[] guids = AssetDatabase.FindAssets("t:LanguageData", new[] { SO_FOLDER_PATH });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            LanguageData langData = AssetDatabase.LoadAssetAtPath<LanguageData>(path);

            if (langData == null) continue;

            string langName = langData.language.ToString();

            // 에셋 이름과 일치하는 컬럼이 CSV에 있는지 확인
            if (!languageColumnMap.ContainsKey(langName))
            {
                Debug.LogWarning($"[Localize] CSV 헤더에 '{langName}' 컬럼이 없어 스킵합니다.");
                continue;
            }

            int targetColumnIndex = languageColumnMap[langName];
            langData.entries.Clear();

            // 4. 데이터 파싱 및 할당
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] columns = ParseCSVLine(lines[i]);
                if (columns.Length <= targetColumnIndex) continue;

                string key = columns[0].Trim();
                // \n 문자를 실제 줄바꿈으로 변경하고, 엑셀의 큰따옴표 흔적 제거
                string value = columns[targetColumnIndex].Trim().Replace("\\n", "\n").Trim('"');

                langData.entries.Add(new LocalizationEntry { key = key, value = value });
            }

            EditorUtility.SetDirty(langData);
            Debug.Log($"[Localize] {langName} 업데이트 완료: {langData.entries.Count}개 항목");
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Localization", "다국어 임포트가 완료되었습니다!", "확인");
    }

    // 큰따옴표 안의 쉼표를 무시하고 분리하는 정규식 기반 파서
    private static string[] ParseCSVLine(string line)
    {
        return Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)")
                    .Select(s => s.Replace("\"\"", "\"")) // 엑셀식 따옴표 이스케이프 처리
                    .ToArray();
    }
}
