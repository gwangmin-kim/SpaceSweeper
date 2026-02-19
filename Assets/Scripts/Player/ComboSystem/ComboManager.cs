using System;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    // singleton
    public static ComboManager Instance { get; private set; }

    [SerializeField] ComboData _comboData;

    [SerializeField] float _currentScore = 0f;
    [SerializeField] int _currentLevelIndex = 0;
    public ComboLevelData CurrentLevel => _comboData.levelList[_currentLevelIndex];

    float _holdTimer = 0f;

    public event Action<int, float> OnComboChanged; // 레벨 인덱스, 현재 점수 비율 전달


    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (_currentScore > 0)
        {
            // 1. 시간에 따른 점수 감소 (유지 요구치 충족 체크용)
            var currentLevel = CurrentLevel;

            if (_holdTimer > 0)
            {
                _holdTimer -= Time.deltaTime;
            }
            else
            {
                _currentScore -= currentLevel.decayRate * Time.deltaTime;
            }

            // 2. 최소 점수 미달 시 콤보 초기화 또는 강등
            if (_currentScore < currentLevel.minScoreRequired)
            {
                ResetCombo();
            }

            OnComboChanged?.Invoke(_currentLevelIndex, GetLevelProgress());
        }
    }

    void CheckLevelUp()
    {
        // 다음 단계가 있고, 점수가 임계치를 넘었다면 레벨업
        if (_currentLevelIndex < _comboData.levelList.Count - 1)
        {
            if (_currentScore >= CurrentLevel.threshold)
            {
                _currentLevelIndex++;
                // Debug.Log($"Combo Up! Current Grade: {CurrentLevel.gradeName}");
            }
        }
    }

    void ResetCombo()
    {
        _currentScore = 0f;
        _currentLevelIndex = 0;
        _holdTimer = 0f;
    }

    public void AddScore(float amount)
    {
        _currentScore += amount;

        var comboSpec = GameManager.Instance.CurrentData.playerSpec.comboSpec;
        _holdTimer = comboSpec.holdTime;

        CheckLevelUp();
    }

    public string GetLevelString(int index)
    {
        if (index < 0 || index >= _comboData.levelList.Count) return null;

        return _comboData.levelList[index].gradeName;
    }

    public float GetLevelProgress()
    {
        // 현재 레벨 내에서의 진행도 (UI 게이지용)
        float prevThreshold = (_currentLevelIndex > 0) ? _comboData.levelList[_currentLevelIndex - 1].threshold : 0f;
        float ratio = (_currentScore - prevThreshold) / (CurrentLevel.threshold - prevThreshold);

        return Mathf.Clamp01(ratio);
    }
}
