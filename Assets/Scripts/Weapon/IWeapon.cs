public interface IWeapon
{
    void Initialize(); // 업그레이드 내역을 가져와 파라미터 초기화
    void Attack(UnityEngine.Vector2 aimDirection);
}
