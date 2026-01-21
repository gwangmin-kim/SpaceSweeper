using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    [Header("Weapon Prefabs")]
    [SerializeField] GameObject _pickaxe;
    [SerializeField] GameObject _shotgun;
    [SerializeField] GameObject _lasergun;

    void Awake()
    {
        Instance = this;
    }

    public GameObject GetCurrentWeapon()
    {
        return GameManager.Instance.CurrentData.playerSpec.currentWeapon switch
        {
            WeaponType.Pickaxe => _pickaxe,
            WeaponType.Shotgun => _shotgun,
            WeaponType.Lasergun => _lasergun,
            _ => null,
        };
    }
}
