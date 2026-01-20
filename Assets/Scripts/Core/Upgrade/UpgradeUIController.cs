using System.Collections.Generic;
using UnityEngine;

public class UpgradeUIController : MonoBehaviour
{
    public static UpgradeUIController Instance { get; private set; }

    [Header("Viewport")]
    [SerializeField] Transform _viewportContentRoot;

    List<UpgradeSlot> _upgradeSlots;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _upgradeSlots = new List<UpgradeSlot>(_viewportContentRoot.GetComponentsInChildren<UpgradeSlot>(true));

        RefreshAllSlots();
    }

    void OnEnable()
    {
        if (_upgradeSlots != null) RefreshAllSlots();
    }

    public void RefreshAllSlots()
    {
        foreach (var slot in _upgradeSlots)
        {
            slot.RefreshState();
        }
    }
}
