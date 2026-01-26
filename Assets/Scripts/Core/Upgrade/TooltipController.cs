using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] GameObject _tooltipPanel;
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] TextMeshProUGUI _costText;
    [SerializeField] TextMeshProUGUI _descriptionText;

    [Header("Settings")]
    [SerializeField] Vector2 _offset;
    [SerializeField] Color _cantUpgradeColor;

    void Awake()
    {
        Instance = this;
        HideTooltip();
    }

    void Update()
    {
        // 툴팁 창을 마우스 위치에 맞추기
        if (_tooltipPanel.activeSelf)
        {
            if (Mouse.current != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                transform.position = (Vector3)mousePos + (Vector3)_offset;
            }
        }
    }

    public void ShowTooltip(UpgradeDefinition upgradeDefinition, bool isUnlocked)
    {
        _tooltipPanel.SetActive(true);

        _nameText.text = upgradeDefinition.upgradeName;
        _descriptionText.text = upgradeDefinition.description;

        if (isUnlocked) { }
        else
        {
            _costText.color = (ResourceManager.Instance.Gold >= upgradeDefinition.Cost)
                ? Color.white : _cantUpgradeColor;
            _costText.text = upgradeDefinition.costString;
        }
    }

    public void HideTooltip()
    {
        _tooltipPanel.SetActive(false);
    }
}
