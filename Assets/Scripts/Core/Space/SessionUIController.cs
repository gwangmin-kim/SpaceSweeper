using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BreakInfinity;

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

    public void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _summaryPanel.SetActive(false);
    }

    string SummaryString(SessionInformation sessionInformation)
    {
        string time = $"{sessionInformation.timer:F2}s";
        string loot = BigDoubleFormatter.Format(sessionInformation.lootAmount);
        string damage = $"{sessionInformation.damageReceived}";

        return $"{time}\n\n{loot}\n\n{damage}";
    }

    public void SetOxygen(float amount, float ratio)
    {
        _oxygenTimer.text = $"{amount:F2}s";
        _oxygenBar.fillAmount = ratio;
    }

    public void SetResource(BigDouble amount)
    {
        _resourceAmount.text = $"{amount}";
    }

    public void SessionSummary(SessionInformation sessionInformation)
    {
        _summaryPanel.SetActive(true);
        _summaryText.text = SummaryString(sessionInformation);
    }
}
