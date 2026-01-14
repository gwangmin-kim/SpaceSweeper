using UnityEngine;

public class SessionManager : MonoBehaviour
{
    // Singleton
    public static SessionManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] SpacePlayerController _player;

    [Header("Spaceship Return")]
    [SerializeField] SpaceshipController _returnArea;

    [Header("Oxygen")]
    [SerializeField] float _totalOxygenAmount; // 산소량 (단위: 초)

    // Oxygen
    float _currentOxygenAmount = 0f;

    // Resource
    int _sessionLoot = 0; // 탐사 세션 중 획득한 자원량 (산소 고갈 시 소실)

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Space Scene에만 존재, 매 세션 초기화 (no DontDestroyOnLoad)
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartSession();
    }

    void Update()
    {
        if (_currentOxygenAmount > 0f)
        {
            _currentOxygenAmount -= Time.deltaTime;
            float oxygenRatio = Mathf.Clamp01(_currentOxygenAmount / _totalOxygenAmount);

            SessionUIManager.Instance.SetOxygen(_currentOxygenAmount, oxygenRatio);
        }
        else
        {
            SessionUIManager.Instance.SetOxygen(0f, 0f);
            EndSession();
        }
    }

    void StartSession()
    {
        _currentOxygenAmount = _totalOxygenAmount;
        _sessionLoot = 0;

        SessionUIManager.Instance.SetOxygen(_currentOxygenAmount, 1f);
        SessionUIManager.Instance.SetResource(_sessionLoot);
    }

    void EndSession()
    {
        if (_currentOxygenAmount > 0f)
        {
            // 플레이어 자발적 귀환: 성공
            if (ResourceManager.Instance != null) ResourceManager.Instance.StoreResources(_sessionLoot);

        }
        else
        {
            // 산소 고갈: 강제 종료
        }

        SessionInformation.lootAmount = _sessionLoot;
        SessionInformation.explorationTime = _totalOxygenAmount - _currentOxygenAmount;
        SessionInformation.damageInflicted = 0;

        SessionUIManager.Instance.SessionSummary();
    }

    public void LootResource(int amount)
    {
        _sessionLoot += amount;
        SessionUIManager.Instance.SetResource(_sessionLoot);
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

    public void Cancel()
    {

    }
}
