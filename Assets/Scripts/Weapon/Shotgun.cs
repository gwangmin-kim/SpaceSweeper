using UnityEngine;

[System.Serializable]
public class BulletData
{
    public int damage;
    public float speed;
    public float duration;
    public bool isPenetrationUnlocked;
}

[System.Serializable]
public class ShotgunStat
{
    public float cooldown;
    public float spreadAngle; // 탄퍼짐 각도 (조준 방향 위아래 부채꼴)
    public float reboundIntensity; // 쏘고 뒤로 밀려나는 정도

    public BulletData bulletData; // 탄환 대미지, 속도, 지속시간, 관통 해금 여부,
    public int bulletCount; // 탄환 발사 개수
    public float bulletSpeedVariation; // 탄환 속도 변위 값: 예를 들어 0.1이면 +-10%
}

public class Shotgun : MonoBehaviour, IWeapon
{
    [Header("Attack Status")]
    [SerializeField] ShotgunStat _stat;
    [SerializeField] Transform _bulletSpawnPoint;
    [SerializeField] GameObject _bulletPrefab;

    float _attackCooldownTimer = 0f;
    bool IsAttackable => _attackCooldownTimer <= 0f;

    void FixedUpdate()
    {
        if (_attackCooldownTimer > 0f) _attackCooldownTimer -= Time.fixedDeltaTime;
    }

    public void Initialize()
    {
        _stat = GameManager.Instance.CurrentData.playerSpec.shotgunStat;
    }

    public void Attack(Vector2 aimDirection)
    {
        if (!IsAttackable) return;

        for (int i = 0; i < _stat.bulletCount; i++)
        {
            float speed = _stat.bulletData.speed *
                (1f + Random.Range(-_stat.bulletSpeedVariation, _stat.bulletSpeedVariation));
            float angle = Random.Range(-_stat.spreadAngle, _stat.spreadAngle);

            Quaternion fireRotation = Quaternion.Euler(0, 0, angle);
            Vector2 fireDirection = fireRotation * aimDirection;

            GameObject bulletObject = Instantiate(_bulletPrefab, _bulletSpawnPoint.position, _bulletSpawnPoint.rotation * fireRotation);
            if (bulletObject.TryGetComponent<Bullet>(out var bullet))
            {
                bullet.Initialize(_stat.bulletData, speed * fireDirection);
            }
        }

        Debug.DrawLine(_bulletSpawnPoint.position, 10f * _stat.bulletData.duration * aimDirection, Color.yellowGreen, 0.5f);

        SpacePlayerController.Instance.ApplyKnockback(-aimDirection, _stat.reboundIntensity);

        _attackCooldownTimer = _stat.cooldown;
    }
}
