using UnityEngine;

public class TitleManager : MonoBehaviour
{
    public void OnNewGameButton()
    {
        GameManager.Instance.CreateNewGameData();
        // 새 게임으로 세이브데이터 전환
        //? 기존 값이 존재한다면 설정값만은 불러와서 저장하는 기능 추가하기
        GameManager.Instance.SaveGame();
        SceneLoader.LoadScene("Hub");
    }

    public void OnContinueButton()
    {
        GameManager.Instance.LoadGame();
        SceneLoader.LoadScene("Hub");
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
