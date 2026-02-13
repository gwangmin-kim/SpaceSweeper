using UnityEngine;

[CreateAssetMenu(fileName = "DefaultSetting", menuName = "Game/Default Setting Data")]
public class DefaultSettingSO : ScriptableObject
{
    // 인스펙터에서 수정할 기본 데이터
    public SettingData data;
}
