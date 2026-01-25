using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] Transform _mapRoot; // 맵이 생성될 부모 오브젝트

    LevelDefinition _currentLevel;
    Collider2D _currentSpawnZone;

    int _remainingDebrisCount = 0;

    void Awake()
    {
        Instance = this;
    }

    void ClearLevel()
    {
        // foreach (Transform child in _mapRoot) Destroy(child.gameObject);
        _remainingDebrisCount = 0;
    }

    void FindSpawnZone()
    {
        GameObject mapObject = Instantiate(_currentLevel.mapPrefab, _mapRoot);

        // 맵 프리팹(_currentLevel.mapPrefab) 안에 SpawnZone이라는 이름의 오브젝트를 포함시켜야 함.
        if (!mapObject.transform.Find("SpawnZone").TryGetComponent(out _currentSpawnZone))
        {
            Debug.LogWarning("Cannot find SpawnZone in map prefab");
            return;
        }
    }

    void SpawnInitialDebris()
    {
        foreach (var spawnData in _currentLevel.debrisList)
        {
            for (int i = 0; i < spawnData.count; i++)
            {
                SpawnSingleDebris(spawnData.debrisPrefab);
            }
        }
    }

    public void LoadLevel(LevelDefinition level)
    {
        _currentLevel = level;

        ClearLevel();

        FindSpawnZone();

        SpawnInitialDebris();
    }

    public void SpawnSingleDebris(GameObject debrisPrefab)
    {
        Vector2 spawnPosition = GetRandomPositionInSpawnZone();

        GameObject debris = Instantiate(debrisPrefab, spawnPosition, Quaternion.identity);

        if (!debris.TryGetComponent<SpaceDebris>(out var component))
        {
            Debug.LogWarning($"{debris} is not a SpaceDebris object");
        }

        Vector2 floatingDirection = Random.insideUnitCircle.normalized;
        component.SetInitialMovement(floatingDirection);

        _remainingDebrisCount++;
    }

    public void OnDebrisDie(GameObject debrisObject)
    {
        _remainingDebrisCount--;

        Destroy(debrisObject);
    }

    Vector2 GetRandomPositionInSpawnZone()
    {
        Bounds bounds = _currentSpawnZone.bounds;
        Vector2 randomPosition = default;

        return randomPosition;
    }
}
