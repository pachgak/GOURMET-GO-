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
            //float rotationAmount = -moveX * totalSpeed * rollSpeedMultiplier * Time.deltaTime;
            float rotationAmount = -moveX * totalSpeed * rollSpeedMultiplier * Time.deltaTime;

            // สั่งหมุน visualChild ไปเรื่อยๆ
            visualChild.Rotate(0, 0, rotationAmount);
        }

        //if (totalSpeed > 0.1f)
        //{
        //    // --- ส่วนที่แก้ไข ---

        //    // หาแค่ทิศทาง: ถ้า moveX เป็นบวก (ขวา) ให้ค่า 1, ถ้าลบ (ซ้าย) ให้ค่า -1
        //    // เราใช้ Mathf.Sign เพื่อเอาแค่ทิศ ไม่เอาขนาดของ X มาลดทอนความเร็ว
        //    float direction = Mathf.Sign(moveX);

        //    // แต่ถ้า moveX เป็น 0 (เดินตรงเป๊ะๆ) ให้ยึดทิศเดิม หรือบังคับเป็น 1 ก็ได้
        //    // ในที่นี้ถ้ามันเดินตรงดิ่ง (moveX แทบจะเป็น 0) เราอาจจะให้มันหมุนตามทิศล่าสุด
        //    // หรือถ้าเอาแบบง่ายคือ ให้มันหมุนทางใดทางหนึ่งไปเลย
        //    if (Mathf.Abs(moveX) < 0.01f) direction = 1f; 

        //    // สูตรใหม่:
        //    // Direction (1 หรือ -1) * ความเร็วรวม * ตัวคูณ
        //    // ใส่ลบ (-) ข้างหน้า direction เพื่อให้เดินขวาแล้วหมุนตามเข็ม (Roll Forward)
        //    float rotationAmount = -direction * totalSpeed * rollSpeedMultiplier * Time.deltaTime;

        //    visualChild.Rotate(0, 0, rotationAmount);
        //}
    }
}