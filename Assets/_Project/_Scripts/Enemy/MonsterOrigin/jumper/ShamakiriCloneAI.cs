using UnityEngine;

public class ShamakiriCloneAI : BaseEnemyAI
{
    // =========================================================
    // 1. ล้างสมองตอนเกิด
    // =========================================================
    protected override void Start()
    {
        ChangeState(EnemyState.Standby);
    }

    // =========================================================
    // 2. ล้างสมองตอนรอดูเชิง (Standby)
    // =========================================================
    protected override void StandbyChangeStateLogic()
    {
        // ปล่อยว่างไว้ ให้ ShamakiriSquadController คุมการเดินค่ายกล 100%
    }

    // =========================================================
    // 3. คืนสมองตอนกำลังวิ่งไล่ (Chase) *** แก้ไขตรงนี้ครับ ***
    // =========================================================
    protected override void ChaseChangeStateLogic()
    {
        // ถ้าวิ่งเข้ามาระยะถึงแล้ว (เช็คจาก Attack Range ใน Inspector) ให้สับเลย!
        if (_playerInAttackRange)
        {
            ChangeState(EnemyState.Attack);
        }
        // ถ้ายังไม่ถึงระยะ ก็วิ่งหน้าตั้งเข้าหาผู้เล่นต่อไป
        else if (playerTarget != null)
        {
            TriggerStartChase(playerTarget.position);
        }
    }

    // =========================================================
    // 4. ล้างสมองตอนตีเสร็จ
    // =========================================================
    protected override void HandleAttackFinished()
    {
        // ตีเสร็จปุ๊บ บังคับให้กลับมารอคำสั่ง (ค่ายกล 3 เหลี่ยม) เสมอ
        ChangeState(EnemyState.Standby);
    }
}