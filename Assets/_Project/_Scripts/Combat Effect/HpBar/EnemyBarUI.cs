public class EnemyBarUI : BaseHpBarUI
{
    // Override ระบบซ่อนของ Base เพราะมอนสเตอร์ต้องคืน Object เข้า Pool
    public override void DisableBar()
    {
        base.DisableBar(); // เคลียร์ Event ก่อน
        ObjectPoolingManager.Instance.Respawn(this.gameObject); // ส่งกลับ Pool
    }
}