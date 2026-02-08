using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BreakInfinity;
using DG.Tweening;

public class SessionUIController : MonoBehaviour
{
    // Singleton
    public static SessionUIController Instance { get; private set; }

    [Header("Oxygen")]
    [SerializeField] Color _defalutColor;
    [SerializeField] Image _oxygenBar;
    [SerializeField] TextMeshProUGUI _oxygenText;

    [Header("Oxygen Warning")]
    [SerializeField] float _warningThreshold;
    [SerializeField] Color _warningColor;
    [SerializeField] Image _warningPanel;
    [SerializeField] TextMeshProUGUI _warningText;
    [SerializeField] float _warningPanelAlpha;

    [Header("Resource")]
    [SerializeField] TextMeshProUGUI _resourceAmount;

    [Header("Summary")]
    [SerializeField] GameObject _summaryPanel;
    [SerializeField] TextMeshProUGUI _summaryFieldText;
    [SerializeField] TextMeshProUGUI _summaryValueText;

    bool _isWarningActive = false;
    float _warningThresholdInverse = 0f;

    public void Awake()
    {
        Instance = this;
        if (_warningThreshold != 0f) _warningThresholdInverse = 1f / _warningThreshold;

        _warningText.alpha = 0f;

        _warningPanel.gameObject.SetActive(false);
        _warningText.gameObject.SetActive(false);
    }

    void Start()
    {
        _summaryPanel.SetActive(false);
    }

    string SummaryFieldString(SessionInformation sessionInformation)
    {
        string time = "탐사 시간";
        string loot = "획득한 금속 파편";
        string damage = (sessionInformation.damageDealt > 0f) ? "입힌 피해량" : "";

        return $"{time}\n\n{loot}\n\n{damage}";
    }

    string SummaryValueString(SessionInformation sessionInformation)
    {
        string time = $"{sessionInformation.timer:F2}s";
        string loot = BigDoubleFormatter.Format(sessionInformation.lootAmount);
        string damage = (sessionInformation.damageDealt > 0f) ? $"{sessionInformation.damageDealt}" : "";

        return $"{time}\n\n{loot}\n\n{damage}";
    }

    public void SetOxygen(float amount, float ratio)
    {
        _oxygenText.text = $"{amount:F2}";
        _oxygenBar.fillAmount = ratio;

        if (ratio < _warningThreshold)
        {
            if (!_isWarningActive) OnWarningEnter();

            UpdateWarningVisual(ratio);
        }
        else if (_isWarningActive)
        {
            OnWarningExit();
        }
    }

    void OnWarningEnter()
    {
        _isWarningActive = true;

        _warningPanel.gameObject.SetActive(true);
        _warningText.gameObject.SetActive(true);

        _warningText.DOFade(1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    void UpdateWarningVisual(float ratio)
    {
        float t = ratio * _warningThresholdInverse;

        Color warningPanelColor = _warningColor;
        warningPanelColor.a = Mathf.Lerp(_warningPanelAlpha, 0f, t);
        _warningPanel.color = warningPanelColor;

        Color indicatorColor = Color.Lerp(_warningColor, _defalutColor, t);
        _oxygenBar.color = indicatorColor;
        _oxygenText.color = indicatorColor;
    }

    void OnWarningExit()
    {
        _isWarningActive = false;

        _oxygenBar.color = _defalutColor;
        _oxygenText.color = _defalutColor;
        _warningText.DOKill();

        _warningText.alpha = 0f;

        _warningPanel.gameObject.SetActive(false);
        _warningText.gameObject.SetActive(false);
    }

    public void SetResource(BigDouble amount)
    {
        _resourceAmount.text = $"{amount}";
    }

    public void SessionSummary(SessionInformation sessionInformation)
    {
        if (_isWarningActive) OnWarningExit();

        _summaryPanel.SetActive(true);
        _summaryFieldText.text = SummaryFieldString(sessionInformation);
        _summaryValueText.text = SummaryValueString(sessionInformation);
    }
}
