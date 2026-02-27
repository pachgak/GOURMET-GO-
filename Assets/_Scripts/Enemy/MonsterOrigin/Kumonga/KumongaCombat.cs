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
    public void OnHitWall(float stunDuration, float bounceForce, float bounceTime)
    {
        Debug.Log("Kumonga ชนกำแพงเต็มๆ!");

        // 1. สั่งหยุด Animation Skill (เช่น ท่าวิ่ง) ทันที
        FinishSkillAnimation();

        // สั่งหยุด Dash (ถ้ามีคำสั่งใน Movement ให้เรียกหยุด)
        if (TryGetComponent(out BaseEnemyMovement movement))
        {
            // ถ้าคุณมีฟังก์ชัน StopDash() ก็เรียกตรงนี้ เพื่อไม่ให้มันไถลต่อ
             movement.StopDashImmediately();

            // *** ทีเด็ด: คำนวณทิศทางถอยหลัง (ตรงข้ามกับที่หันหน้าอยู่) ***
            Vector3 backwardDir = -transform.forward;

            // สั่งให้กระเด็นถอยหลัง โดยใช้ระบบ Knockback ที่คุณมีอยู่แล้ว!
            movement.GetKnockedBack(backwardDir, bounceForce, bounceTime);
        }

        // สั่งให้ AI ติดสตัน
        if (_ai != null)
        {
            _ai.ApplyStun(stunDuration);
        }
    }
}