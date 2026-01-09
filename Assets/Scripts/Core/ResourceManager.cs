using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    // Singleton
    public static ResourceManager Instance;

    private int _storedResource = 0; // 탐사 세션 종료 후, 획득이 확정된 자원량
    private int _ephimeralResource = 0; // 탐사 세션 중 획득한 자원량 (산소 고갈 시 소실)

    private int _gold = 0;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeNewSession()
    {
        _ephimeralResource = 0;
    }

    public void EarnResources(int amount)
    {
        _ephimeralResource += amount;
    }

    public void StoreResources()
    {
        _storedResource += _ephimeralResource;
        _ephimeralResource = 0;
    }

    public void LoseResources()
    {
        _ephimeralResource = 0;
    }
}
