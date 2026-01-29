using UnityEngine;
using BreakInfinity;

[System.Serializable]
public class SessionInformation
{
    public bool isOngoing;
    public float timer;
    public BigDouble lootAmount;
    public float damageReceived; // 잃은 산소량
    public int damageDealt; // 폐기물에 가한 피해량
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

    // Oxygen
    float _totalOxygenAmountInverse = 0f; // 산소 총량의 역수 (연산 효율성 위해 역수로 저장)
    float _currentOxygenAmount = 0f; // 현재 산소량
    float _oxygenConsumptionPerSec = 1f; // 초당 소비 산소량

    public void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitSession();
    }

    void Update()
    {
        if (_info.isOngoing && _currentOxygenAmount > 0f)
        {
            _currentOxygenAmount -= Time.deltaTime * _oxygenConsumptionPerSec;
            float oxygenRatio = Mathf.Clamp01(_currentOxygenAmount * _totalOxygenAmountInverse);

            _info.timer += Time.deltaTime;

            SessionUIController.Instance.SetOxygen(_currentOxygenAmount, oxygenRatio);
        }
        else
        {
            SessionUIController.Instance.SetOxygen(0f, 0f);
            EndSession();
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

        StageManager.Instance.LoadLevel();

        SpacePlayerController.Instance.enabled = true;
    }

    void EndSession()
    {
        if (!_info.isOngoing) return;

        if (_currentOxygenAmount > 0f)
        {
            // 플레이어 자발적 귀환: 성공
            if (ResourceManager.Instance != null) ResourceManager.Instance.StoreResources(_info.lootAmount);

        }
        else
        {
            // 산소 고갈: 강제 종료
            float resourceLossRatio = GameManager.Instance.CurrentData.playerSpec.resourceLossRatio;

            _info.lootAmount *= 1f - resourceLossRatio;
        }

        _info.isOngoing = false;

        SessionUIController.Instance.SessionSummary(_info);

        StageManager.Instance.StopAllGimickRoutines();

        SpacePlayerController.Instance.enabled = false;
    }

    public void ReceiveDamage(float amount)
    {
        // 플레이어가 공격 받아 산소를 잃음
        _currentOxygenAmount -= amount;
        _info.damageReceived += amount;
    }

    public void DealDamage(int amount)
    {
        _info.damageDealt += amount;
    }

    public void LootResource(BigDouble amount)
    {
        _info.lootAmount += amount;
        SessionUIController.Instance.SetResource(_info.lootAmount);
    }

    public void TryReturn()
    {
        if (!_returnArea.IsPlayerOn) return;

        EndSession();
    }

    public void ReturnToHub()
    {
        SceneLoader.LoadScene("Hub");
    }
}
