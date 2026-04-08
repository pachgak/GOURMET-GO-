using UnityEngine;
using UnityEngine.AI;

public class WaterSlimeAI : BaseEnemyAI
{
    [Header("Water Slime Behavior")]
    [SerializeField] private bool _isProvoked = false; // สถานะโกรธ

    // ----------------------------------------------------------------------
    // 1. การสมัครรับ Event (Subscription)
    // ----------------------------------------------------------------------
    protected override void OnEnable()
    {
        base.OnEnable(); // อย่าลืมเรียก Base เพื่อให้ Combat Event ทำงานด้วย

        // สมัครรับ Event เมื่อโดนดาเมจ
        if (_enemyHealth != null)
        {
            _enemyHealth.OnTakeDamage += HandleTakeDamage;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // ยกเลิกการสมัครเมื่อตัวละครถูกปิด (ป้องกัน Error)
        if (_enemyHealth != null)
        {
            _enemyHealth.OnTakeDamage -= HandleTakeDamage;
        }
    }

    // ----------------------------------------------------------------------
    // 2. Logic เมื่อโดนตี (Handler)
    // ----------------------------------------------------------------------
    private void HandleTakeDamage(float damage, GameObject customHitVFX = null)
    {
        // ถ้าโกรธอยู่แล้ว ก็ไม่ต้องทำอะไรเพิ่ม (หรืออาจจะรีเซ็ตเวลานับถอยหลังหายโกรธก็ได้)
        if (_isProvoked) return;

        // เริ่มโกรธ!
        _isProvoked = true;

        // *** Reaction ทันที ***
        // สั่งให้ AI คำนวณระยะใหม่ทันที ณ เฟรมนี้เลย เพื่อเปลี่ยน State
        CheckPlayerDistance();

        // ตัดสินใจเปลี่ยน State ทันที
        if (_playerInAttackRange)
        {
            // ระยะถึง -> ตีสวนเลย
            ChangeState(EnemyState.Attack);
        }
        else
        {
            // ระยะไม่ถึง -> วิ่งไล่
            ChangeState(EnemyState.Chase);
        }
    }

    // ----------------------------------------------------------------------
    // 3. Override Logic การมองเห็น (Passive Behavior)
    // ----------------------------------------------------------------------
    protected override void CheckPlayerDistance()
    {
        // ให้ Base คำนวณระยะทางจริงมาก่อน
        base.CheckPlayerDistance();

        // ถ้า "ไม่โกรธ" -> บังคับให้ AI ตาบอด (มองไม่เห็น Player)
        if (!_isProvoked)
        {
            _playerInSightRange = false;
            _playerInAttackRange = false;
        }
        // ถ้า "โกรธ" -> ใช้ค่าจริงจาก Base (จะทำให้เข้า Chase/Attack ได้ตามปกติ)
    }

    // ----------------------------------------------------------------------
    // 4. Override การเปลี่ยน State (Reset เมื่อเลิกตาม)
    // ----------------------------------------------------------------------
    protected override void ChangeState(EnemyState newState)
    {
        base.ChangeState(newState);

        //// ถ้า AI กลับสู่สถานะ Roaming (แปลว่าผู้เล่นหนีพ้น หรือเลิกตามแล้ว)
        //// ให้หายโกรธ กลับมาเป็นมอนสเตอร์เป็นกลางเหมือนเดิม
        //if (newState == EnemyState.Roaming)
        //{
        //    _isProvoked = false;
        //}
    }

    // เขียนทับ Logic ตอน Standby
    protected override void StandbyChangeStateLogic()
    {
        // สไลม์ใจร้อน พอเข้าโหมด Standby ปุ๊บ สั่งให้เด้งกลับไป Chase ทันที!
        // ทำให้มันไม่มีจังหวะยืนพักเลย
        ChangeState(EnemyState.Chase);
    }

    // Debug ดูสถานะ
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (Application.isPlaying)
        {
            // สีฟ้า = ใจดี, สีแดง = โกรธ
            Gizmos.color = _isProvoked ? Color.red : Color.cyan;
            Gizmos.DrawSphere(transform.position + Vector3.up * 2f, 0.5f);
        }
    }
}