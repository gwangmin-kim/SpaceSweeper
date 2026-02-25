using UnityEngine;
using BreakInfinity;

[System.Serializable]
public class SessionInformation
{
    public bool isOngoing;
    public bool isSuccessful; // 산소 고갈 전에 복귀했는지
    public float timer;
    public BigDouble lootAmount;
    public BigDouble lossAmount;
    public float oxygenLost; // 잃은 산소량
    public float oxygenRestored; // 복구한 산소량
    public float damageDealt; // 폐기물에 가한 피해량 (플레이어가 직접)
    public float criticalDamageDealt; // 폐기물에 가한 치명타 피해량 (플레이어가 직접)
    public float damageByDebris; // 폭발로 인한 피해량
    public int destroyCount; // 파괴된 폐기물 수
}

public class SessionManager : MonoBehaviour
{
    // Singleton
    public static SessionManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] SpacePlayerController _player;

    [Header("Spaceship Return")]
    [SerializeField] SpaceshipController _returnArea;

    [Header("Session Information")]
    [SerializeField] SessionInformation _info;

    public SessionInformation Information => _info;

    // Oxygen
    float _totalOxygenAmountInverse = 0f; // 산소 총량의 역수 (연산 효율성 위해 역수로 저장)
    float _currentOxygenAmount = 0f; // 현재 산소량
    float _oxygenPerSec = 1f; // 초당 소비 산소량
    public void SetOxygenConsumption(float amount)
    {
        _oxygenPerSec = amount;
    }

    public void Awake()
    {
        Instance = this;
        _info = new SessionInformation();
    }

    void Start()
    {
        InitSession();
    }

    void Update()
    {
        if (_info.isOngoing && _currentOxygenAmount > 0f)
        {
            _currentOxygenAmount -= Time.deltaTime * _oxygenPerSec;
            _info.timer += Time.deltaTime;

            float oxygenRatio = Mathf.Clamp01(_currentOxygenAmount * _totalOxygenAmountInverse);

            SessionUIController.Instance.SetOxygen(_currentOxygenAmount, oxygenRatio);
        }
        else
        {
            SessionUIController.Instance.SetOxygen(0f, 0f);
            // 종료 시점에 우주선과 닿아있으면 세이프
            if (!TryReturn()) EndSession(false);
        }
    }

    void InitSession()
    {
        float totalOxygenAmount = GameManager.Instance.CurrentData.playerSpec.oxygenAmount;
        _currentOxygenAmount = totalOxygenAmount;
        _totalOxygenAmountInverse = 1f / totalOxygenAmount;

        _info.lootAmount = 0;
        _info.timer = 0f;
        _info.isOngoing = true;

        SessionUIController.Instance.SetOxygen(_currentOxygenAmount, 1f);
        SessionUIController.Instance.SetResource(_info.lootAmount);

        InGameRoutineManager.Instance.LoadLevel();

        SpacePlayerController.Instance.enabled = true;
    }

    void EndSession(bool isSuccessful)
    {
        if (!_info.isOngoing) return;

        _info.isSuccessful = isSuccessful;

        if (isSuccessful)
        {
            // 플레이어 자발적 귀환: 성공


        }
        else
        {
            // 산소 고갈: 강제 종료
            // 자원 손실
            float resourceLossRatio = GameManager.Instance.CurrentData.resourceSpec.lossRatio;

            _info.lossAmount = _info.lootAmount * resourceLossRatio;
            // _info.lootAmount -= _info.lossAmount;
            LootResource(-_info.lossAmount);
        }

        _info.isOngoing = false;

        if (ResourceManager.Instance != null) ResourceManager.Instance.StoreResources(_info.lootAmount);

        SessionUIController.Instance.SessionSummary(_info);

        InGameRoutineManager.Instance.StopAllGimickRoutines();

        SpacePlayerController.Instance.enabled = false;
    }

    public void ReceiveDamage()
    {
        if (!_info.isOngoing) return;

        // 플레이어가 공격 받아 산소를 잃음
        var playerSpec = GameManager.Instance.CurrentData.playerSpec;
        float amount = playerSpec.oxygenLossRatio * playerSpec.oxygenAmount;

        _currentOxygenAmount -= amount;
        _info.oxygenLost += amount;
    }

    public void RecordAttack(AttackInfo attackInfo)
    {
        if (!_info.isOngoing) return;

        switch (attackInfo.source)
        {
            case AttackerType.Player:
                // record damage
                _info.damageDealt += attackInfo.damage;
                if (attackInfo.isCritical) _info.criticalDamageDealt += attackInfo.damage;

                // apply Combo score
                var comboSpec = GameManager.Instance.CurrentData.playerSpec.comboSpec;
                if (comboSpec.isUnlocked)
                {
                    float score = attackInfo.damage * comboSpec.scoreRate;
                    ComboManager.Instance.AddScore(score);
                }

                break;
            case AttackerType.Debris:
                _info.damageByDebris += attackInfo.damage;
                break;
        }
    }

    public void DestroyDebris()
    {
        if (!_info.isOngoing) return;

        _info.destroyCount++;
    }

    public void RestoreOxygen(float amount)
    {
        if (!_info.isOngoing) return;

        _currentOxygenAmount += amount;
        _info.oxygenRestored += amount;
    }

    public void LootResource(BigDouble amount)
    {
        if (!_info.isOngoing) return;

        _info.lootAmount += amount;
        SessionUIController.Instance.SetResource(_info.lootAmount);
    }

    public bool TryReturn()
    {
        if (!_returnArea.IsPlayerOn) return false;

        EndSession(true);
        return true;
    }

    public void ReturnToHub()
    {
        SceneLoader.LoadScene("Hub");
    }
}
