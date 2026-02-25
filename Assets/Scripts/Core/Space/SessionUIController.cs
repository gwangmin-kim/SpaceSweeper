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

    public void SetOxygen(float amount, float ratio)
    {
        _oxygenText.text = amount.ToString("0.0");
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
        _resourceAmount.text = BigDoubleFormatter.Format(amount);
    }

    public void SessionSummary(SessionInformation info)
    {
        if (_isWarningActive) OnWarningExit();

        _summaryPanel.SetActive(true);
    }
}
