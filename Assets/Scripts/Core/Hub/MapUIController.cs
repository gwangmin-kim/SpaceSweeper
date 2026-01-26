using UnityEngine;
using UnityEngine.InputSystem;

public class MapUIController : MonoBehaviour
{
    public static MapUIController Instance { get; private set; }

    [Header("Contents")]
    [SerializeField] GameObject _selectMapPanel;

    void Awake()
    {
        Instance = this;
        _selectMapPanel.SetActive(false);
    }

    void Update()
    {
        if (_selectMapPanel.activeSelf)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame) OnPanelCloseButton();
            }
        }
    }

    public void ActivatePanel()
    {
        _selectMapPanel.SetActive(true);
    }

    public void OnPanelCloseButton()
    {
        _selectMapPanel.SetActive(false);
    }
}
