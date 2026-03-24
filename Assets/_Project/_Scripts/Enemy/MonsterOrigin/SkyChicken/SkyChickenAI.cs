using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SkyChickenAI : BaseEnemyAI
{
    [Header("Sky Chicken - Kiting Settings")]
    public float idealDistance = 6f;      // ระยะห่างที่เหมาะสมที่สุด (อยากยืนตรงนี้)
    public float distanceBuffer = 1f;     // ค่าความคลาดเคลื่อน (ถ้าอยู่ห่าง 5-7 หน่วย จะยืนนิ่งๆ)
    public float retreatRadius = 4f;      // รัศมีการก้าวถอยหลัง

    [Header("360 Degree Scan Settings")]
    [Tooltip("ความละเอียดในการหมุนองศาหาทางหนี ยิ่งน้อยยิ่งแม่นยำ (เช่น 15 = เช็ค 24 ทิศทาง)")]
    public int angleStep = 15;

    private float _checkTimer;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void Update()
    {
        // ตาย หรือ สตัน หยุดทุกอย่าง
        if ((_enemyHealth != null && _enemyHealth.isDead) || IsStunned)
        {
            return;
        }

        base.Update();
    }

    // ======================================================================
    // *** Standby Logic + ระบบเช็คระยะหนี 360 องศาแบบจัดเต็ม ***
    // ======================================================================
    protected override void StandbyChangeStateLogic()
    {
        if (playerTarget == null) return;

        // 1. ถ้าคูลดาวน์สกิลเสร็จแล้ว ให้กลับไป Chase เพื่อเข้าตี
        if (_enemyCombat != null && _enemyCombat.attackTimer <= 0)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // 2. หน่วงเวลาการเช็คระยะ (เพื่อไม่ให้กินสเปคเครื่องมากเกินไป)
        _checkTimer -= Time.deltaTime;
        if (_checkTimer > 0) return;
        _checkTimer = 0.2f; // รีเซ็ตเวลาเช็ค

        float distToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // ==========================================
        // 3. Logic การรักษาระยะ
        // ==========================================

        // Case A: ผู้เล่นใกล้เกินไป -> กางอาณาเขต 360 องศาเพื่อหาทางหนีที่ดีที่สุด!
        if (distToPlayer < idealDistance - distanceBuffer)
        {
            Vector3 bestRetreatPoint = transform.position;
            float maxDistFromPlayer = 0f;
            bool foundSafeSpot = false;

            // ลูปเช็ค 360 องศารอบตัว เพื่อหาจุดที่ปลอดภัยและไกลผู้เล่นที่สุด
            for (int angle = 0; angle < 360; angle += angleStep)
            {
                // คำนวณทิศทาง (ทีละ angle องศา)
                Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                Vector3 testPoint = transform.position + (dir * retreatRadius);

                NavMeshHit hit;
                // เช็คว่าจุดสมมตินั้น อยู่บนพื้นที่ NavMesh ที่เดินได้หรือไม่ (ระยะคลาดเคลื่อน 1.0f)
                if (NavMesh.SamplePosition(testPoint, out hit, 1.0f, NavMesh.AllAreas))
                {
                    // วัดระยะจากจุดนั้น ไปหาผู้เล่น
                    float distFromTestPointToPlayer = Vector3.Distance(hit.position, playerTarget.position);

                    // เราต้องการจุดที่ "อยู่ห่างจากผู้เล่นมากที่สุด"
                    if (distFromTestPointToPlayer > maxDistFromPlayer)
                    {
                        maxDistFromPlayer = distFromTestPointToPlayer;
                        bestRetreatPoint = hit.position;
                        foundSafeSpot = true;
                    }
                }
            }

            // สรุปผลการสแกน 360 องศา
            if (foundSafeSpot)
            {
                // เจอจุดที่ปลอดภัยและห่างที่สุดแล้ว สั่งให้เดินไปจุดนั้นเลย
                TriggerStartChase(bestRetreatPoint);
            }
            else
            {
                // ถ้าสแกน 360 องศาแล้วไม่มีทางหนีเลย (โดนต้อนเข้ามุมอับ 100%) ให้ยืนนิ่งๆ ไว้
                TriggerStopMovement();
            }
        }
        // Case B: ผู้เล่นไกลเกินไป -> เดินเข้าไปหารักษาระยะ
        else if (distToPlayer > idealDistance + distanceBuffer)
        {
            TriggerStartChase(playerTarget.position);
        }
        // Case C: ระยะกำลังดี -> ยืนนิ่งๆ เตรียมร่ายสกิล
        else
        {
            TriggerStopMovement();
        }
    }
}