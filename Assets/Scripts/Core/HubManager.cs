using UnityEngine;

public class HubManager : MonoBehaviour
{
    // Singleton
    public static HubManager Instance { get; private set; }

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
