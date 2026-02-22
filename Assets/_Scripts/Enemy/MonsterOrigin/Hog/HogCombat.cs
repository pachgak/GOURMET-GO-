using UnityEngine;

public class HogCombat : BaseEnemyCombat , IWallCollidable
{
    private HogAI _hogAI;

    protected override void Awake()
    {
        base.Awake();
        _hogAI = GetComponent<HogAI>();
    }

    // ฟังก์ชันนี้จะถูกเรียกโดย SpawnWallHitStunAction เมื่อ Hitbox ชนกำแพง
    public void OnHitWall(float stunDuration)
    {

        // 1. สั่งหยุด Animation Skill (เช่น ท่าวิ่ง) ทันที
        FinishSkillAnimation();

        // 2. สั่งหยุดการเคลื่อนที่ (Dash) ที่ Movement
        // (*** อย่าลืมเพิ่มฟังก์ชัน StopDashImmediately() ใน BaseEnemyMovement ตามที่คุยกันก่อนหน้านี้นะครับ ***)
        if (_enemyMovement != null)
        {
            _enemyMovement.StopDashImmediately();
        }

        // 3. สั่งให้ AI เข้าสถานะ Stun
        if (_hogAI != null)
        {
            _hogAI.ApplyStun(stunDuration);
        }
    }
}