using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HubUIManager : MonoBehaviour
{
    // Singleton
    public static HubUIManager Instance { get; private set; }

    [Header("Resource")]
    [SerializeField] TextMeshProUGUI _resourceAmount;
    [SerializeField] TextMeshProUGUI _goldAmount;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetResource(ResourceManager.Instance.Resource);
        SetGold(ResourceManager.Instance.Gold);
    }

    public void SetResource(int amount)
    {
        _resourceAmount.text = $"{amount}";
    }

    public void SetGold(int amount)
    {
        _goldAmount.text = $"{amount}";
    }
}
