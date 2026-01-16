using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    // Singleton
    public static ResourceManager Instance { get; private set; }

    [SerializeField] private int _resource = 0;
    [SerializeField] private int _gold = 0;

    public int Resource => _resource;
    public int Gold => _gold;

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

    public void StoreResources(int sessionLootAmount)
    {
        _resource += sessionLootAmount;
    }

    public int WithdrawResources(int amount)
    {
        if (_resource >= amount)
        {
            _resource -= amount;
            return amount;
        }
        else
        {
            amount = _resource;
            _resource = 0;
            return amount;
        }
    }

    public void AddGold(int amount)
    {
        _gold += amount;
    }
}
