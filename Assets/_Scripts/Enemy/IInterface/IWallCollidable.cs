public interface IWallCollidable
{
    // บังคับว่าใครก็ตามที่ใช้ Interface นี้ ต้องมีฟังก์ชัน OnHitWall
    void OnHitWall(float stunDuration);
}