using UnityEngine;
using System.Collections.Generic;

public class StatusPanelController : MonoBehaviour
{
    [SerializeField] List<StatusFieldTextUI> _statusFieldList = new List<StatusFieldTextUI>();

    void Awake()
    {
        _statusFieldList.AddRange(GetComponentsInChildren<StatusFieldTextUI>(true));
    }

    void OnEnable()
    {
        RefreshAll();
    }

    public void RefreshAll()
    {
        var data = GameManager.Instance.CurrentData;

        UpdateVisibility(data.playerSpec);

        foreach (var slot in _statusFieldList)
        {
            if (slot.gameObject.activeSelf)
            {
                slot.UpdateValue();
            }
        }
    }

    void UpdateVisibility(PlayerSpec spec)
    {
        // foreach (var slot in _statusSlots)
        // {
        //     // 특정 조건에 따라 끄고 켜는 로직
        //     // 예: "Shotgun" 키워드가 포함된 슬롯은 샷건일 때만 활성화
        //     if (slot.name.Contains("Shotgun"))
        //     {
        //         slot.gameObject.SetActive(spec.currentWeapon == WeaponType.Shotgun);
        //     }
        //     // ... 추가 조건들
        // }
    }
}
