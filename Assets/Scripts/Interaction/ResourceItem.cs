using UnityEngine;

public class ResourceItem : MonoBehaviour
{
    [Header("Attract Options")]
    [SerializeField] float _standbyDuration; // 생성 직후 곧바로 끌려가지 않고 일정 시간 대기

    Transform _target;



    public void SetTarget(Transform target)
    {
        _target = target;
    }
}
