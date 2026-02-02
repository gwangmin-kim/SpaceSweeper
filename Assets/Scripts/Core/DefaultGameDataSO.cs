using UnityEngine;

[CreateAssetMenu(fileName = "DefaultGameData", menuName = "Game/Default Game Data")]
public class DefaultGameDataSO : ScriptableObject
{
    // 인스펙터에서 수정할 기본 데이터
    public GameData data;
}
