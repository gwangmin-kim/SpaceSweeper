using UnityEngine;

public interface IMagneticStormAffectable
{
    /// <summary>
    /// 자기 폭풍에 의한 영향을 구현하는 함수
    /// </summary>
    /// <param name="velocity">자기 폭풍에 휩쓸려가는 속도</param>
    void ApplyMagneticStorm(Vector2 velocity);
}
