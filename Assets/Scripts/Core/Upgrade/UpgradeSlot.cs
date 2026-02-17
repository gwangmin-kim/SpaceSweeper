using DG.Tweening;
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

    // animation
    Sequence _currentSequence;

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

    void OnValidate()
    {
#if UNITY_EDITOR
        if (_iconImage == null) return;
        if (_upgradeDefinition != null)
        {
            _iconImage.sprite = _upgradeDefinition.icon;
            gameObject.name = $"Slot_{_upgradeDefinition.Depth}_{_upgradeDefinition.name}_{_upgradeDefinition.costString}";
        }
#endif
    }

    void OnEnable()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(1.0f, 0.15f);
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
            if (_currentSequence != null && _currentSequence.IsActive())
            {
                _currentSequence.Kill();
            }
            _currentSequence = DOTween.Sequence();
            _currentSequence.Append(transform.DOScale(2.0f, 0.01f));
            _currentSequence.Append(transform.DOScale(1.0f, 0.3f)).SetEase(Ease.InCirc);
            _currentSequence.Play();
        }
        else
        {
            Debug.Log("Failed to purchase upgrade: Resource insufficient.");

            if (_currentSequence != null && _currentSequence.IsActive())
            {
                _currentSequence.Kill();
            }
            transform.rotation = Quaternion.identity;
            _currentSequence = DOTween.Sequence();
            _currentSequence.Append(transform.DORotate(new Vector3(0f, 0f, 10f), 0.1f));
            _currentSequence.Append(transform.DORotate(new Vector3(0f, 0f, -10f), 0.1f));
            _currentSequence.Append(transform.DORotate(new Vector3(0f, 0f, 0f), 0.1f));
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_upgradeDefinition == null)
        {
            Debug.LogWarning($"{name}'s upgrade definition is null");
            return;
        }

        bool isUnlocked = UpgradeManager.Instance.IsUnlocked(_upgradeDefinition.ID);
        TooltipController.Instance.ShowTooltip(_upgradeDefinition, isUnlocked);

        if (UpgradeManager.Instance.GetUpgradeState(_upgradeDefinition)
                == UpgradeManager.UpgradeState.Available)
            transform.DOScale(2.0f, 0.1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.Instance.HideTooltip();

        transform.DOScale(1.0f, 0.1f);
    }

    // 에디터 씬 뷰에서만 작동
    void OnDrawGizmos()
    {
        if (_upgradeDefinition == null) return;

        // 만약 이 업그레이드의 '부모(선행) 업그레이드'가 정의되어 있다면
        // (UpgradeDefinition에 preRequisite 같은 필드가 있다고 가정)
        if (_upgradeDefinition.parent != null)
        {
            // 하이어라키에 있는 다른 슬롯들 중에서 부모 SO를 가진 녀석을 찾음
            // (주의: FindObjectsOfType은 느리므로 에디터 확인용으로만 쓸 것)
            UpgradeSlot[] allSlots = FindObjectsByType<UpgradeSlot>(FindObjectsSortMode.None);
            foreach (var slot in allSlots)
            {
                if (slot._upgradeDefinition == _upgradeDefinition.parent)
                {
                    // 부모 -> 나 (화살표 그리기)
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(slot.transform.position, transform.position);

                    // 끝점에 구체 그려서 방향 표시
                    Gizmos.DrawSphere(transform.position, 5f);
                    break;
                }
            }
        }
    }

    void OnDisable()
    {
        _currentSequence?.Kill();
    }
}
