using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class BonusPickaxeComponent : MonoBehaviour
{
    [SerializeField] Transform _visualRoot;
    [SerializeField] float _rotationAnglePerSecond;

    BonusPickaxeStat _stat;
    float _damage;
    float _criticalChance;
    float _criticalDamage;

    void OnEnable()
    {
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.OnComboChanged += SetComboBonus;
        }
    }

    void OnDisable()
    {
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.OnComboChanged -= SetComboBonus;
        }
    }

    void Update()
    {
        _visualRoot.Rotate(_rotationAnglePerSecond * Time.deltaTime * Vector3.forward);
    }

    public void Init(BonusPickaxeStat stat, float damage, float criticalChance, float criticalDamage)
    {
        _stat = stat;
        _damage = damage;
        _criticalChance = criticalChance;
        _criticalDamage = criticalDamage;

        transform.localScale = new Vector3(_stat.attackRadius, _stat.attackRadius, 1f);
        _visualRoot.localScale = Vector3.zero;
        _visualRoot.DOScale(Vector3.one, 1.0f).SetLink(gameObject);

        _visualRoot.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
    }

    void SetComboBonus(int levelIndex, float _)
    {
        var spec = GameManager.Instance.CurrentData.playerSpec.comboSpec;
        float bonusRangeRate = 1f + spec.pickaxeRangeBonus * levelIndex;

        transform.localScale = bonusRangeRate * new Vector3(_stat.attackRadius, _stat.attackRadius, 1f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Debug.Log($"collision detected: {collision}");
        if (collision.gameObject.TryGetComponent<IDamagable>(out var component))
        {
            Vector2 deltaPosition = collision.transform.position - transform.position;
            Vector2 knockbackDirection = deltaPosition.normalized;

            bool isCritical = Random.value < _criticalChance;
            float damage = _damage;
            if (isCritical) damage *= _criticalDamage;

            AttackInfo attackInfo = new AttackInfo
            {
                source = AttackerType.BonusWeapon,
                isCritical = isCritical,
                damage = damage,
                direction = knockbackDirection,
                knockbackIntensity = _stat.knockbackIntensity
            };

            component.ApplyAttack(attackInfo);
        }
    }
}
