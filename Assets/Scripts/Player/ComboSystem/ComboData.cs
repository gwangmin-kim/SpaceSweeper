using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ComboLevelData
{
    public string gradeName;
    public float threshold; // 다음 단계로 넘어가기 위한 누적 점수
    public float decayRate; // 초당 감소하는 점수 (시간 제한 대신 점수 감소 방식)
    public float minScoreRequired; // 해당 단계를 유지하기 위한 최소 점수
}

[CreateAssetMenu(fileName = "ComboData", menuName = "Game/Combo Data")]
public class ComboData : ScriptableObject
{
    public List<ComboLevelData> levelList;
    public float holdTime; // 콤보가 갱신되었을 때 점수가 떨어지지 않는 시간 유예
}
