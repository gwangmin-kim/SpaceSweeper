using System;

[Serializable]
public class BulletData
{
    public int damage;
    public float speed;
    public float duration;
    public bool isPenetrationUnlocked;
    public UnityEngine.LayerMask targetLayer;
}
