using UnityEngine;

public class UpgradeNode
{
    private string _name;
    private string _description;
    private bool _isUnlocked;

    public string Name => _name;
    public string Description => _description;
    public bool IsUnlocked => _isUnlocked;
}

public class UpgradeManager : MonoBehaviour
{
    // Singleton
    public static UpgradeManager Instance;

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
    }
}
