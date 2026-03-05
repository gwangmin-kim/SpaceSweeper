using UnityEngine;

[System.Serializable]
public class BonusPickaxeStat
{
    public bool unlocked;
    public int count;
    public float rotationRadius;
    public float attackRadius;
    public float rotationAnglePerSecond;
    public float knockbackIntensity;
}

public class BonusPickaxeController : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] BonusPickaxeStat _stat;
    [SerializeField] GameObject _prefab;
    [SerializeField] float _damage;
    [SerializeField] float _criticalChance;
    [SerializeField] float _criticalDamage;

    void Start()
    {
        _stat = GameManager.Instance.CurrentData.playerSpec.bonusPickaxeStat;
        if (!_stat.unlocked) return;

        Init();
    }

    void Init()
    {
        var pickaxeStat = GameManager.Instance.CurrentData.playerSpec.pickaxeStat;
        _damage = pickaxeStat.attackDamage;
        _criticalChance = pickaxeStat.criticalChance;
        _criticalDamage = pickaxeStat.criticalDamage;

        // spawn
        float unitAngle = 2f * Mathf.PI / _stat.count;
        for (int i = 0; i < _stat.count; i++)
        {
            float angle = unitAngle * i;
            Vector2 localPosition = _stat.rotationRadius * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            GameObject pickaxeObject = Instantiate(_prefab, transform);

            pickaxeObject.transform.SetLocalPositionAndRotation(localPosition, Quaternion.identity);
            pickaxeObject.GetComponent<BonusPickaxeComponent>().Init(_stat, _damage, _criticalChance, _criticalDamage);
        }
    }

    void Update()
    {
        transform.Rotate(_stat.rotationAnglePerSecond * Time.deltaTime * Vector3.forward);
    }
}
