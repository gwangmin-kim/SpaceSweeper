using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TooltipController : MonoBehaviour
{
    RectTransform _rectTransform;

    public static TooltipController Instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] GameObject _tooltipPanel;
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] Image _costIcon;
    [SerializeField] TextMeshProUGUI _costText;
    [SerializeField] TextMeshProUGUI _descriptionText;

    [Header("Settings")]
    [SerializeField] Vector2 _offset;
    [SerializeField] Color _unlockedColor;
    [SerializeField] Color _cantUpgradeColor;

    void Awake()
    {
        Instance = this;

        _rectTransform = _tooltipPanel.GetComponent<RectTransform>();
        HideTooltip();
    }

    void Update()
    {
        // 툴팁 창을 마우스 위치에 맞추기
        if (_tooltipPanel.activeSelf)
        {
            if (Mouse.current != null)
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                // Debug.Log($"mouse position: {mousePosition}");

                // 기본 오프셋은 툴팁 좌상단 기준
                // 만약 툴팁이 카메라를 벗어난다면 (오른쪽 혹은 아래쪽으로)
                // 툴팁의 크기에 따라 위치를 이동
                Vector2 finalPosition = mousePosition + _offset;

                float width = _rectTransform.rect.width;
                float height = _rectTransform.rect.height;

                // Debug.Log($"width: {width}, height: {height}");
                // Debug.Log($"screen width: {Screen.width}, height: {Screen.height}");

                if (finalPosition.x + width > Screen.width)
                {
                    finalPosition.x = mousePosition.x - width - _offset.x;
                }
                if (finalPosition.y - height < 0)
                {
                    finalPosition.y = mousePosition.y + height + _offset.y;
                }

                transform.position = finalPosition;
            }
        }
    }

    public void ShowTooltip(UpgradeDefinition upgradeDefinition, bool isUnlocked)
    {
        _tooltipPanel.SetActive(true);

        _nameText.text = upgradeDefinition.upgradeName;
        _descriptionText.text = upgradeDefinition.description;

        if (isUnlocked)
        {
            _costIcon.enabled = false;
            _costText.text = LocalizationManager.Instance.GetLocalizedText("Unlocked");
            _costText.color = _unlockedColor;
        }
        else
        {
            _costIcon.enabled = true;
            _costText.text = BigDoubleFormatter.Format(upgradeDefinition.Cost);
            _costText.color = (ResourceManager.Instance.Gold >= upgradeDefinition.Cost)
                ? Color.white : _cantUpgradeColor;
        }
    }

    public void HideTooltip()
    {
        _tooltipPanel.SetActive(false);
    }
}
