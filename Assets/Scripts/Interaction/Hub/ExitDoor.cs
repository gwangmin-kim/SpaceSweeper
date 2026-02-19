using System.Collections;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))]
public class ExitDoor : MonoBehaviour, IInteractable
{
    [Header("Collision")]
    [SerializeField] LayerMask _playerMask;

    [Header("UI")]
    [SerializeField] GameObject _uiExitIndicator;
    [SerializeField] GameObject _upgradeWarningIndicator;
    [SerializeField] float _warningTime;

    Sequence _currentSequence;
    Coroutine _warningRoutine;

    // 산소가 있어야 탐사 가능
    bool IsAvailable => GameManager.Instance.CurrentData.playerSpec.oxygenAmount > 0f;

    void Awake()
    {
        _uiExitIndicator.SetActive(false);
        _upgradeWarningIndicator.SetActive(false);
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

    IEnumerator WarningUIRoutine()
    {
        yield return new WaitForSeconds(_warningTime);
        _upgradeWarningIndicator.SetActive(false);
    }

    public void Interact(bool isPressed)
    {
        if (!isPressed) return;

        if (!IsAvailable)
        {
            _upgradeWarningIndicator.SetActive(true);

            // Vector2 position = _upgradeWarningIndicator.transform.localPosition;
            // position.x = 0f;
            // _upgradeWarningIndicator.transform.localPosition = position;

            if (_currentSequence != null && _currentSequence.IsActive())
            {
                _currentSequence.Kill();
            }

            _currentSequence = DOTween.Sequence();
            _currentSequence.Append(_upgradeWarningIndicator.transform.DOMoveX(0f, 0.05f));
            _currentSequence.Append(_upgradeWarningIndicator.transform.DOMoveX(-0.2f, 0.05f));
            _currentSequence.Append(_upgradeWarningIndicator.transform.DOMoveX(0.2f, 0.05f));
            _currentSequence.Append(_upgradeWarningIndicator.transform.DOMoveX(-0.2f, 0.05f));
            _currentSequence.Append(_upgradeWarningIndicator.transform.DOMoveX(0.2f, 0.05f));
            _currentSequence.Append(_upgradeWarningIndicator.transform.DOMoveX(0f, 0.05f));

            if (_warningRoutine != null) StopCoroutine(_warningRoutine);
            _warningRoutine = StartCoroutine(WarningUIRoutine());
            return;
        }

        SceneLoader.LoadScene("Game");
    }
}
