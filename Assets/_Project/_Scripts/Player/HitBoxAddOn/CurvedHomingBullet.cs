using UnityEngine;

public class CurvedHomingBullet : MonoBehaviour, ISpeed, ITargetable
{
    [Header("Phase 1: Linear Move (จังหวะที่ 1)")]
    public float baseSpeed = 10f; // ความเร็วเริ่มต้นตอนพุ่งตรง
    [Tooltip("เวลาที่จะพุ่งตรงไปก่อนจะเริ่มเลี้ยวโค้ง")]
    public float linearDuration = 0.5f;

    [Header("Phase 2: Curved Homing (จังหวะที่ 2)")]
    [Tooltip("ความเร็วตอนเลี้ยวกลับ")]
    public float homingSpeed = 25f;
    [Tooltip("ความเร็วในการเลี้ยว (ยิ่งเยอะ วงแคบ)")]
    public float turnSpeed = 30f;
    [Tooltip("ใส่ Transform ของ Player หรือเป้าหมายที่นี่")]
    public Transform target;

    // Implement ISpeed
    public float _speed { get => currentSpeed; set => baseSpeed = value; }

    private float currentSpeed;
    private float stateTimer;
    private Vector3 currentVelocity;

    private enum BulletState { Linear, Homing }
    private BulletState currentState;

    private void OnEnable()
    {
        // 1. Initialize ค่าเริ่มต้น
        currentState = BulletState.Linear;
        stateTimer = linearDuration;
        currentSpeed = baseSpeed;

        // *** ลบการหา currentVelocity ออกจากตรงนี้ เพื่อไปรอรับค่าจาก Update แทน (แก้ปัญหา Pooling) ***
    }

    private void Update()
    {
        // 2. จัดการสถานะการเคลื่อนที่
        if (currentState == BulletState.Linear)
        {
            // ดึงทิศทางจากหน้ากระสุนล่าสุด (ซึ่งตอนนี้ถูก SpawnHitAction_M หมุนให้เรียบร้อยแล้ว)
            currentVelocity = transform.forward * currentSpeed;

            // จังหวะที่ 1: พุ่งตรงไปข้างหน้า
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0)
            {
                // หมดเวลาพุ่งตรง -> เปลี่ยนเป็นจังหวะที่ 2
                currentState = BulletState.Homing;
                currentSpeed = homingSpeed;
                currentVelocity = currentVelocity.normalized * currentSpeed;
            }
        }
        else if (currentState == BulletState.Homing)
        {
            // จังหวะที่ 2: เลี้ยวโค้งหาเป้าหมาย
            if (target != null)
            {
                // *** จุดแก้ไขที่ 1: ทำการ Flatten (แบน) เวกเตอร์ทิศทางเป้าหมาย ***
                // เราจะเอาเฉพาะค่าแกน X และ Z เพื่อหาทิศทางในแนวราบ (Y-axis only rotation)
                Vector3 bulletPos = transform.position;
                Vector3 targetPos = target.position;
                Vector3 flattenedDirection = new Vector3(targetPos.x - bulletPos.x, 0, targetPos.z - bulletPos.z).normalized;

                // คำนวณวงเลี้ยวจาก VectorMath (ใช้เวกเตอร์ที่แบนแล้ว)
                currentVelocity = VectorMath.Steering(currentVelocity, flattenedDirection, turnSpeed, Time.deltaTime);
            }
        }

        // 3. จัดการการหมุนหน้ากระสุน (LookRotation)
        if (currentVelocity != Vector3.zero)
        {
            // *** จุดแก้ไขที่ 2: ทำการ Flatten (แบน) เวกเตอร์ความเร็วตอนหมุนหน้ากระสุน ***
            // เพื่อให้กระสุนดูเชิดตรงเสมอ ไม่ก้มหน้าลงไปหาเป้าหมายที่อยู่ต่ำกว่า หรือเงยหน้าขึ้นหาเป้าหมายที่อยู่สูงกว่า
            Vector3 lookVelocity = currentVelocity;
            lookVelocity.y = 0; // บังคับให้หน้ามองตรงเสมอ

            if (lookVelocity != Vector3.zero) // กันเหนียว เผื่อกรณี X และ Z เป็น 0 พร้อมกัน
            {
                transform.rotation = Quaternion.LookRotation(lookVelocity);
            }
        }

        // 4. สั่งเคลื่อนที่จริง (ตัวกระสุนจะรักษาระดับความสูง (Height) ของมันไว้ เพราะ steering บังคับแค่ซ้าย/ขวา)
        transform.position += currentVelocity * Time.deltaTime;
    }

    // Implement จาก ITargetable
    public void SetTarget(Transform targetTransform)
    {
        this.target = targetTransform;
    }
}