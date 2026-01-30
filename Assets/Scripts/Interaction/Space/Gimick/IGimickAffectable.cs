using UnityEngine;

public interface IGimickAffectable
{
    // 외부 요인에 의해 속도에 영향을 받게 함
    void AddExternalVelocity(Vector2 velocity);
}
