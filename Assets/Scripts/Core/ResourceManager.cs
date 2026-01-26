using UnityEngine;
using BreakInfinity;

public class ResourceManager : MonoBehaviour
{
    // Singleton
    public static ResourceManager Instance { get; private set; }

    public BigDouble Resource => GameManager.Instance.CurrentData.resource;
    public BigDouble Gold => GameManager.Instance.CurrentData.gold;

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

    public void StoreResources(BigDouble sessionLootAmount)
    {
        GameManager.Instance.CurrentData.resource += sessionLootAmount;
    }

    public BigDouble WithdrawResources(BigDouble amount)
    {
        var data = GameManager.Instance.CurrentData;

        if (data.resource >= amount)
        {
            data.resource -= amount;
            return amount;
        }
        else
        {
            BigDouble remaining = data.resource;
            data.resource = 0;
            return remaining;
        }
    }

    public void AddGold(BigDouble amount)
    {
        GameManager.Instance.CurrentData.gold += amount;
    }

    public bool TryWithdrawGold(BigDouble amount)
    {
        var data = GameManager.Instance.CurrentData;

        if (data.gold < amount) return false;

        data.gold -= amount;
        return true;
    }
}
