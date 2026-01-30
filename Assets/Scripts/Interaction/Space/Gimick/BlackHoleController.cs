using UnityEngine;

[System.Serializable]
public struct BlackholeData
{
    public float pullForce;
    public float scale;
    public float duration;
}

public class BlackHoleController : MonoBehaviour
{
    BlackholeData _data;

    float _timer;

    void Start()
    {
        var data = GameManager.Instance.CurrentData.gimickSpec.blackholeData;
        InitBlackhole(data);
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

    void InitBlackhole(BlackholeData data)
    {
        _data = data;
        _timer = _data.duration;

        transform.localScale *= _data.scale;

        InitPosition();
    }

    void InitPosition()
    {
        Camera camera = Camera.main;
        Vector2 randomViewportPosition = new Vector2(Random.Range(-0.5f, 1.5f), Random.Range(-0.5f, 1.5f));
        Vector2 worldPosition = camera.ViewportToWorldPoint(randomViewportPosition);

        transform.position = worldPosition;
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.TryGetComponent<IGimickAffectable>(out var target)) return;

        Vector2 direction = transform.position - collision.transform.position;

        // ? 나중에 거리에 따라 힘 조절하는 경우도 고려
        target.AddExternalVelocity(direction.normalized * _data.pullForce);
    }

    void OnDrawGizmos()
    {
        if (!TryGetComponent<CircleCollider2D>(out var collider)) return;

        Gizmos.color = Color.orangeRed;
        Gizmos.DrawWireSphere(transform.position, collider.radius * transform.localScale.x);
    }
}
