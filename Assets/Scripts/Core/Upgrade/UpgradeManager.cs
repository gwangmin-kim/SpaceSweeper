using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UpgradeManager : MonoBehaviour
{
    // Singleton
    public static UpgradeManager Instance { get; private set; }

    [Header("All Upgrades")]
    [SerializeField] List<UpgradeDefinition> _allUpgrades;

    [Header("UI")]
    [SerializeField] GameObject _upgradePanel;

    // 검색 효율 위한 딕셔너리 (ID -> SO)
    Dictionary<string, UpgradeDefinition> _upgradeMap;

    public enum UpgradeState { Locked, Available, Unlocked };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeDictionary();

        _upgradePanel.SetActive(false);
    }

    void Update()
    {
        if (_upgradePanel.activeSelf)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame) OnPanelCloseButton();
            }
        }
    }

    void InitializeDictionary()
    {
        _upgradeMap = new Dictionary<string, UpgradeDefinition>();

        foreach (var upgrade in _allUpgrades)
        {
            _upgradeMap.Add(upgrade.id, upgrade);
        }
    }

    void ApplyUpgrade(string id)
    {

    }

    public bool IsUnlocked(string id)
    {
        return GameManager.Instance.CurrentData.unlockedUpgrades.Contains(id);
    }

    public UpgradeState GetUpgradeState(UpgradeDefinition upgradeDefinition)
    {
        if (IsUnlocked(upgradeDefinition.id))
            return UpgradeState.Unlocked;

        if (upgradeDefinition.parent != null && !IsUnlocked(upgradeDefinition.parent.id))
            return UpgradeState.Locked;

        return UpgradeState.Available;
    }

    public bool TryPurchaseUpgrade(UpgradeDefinition upgradeDefinition)
    {
        if (GetUpgradeState(upgradeDefinition) != UpgradeState.Available
        || !ResourceManager.Instance.TryWithdrawGold(upgradeDefinition.Cost))
            return false;

        HubUIController.Instance.SetResource(ResourceManager.Instance.Resource);
        HubUIController.Instance.SetGold(ResourceManager.Instance.Gold);

        GameManager.Instance.CurrentData.unlockedUpgrades.Add(upgradeDefinition.id);

        ApplyUpgrade(upgradeDefinition.id);

        GameManager.Instance.SaveGame();
        return true;
    }

    public void ActivatePanel()
    {
        _upgradePanel.SetActive(true);
    }

    public void OnPanelCloseButton()
    {
        _upgradePanel.SetActive(false);
    }
}
