using UnityEngine;

public class ExitDoor : MonoBehaviour, IInteractable
{
    [Header("Collision")]
    [SerializeField] LayerMask _playerMask;

    [Header("UI")]
    [SerializeField] GameObject _uiExitIndicator;

    void Awake()
    {
        _uiExitIndicator.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & _playerMask) != 0)
        {
            // UI
            _uiExitIndicator.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & _playerMask) != 0)
        {
            // UI
            _uiExitIndicator.SetActive(false);
        }
    }

    public void Interact()
    {
        SceneLoader.LoadScene("Game");
    }
}
