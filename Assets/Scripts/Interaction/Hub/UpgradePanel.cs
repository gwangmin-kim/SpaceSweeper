using UnityEngine;

public class UpgradePanel : MonoBehaviour, IInteractable
{
    [Header("Collision")]
    [SerializeField] LayerMask _playerMask;

    [Header("UI")]
    [SerializeField] GameObject _uiUpgradeIndicator;

    void Awake()
    {
        _uiUpgradeIndicator.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & _playerMask) != 0)
        {
            // UI
            _uiUpgradeIndicator.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & _playerMask) != 0)
        {
            // UI
            _uiUpgradeIndicator.SetActive(false);
        }
    }

    public void Interact(bool isPressed)
    {
        if (!isPressed) return;

        // 업그레이드 패널 UI 호출
    }
}
