using UnityEngine;

public class Shotgun : MonoBehaviour, IWeapon
{
    [Header("Attack Status")]
    [SerializeField] float _attackCooldown;
    [SerializeField] float _reboundIntensity; // 쏘고 뒤로 밀려나는 정도
    [SerializeField] Transform _bulletSpawnPoint;

    [Header("Bullet Settings")]
    [SerializeField] GameObject _bulletPrefab;
    [SerializeField] BulletData _bulletData; // 탄환 대미지, 속도, 지속시간, 관통 해금 여부,
    [SerializeField] int _bulletCount; // 탄환 발사 개수
    [SerializeField] float _spreadAngle; // 탄퍼짐 각도 (조준 방향 위아래 부채꼴)
    [SerializeField] float _bulletSpeedVariation; // 탄환 속도 변위 값: 예를 들어 0.1이면 +-10%

    [Header("Rebound")]
    [SerializeField] SpacePlayerController _player;

    float _attackCooldownTimer = 0f;
    bool IsAttackable => _attackCooldownTimer <= 0f;

    void FixedUpdate()
    {
        if (_attackCooldownTimer > 0f) _attackCooldownTimer -= Time.fixedDeltaTime;
    }

    public void Initialize()
    {

    }

    public void Attack(Vector2 aimDirection)
    {
        if (!IsAttackable) return;

        for (int i = 0; i < _bulletCount; i++)
        {
            float speed = _bulletData.speed * (1f + Random.Range(-_bulletSpeedVariation, _bulletSpeedVariation));
            float angle = Random.Range(-_spreadAngle, _spreadAngle);

            Quaternion fireRotation = Quaternion.Euler(0, 0, angle);
            Vector2 fireDirection = fireRotation * aimDirection;

            GameObject bulletObject = Instantiate(_bulletPrefab, _bulletSpawnPoint.position, _bulletSpawnPoint.rotation * fireRotation);
            if (bulletObject.TryGetComponent<Bullet>(out var bullet))
            {
                bullet.Initialize(_bulletData, speed * fireDirection);
            }
        }

        _player.StartKnockback(-aimDirection, _reboundIntensity);

        _attackCooldownTimer = _attackCooldown;
    }
}
