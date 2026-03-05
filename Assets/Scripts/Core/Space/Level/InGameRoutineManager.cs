using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class InGameRoutineManager : MonoBehaviour
{
    public static InGameRoutineManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] Transform _mapRoot; // 맵이 생성될 부모 오브젝트
    [SerializeField] Transform _gimickHolder; // 기믹이 생성될 부모 오브젝트

    [Header("Camera Settings")]
    [SerializeField] CinemachineConfiner2D _cameraConfiner; // Cinamachine VCam
    [SerializeField] BoxCollider2D _cameraBoundsCollider; // Confiner Collider Object

    [Header("Gimick Target")]
    [SerializeField] LayerMask _gimickTargetLayer;

    LevelDefinition _currentLevel;
    Collider2D _currentSpawnZone;
    public Bounds CurrentMapBounds => (_currentSpawnZone != null) ? _currentSpawnZone.bounds : default;

    int _remainingDebrisCount = 0;

    public LayerMask GimickTargetLayer => _gimickTargetLayer;

    // 기믹 관리
    List<Coroutine> _runningGimicks = new List<Coroutine>();

    void Awake()
    {
        Instance = this;
    }

    void InitLevel()
    {
        // foreach (Transform child in _mapRoot) Destroy(child.gameObject);
        int currentLevelID = GameManager.Instance.CurrentData.currentLevelID;
        _currentLevel = LevelManager.Instance.GetLevel(currentLevelID);

        SessionManager.Instance.SetOxygenConsumption(_currentLevel.oxygenPerSecond);

        // 맵 프리팹(_currentLevel.mapPrefab) 안에 SpawnZone이라는 이름의 오브젝트를 포함시켜야 함.
        GameObject mapObject = Instantiate(_currentLevel.mapPrefab, _mapRoot);
        if (!mapObject.transform.Find("SpawnZone").TryGetComponent(out _currentSpawnZone))
        {
            Debug.LogWarning("Cannot find SpawnZone in map prefab");
            return;
        }

        _remainingDebrisCount = 0;
    }

    public void LoadLevel()
    {
        InitLevel();

        SpawnInitialObjects();

        SetCameraBounds();

        StartGimicks(_currentLevel.gimickList);
    }

    void SpawnInitialObjects()
    {
        var debrisSpec = GameManager.Instance.CurrentData.debrisSpec;

        for (int i = 0; i < _currentLevel.resourceSpawnData.count; i++)
        {
            SpawnSingleResource();
        }

        foreach (var spawnData in _currentLevel.debrisList)
        {
            int count = (int)(spawnData.count * debrisSpec.spawnRate);
            for (int i = 0; i < count; i++)
            {
                SpawnSingleDebris(spawnData.debrisPrefab);
            }
        }
    }

    void SetCameraBounds()
    {
        Bounds targetBounds = CurrentMapBounds;

        // collider 크기 맞추기
        _cameraBoundsCollider.size = targetBounds.size;
        _cameraBoundsCollider.offset = targetBounds.center;
        // _cameraBoundsCollider.transform.position = targetBounds.center;

        _cameraConfiner.BoundingShape2D = _cameraBoundsCollider;
        _cameraConfiner.InvalidateBoundingShapeCache();
    }

    public void SpawnSingleResource()
    {
        Vector2 spawnPosition = GetRandomPositionInSpawnZone();

        ResourceItem item = ResourcePoolManager.Instance.Get();
        item.transform.position = spawnPosition;

        Vector2 floatingDirection = Random.insideUnitCircle.normalized;
        item.InitFloating(floatingDirection);
    }

    void SpawnSingleDebris(GameObject debrisPrefab)
    {
        Vector2 spawnPosition = GetRandomPositionInSpawnZone();

        GameObject debrisObject = Instantiate(debrisPrefab, _mapRoot);
        debrisObject.transform.position = spawnPosition;

        if (!debrisObject.TryGetComponent<SpaceDebris>(out var debris))
        {
            Debug.LogWarning($"{debrisObject} is not a SpaceDebris object");
            return;
        }

        Vector2 floatingDirection = Random.insideUnitCircle.normalized;
        debris.InitMovement(floatingDirection);

        _remainingDebrisCount++;
    }

    public void SpawnSingleDebris(GameObject debrisPrefab, Vector2 spawnPosition, Vector2 initialVelocity)
    {
        GameObject debrisObject = Instantiate(debrisPrefab, _mapRoot);
        debrisObject.transform.position = spawnPosition;

        if (!debrisObject.TryGetComponent<SpaceDebris>(out var debris))
        {
            Debug.LogWarning($"{debrisObject} is not a SpaceDebris object");
            return;
        }

        debris.InitMovement(initialVelocity);

        _remainingDebrisCount++;
    }

    public void OnDebrisDestroy(GameObject debrisObject)
    {
        _remainingDebrisCount--;
        SessionManager.Instance.DestroyDebris();

        // 산소량 복구 로직
        var debrisSpec = GameManager.Instance.CurrentData.debrisSpec;
        if (debrisSpec.oxygenRestoreChance > 0f && Random.value < debrisSpec.oxygenRestoreChance)
        {
            SessionManager.Instance.RestoreOxygen(debrisSpec.oxygenRestoreAmount);
        }

        Destroy(debrisObject);
    }

    void StartGimicks(List<StageGimick> gimicks)
    {
        foreach (var gimickData in gimicks)
        {
            if (!gimickData.isEnabled) continue;

            Coroutine routine = StartCoroutine(GimickRoutine(gimickData));

            _runningGimicks.Add(routine);
        }
    }

    public void StopAllGimickRoutines()
    {
        foreach (var routine in _runningGimicks)
        {
            if (routine != null) StopCoroutine(routine);
        }
        _runningGimicks.Clear();
    }

    void SpawnGimick(GameObject prefab)
    {
        if (prefab == null) return;

        Instantiate(prefab, _gimickHolder);
    }

    IEnumerator GimickRoutine(StageGimick data)
    {
        while (true)
        {
            if (Random.value <= data.probability)
            {
                SpawnGimick(data.GimickPrefab);
            }

            float nextWaitTime = Random.Range(data.minInterval, data.maxInterval);
            if (nextWaitTime < 0.1f) nextWaitTime = 0.1f;

            yield return new WaitForSeconds(nextWaitTime);
        }
    }

    Vector2 GetRandomPositionInSpawnZone()
    {
        // Rejection Sampling
        // 성능을 위해선 SpawnZone이 Bounding box의 대부분 영역을 차지하도록 잡아야 함.
        Bounds targetBounds = _currentSpawnZone.bounds;
        Vector2 randomPosition;
        int safetyCount = 0;

        do
        {
            float x = Random.Range(targetBounds.min.x, targetBounds.max.x);
            float y = Random.Range(targetBounds.min.y, targetBounds.max.y);
            randomPosition = new Vector2(x, y);

            safetyCount++;
        }
        while (!_currentSpawnZone.OverlapPoint(randomPosition) && safetyCount < 100);

        return randomPosition;
    }

    void OnDrawGizmos()
    {
        // draw map bounds
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(CurrentMapBounds.center, CurrentMapBounds.size);
    }
}
