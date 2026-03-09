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
    public void OnHitWall(float stunDuration, float bounceForce, float bounceTime)
    {
        Debug.Log("Hog ชนกำแพง! เด้งถอยหลังและติดสตัน!");

        // 1. สั่งหยุด Animation ทันที
        FinishSkillAnimation();

        // 2. จัดการเรื่องการเคลื่อนที่
        if (_enemyMovement != null)
        {
            // เบรกหัวทิ่มก่อน
            _enemyMovement.StopDashImmediately();

            // *** แก้ปัญหาข้อ 1: ใช้ currentDiractionSkill แทน transform.forward ***
            // เนื่องจากเกมเป็น 2.5D เราจึงเอา "ทิศทางที่ใช้ยิงสกิล" มากลับด้าน (-Vector) ซะเลย
            Vector3 backwardDir = -currentDiractionSkill.normalized;

            // *** แก้ปัญหาข้อ 2: ปลด Super Armor ***
            // สกิลถูกขัดจังหวะแล้ว (ชนกำแพง) เราต้องเปิดให้มันกลับมากระเด็นได้อีกครั้ง
            _enemyMovement.canKnockback = true;

            // สั่งให้กระเด็นถอยหลัง
            _enemyMovement.GetKnockedBack(backwardDir, bounceForce, bounceTime);
        }

        // 3. สั่งให้ AI เข้าสถานะ Stun
        if (_hogAI != null)
        {
            _hogAI.ApplyStun(stunDuration);
        }
    }
}