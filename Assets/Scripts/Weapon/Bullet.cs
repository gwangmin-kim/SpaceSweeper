using System;
using UnityEngine;

[Serializable]
public class BulletData
{
    public int damage;
    public float speed;
    public float duration;
    public bool isPenetrationUnlocked;
    public LayerMask targetLayer;
}

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    Rigidbody2D _rigidbody;

    BulletData _bulletData;

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
        if ((((1 << collision.gameObject.layer) & _bulletData.targetLayer) != 0)
            && collision.TryGetComponent<IDamagable>(out var component))
        {
            component.TakeDamage(_bulletData.damage);

            if (!_bulletData.isPenetrationUnlocked)
            {
                Destroy(gameObject);
            }
        }
    }
}
