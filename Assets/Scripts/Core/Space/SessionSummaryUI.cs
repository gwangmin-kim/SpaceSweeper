using UnityEngine;
using System.Collections.Generic;

public class SessionSummaryUI : MonoBehaviour
{
    [SerializeField] List<SessionFieldTextUI> _sessionFieldList = new List<SessionFieldTextUI>();
    void Awake()
    {
        _sessionFieldList.AddRange(GetComponentsInChildren<SessionFieldTextUI>(true));
    }

    void OnEnable()
    {
        RefreshAll();
    }

    void UpdateVisibility()
    {

    }

    public void RefreshAll()
    {
        UpdateVisibility();

        foreach (var slot in _sessionFieldList)
        {
            if (slot.gameObject.activeSelf)
            {
                slot.UpdateValue();
            }
        }
    }
}
