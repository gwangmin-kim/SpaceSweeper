using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class SpacePlayerMagnet : MonoBehaviour
{
    CircleCollider2D _magnetArea;
    [SerializeField] LayerMask _resourceLayer;

    void Awake()
    {
        _magnetArea = GetComponent<CircleCollider2D>();
    }

    public void InitializeMagnet(float radius)
    {
        _magnetArea.radius = radius;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & _resourceLayer) != 0 && collision.TryGetComponent<ResourceItem>(out var item))
        {
            item.SetTarget(transform);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.softRed;
        float radius = (_magnetArea == null) ? 0f : _magnetArea.radius;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
