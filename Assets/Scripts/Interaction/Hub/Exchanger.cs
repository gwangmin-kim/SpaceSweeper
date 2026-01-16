using Mono.Cecil;
using UnityEngine;

public class Exchanger : MonoBehaviour, IInteractable
{
    [Header("Collision")]
    [SerializeField] LayerMask _playerMask;

    [Header("UI")]
    [SerializeField] GameObject _uiExchangeIndicator;

    [Header("Exchange Status")]
    [SerializeField] int _exchangeAmount; // 한 번에 교환하는 자원량
    [SerializeField] float _exchangeInterval; // 교환 속도 (매 횟수 발동 간격)
    [SerializeField] int _goldPerResource; // 교환비

    bool _isInteracting = false;

    float _exchangeTimer = 0f;

    void Awake()
    {
        _uiExchangeIndicator.SetActive(false);
    }

    void Update()
    {
        if (_exchangeTimer > 0f) _exchangeTimer -= Time.deltaTime;

        if (_isInteracting && _exchangeTimer <= 0f)
        {
            _exchangeTimer = _exchangeInterval;

            // 자원 교환
            int amount = ResourceManager.Instance.WithdrawResources(_exchangeAmount);
            if (amount > 0)
            {
                ResourceManager.Instance.AddGold(amount * _goldPerResource);
            }

            HubUIManager.Instance.SetResource(ResourceManager.Instance.Resource);
            HubUIManager.Instance.SetGold(ResourceManager.Instance.Gold);
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
