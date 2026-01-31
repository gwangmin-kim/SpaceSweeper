using UnityEngine;

public interface IBlackholeAffectable
{
    /// <summary>
    /// 블랙홀에 의한 영향을 구현하는 함수
    /// </summary>
    /// <param name="velocity">블랙홀에 의해 빨려들어가는 기준 속도</param>
    void ApplyBlackhole(Vector2 velocity);
}
