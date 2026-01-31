using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MagneticStormData
{
    public float force;
    public float width;
    public float duration;
    public List<DebrisSpawnData> debrisSpawnList;
}

public class MagneticStormController : MonoBehaviour
{
    [SerializeField] Transform _spawnPoint;

    MagneticStormData _data;

    float _timer;

    void Start()
    {
        var data = GameManager.Instance.CurrentData.gimickSpec.magneticStormData;
        InitMagneticStorm(data);
    }

    void Update()
    {
        if (_timer > 0f)
        {
            _timer -= Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitMagneticStorm(MagneticStormData data)
    {
        _data = data;
        _timer = _data.duration;

        Vector3 localScale = transform.localScale;
        localScale.x *= _data.width;
        localScale.y *= 100f;
        transform.localScale = localScale;

        InitPosition();
        InitDirection();

        SpawnDebris();
    }

    void InitPosition()
    {
        Camera camera = Camera.main;
        Vector2 randomViewportPosition = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        Vector2 worldPosition = camera.ViewportToWorldPoint(randomViewportPosition);

        transform.position = worldPosition;
    }

    void InitDirection()
    {
        Vector2 direction = Random.insideUnitCircle.normalized;
        transform.up = direction;
    }

    void SpawnDebris()
    {
        foreach (var spawnData in _data.debrisSpawnList)
        {
            for (int i = 0; i < spawnData.count; i++)
            {
                StageManager.Instance.SpawnSingleDebris(
                    spawnData.debrisPrefab, _spawnPoint.position, Vector2.zero);
            }
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IMagneticStormAffectable>(out var target))
        {
            Vector2 direction = transform.up;

            target.ApplyMagneticStorm(direction * _data.force);
        }
    }

    void OnDrawGizmos()
    {
        if (!TryGetComponent<BoxCollider2D>(out var collider)) return;

        Gizmos.color = Color.orangeRed;
        Gizmos.DrawWireCube(transform.position, collider.size);
    }
}
