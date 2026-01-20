using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionUIController : MonoBehaviour
{
    // Singleton
    public static SessionUIController Instance { get; private set; }

    [Header("Oxygen")]
    [SerializeField] Image _oxygenBar;
    [SerializeField] TextMeshProUGUI _oxygenTimer;

    [Header("Resource")]
    [SerializeField] TextMeshProUGUI _resourceAmount;

    [Header("Summary")]
    [SerializeField] GameObject _summaryPanel;
    [SerializeField] TextMeshProUGUI _summaryText;

    string SummaryString => $"{SessionInformation.explorationTime:F2}s\n\n{SessionInformation.lootAmount}\n\n{SessionInformation.damageInflicted}";

    public void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _summaryPanel.SetActive(false);
    }

    public void SetOxygen(float amount, float ratio)
    {
        _oxygenTimer.text = $"{amount:F2}s";
        _oxygenBar.fillAmount = ratio;
    }

    public void SetResource(int amount)
    {
        _resourceAmount.text = $"{amount}";
    }

    public void SessionSummary()
    {
        _summaryPanel.SetActive(true);
        _summaryText.text = SummaryString;
    }
}
