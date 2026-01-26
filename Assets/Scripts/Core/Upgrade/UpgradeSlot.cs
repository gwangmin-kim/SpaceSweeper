using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class UpgradeSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Upgrade Data")]
    [SerializeField] UpgradeDefinition _upgradeDefinition;

    [Header("UI Components")]
    [SerializeField] Button _button;
    [SerializeField] Image _iconImage; // 자식 오브젝트에 달림
    [SerializeField] Image _backgroundImage;
    [SerializeField] GameObject _completedOverlay;

    void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);

        _backgroundImage = GetComponent<Image>();
    }

    void Start()
    {
        _iconImage.sprite = _upgradeDefinition.icon;

        RefreshState();
    }

    void SetTransparency(float alpha)
    {
        Color iconColor = _iconImage.color;
        iconColor.a = alpha;
        _iconImage.color = iconColor;

        Color backgroundColor = _backgroundImage.color;
        backgroundColor.a = alpha;
        _backgroundImage.color = backgroundColor;
    }

    public void RefreshState()
    {
        if (_upgradeDefinition == null)
        {
            Debug.LogWarning($"{name}'s upgrade definition is null");
            return;
        }

        var upgradeState = UpgradeManager.Instance.GetUpgradeState(_upgradeDefinition);
        switch (upgradeState)
        {
            case UpgradeManager.UpgradeState.Locked:
                gameObject.SetActive(false);
                break;
            case UpgradeManager.UpgradeState.Available:
                gameObject.SetActive(true);

                _completedOverlay.SetActive(false);
                _button.interactable = true;

                SetTransparency(0.5f);
                break;
            case UpgradeManager.UpgradeState.Unlocked:
                gameObject.SetActive(true);

                _completedOverlay.SetActive(true);
                _button.interactable = false;

                SetTransparency(1.0f);
                break;
        }
    }

    public void OnClick()
    {
        if (UpgradeManager.Instance.TryPurchaseUpgrade(_upgradeDefinition))
        {
            UpgradeUIController.Instance.RefreshAllSlots();
        }
        else
        {
            Debug.Log("Failed to purchase upgrade: Resource insufficient.");
            // 실패 시 피드백 효과
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_upgradeDefinition == null)
        {
            Debug.LogWarning($"{name}'s upgrade definition is null");
            return;
        }

        bool isUnlocked = UpgradeManager.Instance.IsUnlocked(_upgradeDefinition.id);
        TooltipController.Instance.ShowTooltip(_upgradeDefinition, isUnlocked);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.Instance.HideTooltip();
    }
}
