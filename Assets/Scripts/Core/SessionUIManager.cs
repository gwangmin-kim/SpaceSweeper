using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionUIManager : MonoBehaviour
{
    // Singleton
    public static SessionUIManager Instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] Image _oxygenBar;
    [SerializeField] TextMeshProUGUI _oxygenTimer;
    [SerializeField] TextMeshProUGUI _resourceAmount;

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

    public void SetOxygen(float amount, float ratio)
    {
        _oxygenTimer.text = $"{amount:F1}s";
        _oxygenBar.fillAmount = ratio;
    }

    public void SetResource(int amount)
    {
        _resourceAmount.text = $"{amount}";
    }
}
