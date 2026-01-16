using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    // Singleton
    public static ResourceManager Instance { get; private set; }

    public int Resource => GameManager.Instance.CurrentData.resource;
    public int Gold => GameManager.Instance.CurrentData.gold;

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
        GameManager.Instance.CurrentData.resource += sessionLootAmount;
    }

    public int WithdrawResources(int amount)
    {
        var data = GameManager.Instance.CurrentData;

        if (data.resource >= amount)
        {
            data.resource -= amount;
            return amount;
        }
        else
        {
            int remaining = data.resource;
            data.resource = 0;
            return remaining;
        }
    }

    public void AddGold(int amount)
    {
        GameManager.Instance.CurrentData.gold += amount;
    }
}
