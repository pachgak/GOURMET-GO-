using UnityEngine;
using UnityEngine.AI;

public class CabbageAnimation : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public Transform visualChild;
    private NavMeshAgent _agent;

    [Header("Rolling Settings")]
    [Tooltip("ความไวในการหมุน (ยิ่งเลขเยอะ ยิ่งหมุนติ้วๆ)")]
    public float rollSpeedMultiplier = 300f;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        // กันลืมใส่ visualChild
        if (visualChild == null && spriteRenderer != null)
            visualChild = spriteRenderer.transform;
    }

    void Update()
    {
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        // 1. ดึงความเร็วในพิกัดโลก
        Vector3 worldVelocity = _agent.velocity;
        float totalSpeed = worldVelocity.magnitude;

        // 2. แปลงเป็น Local Velocity เพื่อดูทิศทาง (ซ้าย/ขวา)
        Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity.normalized);
        float moveX = localVelocity.x;

        // 3. FlipX (คงเดิมไว้ตามที่คุณเขียน)
        // หมายเหตุ: ถ้ากลิ้ง 360 องศา flipX อาจจะไม่จำเป็นเท่าไหร่ แต่ใส่ไว้ก็ไม่เสียหายครับ
        //if (moveX > 0) spriteRenderer.flipX = true;
        //if (moveX < 0) spriteRenderer.flipX = false;

        // 4. *** คำนวณการหมุน (Rolling Logic) ***
        // ถ้ามีความเร็ว (เคลื่อนที่อยู่)
        if (totalSpeed > 0.1f)
        {
            // สูตร: ความเร็ว * ตัวคูณ * ทิศทาง * เวลา
            // moveX บอกทิศทาง (-1 คือซ้าย, 1 คือขวา)
            // เครื่องหมายลบ (-) ใส่เพื่อให้หมุนถูกต้องตามฟิสิกส์ (เดินขวา ต้องหมุนลบ/ตามเข็ม)
            float rotationAmount = -moveX * totalSpeed * rollSpeedMultiplier * Time.deltaTime;

            // สั่งหมุน visualChild ไปเรื่อยๆ
            visualChild.Rotate(0, 0, rotationAmount);
        }
    }
}