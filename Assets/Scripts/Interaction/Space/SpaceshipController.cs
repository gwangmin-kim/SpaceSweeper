using UnityEngine;

public class SpaceshipController : MonoBehaviour
{
    [Header("Collision")]
    [SerializeField] LayerMask _playerMask;

    [Header("UI")]
    [SerializeField] GameObject _uiReturnIndicator;

    bool _isPlayerOn = false;

    public bool IsPlayerOn => _isPlayerOn;

    void Awake()
    {
        _uiReturnIndicator.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & _playerMask) != 0)
        {
            _isPlayerOn = true;

            // UI
            _uiReturnIndicator.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & _playerMask) != 0)
        {
            _isPlayerOn = false;

            // UI
            _uiReturnIndicator.SetActive(false);
        }
    }
}
