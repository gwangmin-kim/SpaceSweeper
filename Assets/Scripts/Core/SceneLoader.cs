using UnityEngine.SceneManagement;

public static class SceneLoader
{
    // 다음에 로드할 씬의 이름을 저장
    public static string TargetSceneName { get; private set; }

    public static void LoadScene(string sceneName)
    {
        TargetSceneName = sceneName;
        // 즉시 로딩 전용 씬 호출
        SceneManager.LoadScene("Loading");
    }
}
