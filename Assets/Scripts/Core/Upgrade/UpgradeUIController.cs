using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UpgradeUIController : MonoBehaviour
{
    public static UpgradeUIController Instance { get; private set; }

    [Header("Upgrade Tree Viewport")]
    [SerializeField] GameObject _upgradePanel;
    [SerializeField] Transform _viewportContentRoot;
    [SerializeField] TooltipController _tooltip;
    RectTransform _upgradeTreeRoot;

    [Header("Status Viewport")]
    [SerializeField] StatusPanelController _statusPanel;

    List<UpgradeSlot> _upgradeSlots;

    void Awake()
    {
        Instance = this;
        _upgradePanel.SetActive(false);
    }

    void Start()
    {
        _upgradeSlots = new List<UpgradeSlot>(_viewportContentRoot.GetComponentsInChildren<UpgradeSlot>(true));
        _upgradeTreeRoot = _viewportContentRoot.GetComponent<RectTransform>();

        RefreshAllSlots();
    }

    void OnEnable()
    {
        if (_upgradeSlots != null) RefreshAllSlots();
    }

    void Update()
    {
        if (_upgradePanel.activeSelf)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame) OnPanelCloseButton();
            }
        }
    }

    public void RefreshAllSlots()
    {
        foreach (var slot in _upgradeSlots)
        {
            slot.RefreshState();
        }
        _statusPanel.RefreshAll();
    }

    public void ActivatePanel()
    {
        _upgradePanel.SetActive(true);

        _upgradeTreeRoot.anchoredPosition = UpgradeManager.Instance == null ? Vector2.zero : UpgradeManager.Instance.upgradeTreePosition;
    }

    public void OnPanelCloseButton()
    {
        if (UpgradeManager.Instance != null) UpgradeManager.Instance.upgradeTreePosition = _upgradeTreeRoot.anchoredPosition;

        _upgradePanel.SetActive(false);
        _tooltip.HideTooltip();
    }
}
