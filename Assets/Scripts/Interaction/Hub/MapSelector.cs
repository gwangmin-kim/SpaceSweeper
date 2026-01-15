using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MapSelector : MonoBehaviour, IInteractable
{
    [Header("Collision")]
    [SerializeField] LayerMask _playerMask;

    [Header("UI")]
    [SerializeField] GameObject _uiSelectMapIndicator;

    void Awake()
    {
        _uiSelectMapIndicator.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & _playerMask) != 0)
        {
            // UI
            _uiSelectMapIndicator.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & _playerMask) != 0)
        {
            // UI
            _uiSelectMapIndicator.SetActive(false);
        }
    }

    public void Interact(bool isPressed)
    {
        if (!isPressed) return;

        // 장소 선택 UI 호출
    }
}
