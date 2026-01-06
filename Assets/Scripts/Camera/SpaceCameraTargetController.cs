using UnityEngine;

public class SpaceCameraTargetController : MonoBehaviour
{
    [SerializeField] SpacePlayerController _player;
    [SerializeField] float _trackingFactor;
    [SerializeField] float _maxDistance;

    void Update()
    {
        Vector3 playerPosition = _player.transform.position;
        Vector3 mousePosition = _player.AimPosition;

        Vector3 targetPosition = Vector3.Lerp(playerPosition, mousePosition, _trackingFactor);

        Vector2 offset = targetPosition - playerPosition;
        offset = Vector2.ClampMagnitude(offset, _maxDistance);

        targetPosition = playerPosition + (Vector3)offset;
        targetPosition.z = 0f;

        transform.position = targetPosition;
    }

    private void OnDrawGizmos()
    {
        if (_player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_player.transform.position, _maxDistance);
            Gizmos.DrawSphere(transform.position, 0.05f);
        }
    }
}
