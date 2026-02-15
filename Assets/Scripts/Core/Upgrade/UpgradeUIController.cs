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

    List<UpgradeSlot> _upgradeSlots;

    void Awake()
    {
        Instance = this;
        _upgradePanel.SetActive(false);
    }

    void Start()
    {
        _upgradeSlots = new List<UpgradeSlot>(_viewportContentRoot.GetComponentsInChildren<UpgradeSlot>(true));

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
    }

    public void ActivatePanel()
    {
        _upgradePanel.SetActive(true);
    }

    public void OnPanelCloseButton()
    {
        _upgradePanel.SetActive(false);
        _tooltip.HideTooltip();
    }
}
