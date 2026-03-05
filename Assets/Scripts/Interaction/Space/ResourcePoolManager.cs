using UnityEngine;
using UnityEngine.Pool;

public class ResourcePoolManager : MonoBehaviour
{
    public static ResourcePoolManager Instance { get; private set; }

    [SerializeField] ResourceItem _prefab;
    [SerializeField] int _defaultCapacity;
    [SerializeField] int _maxPoolSize;

    private IObjectPool<ResourceItem> _pool;

    void Awake()
    {
        Instance = this;

        int currentLevelID = GameManager.Instance.CurrentData.currentLevelID;
        var currentLevel = LevelManager.Instance.GetLevel(currentLevelID);
        _prefab = currentLevel.resourceSpawnData.resourcePrefab;

        // 풀 초기화 설정
        _pool = new ObjectPool<ResourceItem>(
            createFunc: CreateItem,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroyItem,
            collectionCheck: true, // helps catch double-release mistakes
            defaultCapacity: _defaultCapacity,
            maxSize: _maxPoolSize
        );
    }

    // 1. 실제 프리팹 생성
    ResourceItem CreateItem()
    {
        var item = Instantiate(_prefab, transform);
        item.SetPool(_pool); // 파편에게 자기가 속한 풀을 알려줌
        return item;
    }

    // 2. 풀에서 꺼낼 때 (활성화)
    void OnGet(ResourceItem item)
    {
        item.gameObject.SetActive(true);
    }

    // 3. 풀로 돌려보낼 때 (비활성화)
    void OnRelease(ResourceItem item)
    {
        item.gameObject.SetActive(false);
    }

    // 4. 풀 용량 초과 시 실제 파괴
    void OnDestroyItem(ResourceItem item)
    {
        Destroy(item.gameObject);
    }

    // 외부에서 파편을 빌려갈 때 쓰는 함수
    public ResourceItem Get() => _pool.Get();
}
