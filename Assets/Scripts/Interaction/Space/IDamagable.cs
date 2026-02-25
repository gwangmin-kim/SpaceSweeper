using UnityEngine;

public interface IDamagable
{
    void ApplyAttack(AttackInfo attackInfo);
    void ApplyKnockback(Vector2 direction, float intensity);
    bool IsAffectable();
}

public enum AttackerType
{
    Player,
    Debris,
    Gimick,
}

[System.Serializable]
public struct AttackInfo
{
    public AttackerType source;
    public bool isCritical;
    public float damage;

    public Vector2 direction;
    public float knockbackIntensity;
}
