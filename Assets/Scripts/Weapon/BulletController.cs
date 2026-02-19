using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class BulletController : MonoBehaviour
{
    Rigidbody2D _rigidbody;

    [SerializeField] BulletData _bulletData;
    LayerMask TargetLayer => SpacePlayerController.Instance.TargetLayer;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 0f;
    }

    public void Initialize(BulletData bulletData, Vector2 velocity)
    {
        _bulletData = bulletData;
        _rigidbody.linearVelocity = velocity;

        // 지속 시간 후 파괴
        Destroy(gameObject, _bulletData.duration);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if ((((1 << collision.gameObject.layer) & TargetLayer) != 0)
            && collision.TryGetComponent<IDamagable>(out var component))
        {
            if (!component.IsAffectable()) return;

            component.TakeDamage(_bulletData.damage);

            if (!_bulletData.isPenetrationUnlocked)
            {
                Destroy(gameObject);
            }
        }
    }
}
