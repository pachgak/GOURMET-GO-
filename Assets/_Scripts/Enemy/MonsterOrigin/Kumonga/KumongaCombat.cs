using UnityEngine;

public class KumongaCombat : BaseEnemyCombat , IWallCollidable// สืบทอดจากคลาสแม่ที่คุณมี
{
    private KumongaAI _ai;

    protected override void Awake()
    {
        base.Awake();
        _ai = GetComponent<KumongaAI>();
    }

    // เมื่อ Hitbox (เช่น SpawnWallHitStunAction) ชนกับ Layer "Wall" มันจะเรียกฟังก์ชันนี้
    public void OnHitWall(float stunDuration)
    {
        Debug.Log("Kumonga ชนกำแพงเต็มๆ!");

        // สั่งให้ AI ติดสตัน
        if (_ai != null)
        {
            _ai.ApplyStun(stunDuration);
        }

        // สั่งหยุด Dash (ถ้ามีคำสั่งใน Movement ให้เรียกหยุด)
        if (TryGetComponent(out BaseEnemyMovement movement))
        {
            // ถ้าคุณมีฟังก์ชัน StopDash() ก็เรียกตรงนี้ เพื่อไม่ให้มันไถลต่อ
            // movement.StopDash(); 
        }
    }
}