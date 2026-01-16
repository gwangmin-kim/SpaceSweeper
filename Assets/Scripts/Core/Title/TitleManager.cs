using UnityEngine;

public class TitleManager : MonoBehaviour
{
    public void OnNewGameButton()
    {

    }

    public void OnContinueButton()
    {

    }

    public void OnSettingButton()
    {

    }

    public void OnExitButton()
    {
        // 에디터에서 실행 중인 경우 플레이 모드 종료
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 실제 빌드된 게임인 경우 애플리케이션 종료
        Application.Quit();
#endif
    }
}
