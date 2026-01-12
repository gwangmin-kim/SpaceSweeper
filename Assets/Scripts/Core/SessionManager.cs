using UnityEngine;

public class SessionManager : MonoBehaviour
{
    // Singleton
    public static SessionManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] SpacePlayerController _player;

    [Header("Spaceship Return")]
    [SerializeField] Collider2D _returnArea; // 트리거 콜라이더, 이 위에서 귀환 시도 시 세션 종료

    [Header("Oxygen")]
    [SerializeField] float _totalOxygenAmount; // 산소량 (단위: 초)

    // Return
    bool _isReturning = false;

    // Oxygen
    float _currentOxygenAmount = 0f;

    // Resource
    int _sessionLoot = 0; // 탐사 세션 중 획득한 자원량 (산소 고갈 시 소실)

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Space Scene에만 존재, 매 세션 초기화
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
            float oxygenRatio = _currentOxygenAmount / _totalOxygenAmount;

            SessionUIManager.Instance.SetOxygen(_currentOxygenAmount, oxygenRatio);
        }
        else
        {
            SessionUIManager.Instance.SetOxygen(0f, 0f);
            EndSession();
        }
    }

    public void StartSession()
    {
        _isReturning = false;
        _currentOxygenAmount = _totalOxygenAmount;
        _sessionLoot = 0;

        SessionUIManager.Instance.SetOxygen(_currentOxygenAmount, 1f);
        SessionUIManager.Instance.SetResource(_sessionLoot);
    }

    public void LootResource(int amount)
    {
        _sessionLoot += amount;
        SessionUIManager.Instance.SetResource(_sessionLoot);
    }

    public void EndSession()
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
    }

    public void TryReturn()
    {
        // 귀환 시도 중일 경우: 이미 해당 UI 출력 중, 수락 시 세션 종료
        if (_isReturning)
        {
            _isReturning = true;
            //? 옵션: 이 동안 세션 내 일시 정지 효과
        }
        // 첫 귀환 시도: 세션 종료 확정 여부 UI 출력
        else
        {
            EndSession();
        }
    }

    public void Cancel()
    {
        // 귀환 시도 중일 경우: 해당 UI 제거 및 세션 재개
        if (_isReturning)
        {
            _isReturning = false;
            //? 옵션: 세션 재개
        }
        // 일시정지 및 해당 UI 출력 (설정, 종료 등)
        else
        {

        }
    }
}
