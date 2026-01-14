using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] Slider _progressBar;
    [SerializeField] TextMeshProUGUI _loadingText;

    [Header("Settings")]
    [SerializeField] float _minLoadingTime; // 로딩이 너무 빨리 끝나도 최소한 이 시간만큼은 보여줌 (연출용)

    void Start()
    {
        // 로딩 프로세스 시작
        StartCoroutine(Co_LoadSceneProcess());
    }

    IEnumerator Co_LoadSceneProcess()
    {
        // 1. SceneLoader에 저장된 목표 씬을 비동기로 로드 시작
        string targetScene = SceneLoader.TargetSceneName;
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(targetScene);

        // 2. 로딩이 다 되어도 바로 넘어가지 않게 막음
        loadOperation.allowSceneActivation = false;

        float timer = 0f;

        // 3. 로딩 진행 루프
        while (!loadOperation.isDone)
        {
            yield return null; // 1 프레임 대기
            timer += Time.deltaTime;

            // Unity의 비동기 로딩은 0.9에서 멈추고 activation을 기다림
            // op.progress : 0.0 ~ 0.9

            // 연출을 위해 가짜 진행률 계산 (실제 로딩과 최소 시간 중 더 느린 쪽을 따라감)
            if (loadOperation.progress < 0.9f)
            {
                // 로딩 중: 바를 천천히 채움
                _progressBar.value = Mathf.Lerp(_progressBar.value, loadOperation.progress, timer);
            }
            else
            {
                // 로딩은 끝났음 (0.9 도달)
                // 하지만 최소 시간(_minLoadingTime)은 채웠는지 확인
                _progressBar.value = Mathf.Lerp(_progressBar.value, 1f, timer);

                if (_progressBar.value >= 0.99f && timer >= _minLoadingTime)
                {
                    // 모든 조건 충족 시 씬 전환
                    loadOperation.allowSceneActivation = true;
                }
            }
        }
    }
}
