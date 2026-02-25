using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class LightningEffect : MonoBehaviour
{
    LineRenderer _lineRenderer;
    [Header("Lightning Settings")]
    [SerializeField] int _segments; // 번개가 꺾이는 지점 수
    [SerializeField] float _radius; // 폐기물 주변 반지름
    [SerializeField] float _jaggedness; // 꺾이는 정도
    [SerializeField] float _duration; // 지속 시간

    [Header("Width Settings")]
    [SerializeField] float _minWidth; // 최소 너비
    [SerializeField] float _maxWidth; // 최대 너비

    WaitForSeconds _wait;

    public void SetRadius(float radius)
    {
        _radius = radius;
    }

    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _wait = new WaitForSeconds(_duration);
    }

    public void GenerateLightning()
    {
        StopAllCoroutines();
        StartCoroutine(LightningRoutine());
    }

    IEnumerator LightningRoutine()
    {
        // 번개가 칠 때마다 너비를 랜덤하게 설정
        float randomWidth = Random.Range(_minWidth, _maxWidth);
        _lineRenderer.startWidth = randomWidth;
        _lineRenderer.endWidth = randomWidth;

        _lineRenderer.positionCount = _segments;

        // 원형을 따라 무작위 지점들을 연결하여 번개 형태 생성
        float angleStep = 360f / (_segments - 1);
        for (int i = 0; i < _segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            // 반지름에 무작위 오차를 주어 파지직거리는 느낌 표현
            float offset = Random.Range(-_jaggedness, _jaggedness);
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * (_radius + offset);
            _lineRenderer.SetPosition(i, pos);
        }

        yield return _wait; // 잠깐 보여주고
        _lineRenderer.positionCount = 0; // 선 지우기
    }
}
