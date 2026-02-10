// using System.Collections.Generic;
using UnityEngine;
using BreakInfinity;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Upgrades/Upgrade Definition")]
public class UpgradeDefinition : ScriptableObject
{
    [Header("ID (Auto Generated)")]
    [SerializeField, TextArea(1, 1)]
    private string _id;
    public string ID
    {
        get
        {
#if UNITY_EDITOR
            // 런타임이 아닐 때 ID가 비어있으면 갱신 시도
            if (string.IsNullOrEmpty(_id))
                _id = UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(this));
#endif
            return _id;
        }
    }

    [Header("Behavior")]
    public List<UpgradeEffect> effects;

    [Header("Display")]
    public string upgradeName;
    [TextArea]
    public string description;
    public Sprite icon;

    [Header("Setting")]
    // 단순 해금 방식 (레벨 개념 없음)
    public string costString;
    public BigDouble Cost => BigDoubleFormatter.Parse(costString);

    [Header("Structure")]
    public UpgradeDefinition parent;
    public int Depth => parent == null ? 0 : parent.Depth + 1;

    void OnValidate()
    {
#if UNITY_EDITOR
        // ID 자동 동기화
        string path = UnityEditor.AssetDatabase.GetAssetPath(this);
        string guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
        if (_id != guid) _id = guid;

        // 순환 참조 방지
        if (parent == this) parent = null;
        else if (parent != null)
        {
            UpgradeDefinition check = parent;
            int safetyCount = 0;
            while (check != null && safetyCount < 100)
            {
                if (check == this)
                {
                    Debug.LogError($"[UpgradeDef] {name}: detected circular reference (Loop with {parent.name})");
                    parent = null;
                    break;
                }
                check = check.parent;
                safetyCount++;
            }
        }

        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
