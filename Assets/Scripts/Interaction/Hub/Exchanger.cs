using UnityEngine;

public class Exchanger : MonoBehaviour, IInteractable
{
    [Header("Collision")]
    [SerializeField] LayerMask _playerMask;

    [Header("UI")]
    [SerializeField] GameObject _uiExchangeIndicator;

    bool _isInteracting = false;

    void Awake()
    {
        _uiExchangeIndicator.SetActive(false);
    }

    void Update()
    {
        if (_isInteracting)
        {

        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & _playerMask) != 0)
        {
            // UI
            _uiExchangeIndicator.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & _playerMask) != 0)
        {
            // UI
            _uiExchangeIndicator.SetActive(false);
        }
    }

    public void Interact(bool isPressed)
    {
        // 교환 애니메이션?
        // E를 꾹 누르면 플레이어로부터 자원파편이 좌르륵 빨려들어가서 돈으로 띠링띠링 전환되는 연출이 좋아보임
        // 교환을 처음에는 좀 느리게 하다가, 교환 속도도 업그레이드로 추가?
        _isInteracting = isPressed;
    }
}
