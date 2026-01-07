using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class SpacePlayerMagnet : MonoBehaviour
{
    CircleCollider2D _magnetArea;

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

    }
}
