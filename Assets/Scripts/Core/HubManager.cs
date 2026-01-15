using UnityEngine;

public class HubManager : MonoBehaviour
{
    // Singleton
    public static HubManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] HubPlayerController _playerController;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
