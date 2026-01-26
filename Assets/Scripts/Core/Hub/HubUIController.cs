using TMPro;
using UnityEngine;
using BreakInfinity;

public class HubUIController : MonoBehaviour
{
    // Singleton
    public static HubUIController Instance { get; private set; }

    [Header("Resource")]
    [SerializeField] TextMeshProUGUI _resourceAmount;
    [SerializeField] TextMeshProUGUI _goldAmount;

    public void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetResource(ResourceManager.Instance.Resource);
        SetGold(ResourceManager.Instance.Gold);
    }

    public void SetResource(BigDouble amount)
    {
        _resourceAmount.text = $"{BigDoubleFormatter.Format(amount)}";
    }

    public void SetGold(BigDouble amount)
    {
        _goldAmount.text = $"{BigDoubleFormatter.Format(amount)}";
    }
}
