using UnityEngine;
using TMPro; // TMPro อาจไม่จำเป็น แต่คงไว้ถ้าใช้ในอนาคต

public class ItemDropMovement : MonoBehaviour
{
    [Header("Bouncing Settings")]
    private float _currentBounceForce; // ใช้เก็บแรงผลักสุ่มในแต่ละครั้ง
    [SerializeField] private float maxBounceForce = 5f; // แรงผลักสูงสุดที่สุ่มได้

    [Header("Direction Randomness")]
    [SerializeField] private float horizontalRandomness = 0.5f; // ค่าสุ่มสำหรับแกน X (ซ้าย/ขวา)
    [SerializeField] private float forwardRandomness = 0.5f; // ค่าสุ่มสำหรับแกน Z (หน้า/หลัง)
    [SerializeField] private float verticalLift = 1f;           // ค่าบังคับแรงดีดขึ้นในแกน Y

    [Header("Ground Check Settings")]
    [SerializeField] private LayerMask groundLayer;
    // ใช้ค่าเยอะเพื่อให้ Raycast ไม่พลาดการชนพื้น
    // ระยะห่างที่ไอเทมต้องเข้าใกล้พื้นผิวจริง ก่อนจะหยุดการเคลื่อนที่
    [SerializeField] private float raycastDistance = 0.1f;

    private Rigidbody rb;
    private Collider _collider;
    private bool hasBounced = false;

    private Item item;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        item = GetComponent<Item>();
        if (rb == null)
        {
            Debug.LogError("ItemDropMovement requires a Rigidbody component on the same GameObject.");
            enabled = false;
        }
    }

    private void OnEnable()
    {
        item.OnItemSetup += Bounce;
    }

    private void OnDisable()
    {
        item.OnItemSetup -= Bounce;
    }

    private void FixedUpdate()
    {
        // ตรวจสอบพื้นต่อเมื่อไอเทมถูกผลักออกไปแล้วเท่านั้น
        if (hasBounced)
        {
            CheckGround();
        }
    }

    /// <summary>
    /// ฟังก์ชันสำหรับเรียกใช้การกระดุ้งไอเทม (3D)
    /// </summary>
    public void Bounce()
    {
        StopMovement(false);
        // 1. สุ่มแรงผลัก (Bounce Force)
        // ใช้ค่า maxBounceForce เป็นค่าสูงสุดในการสุ่ม
        _currentBounceForce = Random.Range(maxBounceForce/2, maxBounceForce);

        // 2. สุ่มทิศทาง (X, Z) และกำหนดแรงดีดขึ้น (Y)

        // **แกน X (ซ้าย/ขวา):** สุ่มค่าระหว่าง -horizontalRandomness ถึง +horizontalRandomness
        float randomX = Random.Range(-horizontalRandomness, horizontalRandomness);

        // **แกน Z (หน้า/หลัง):** สุ่มค่าระหว่าง -forwardRandomness ถึง 0 หรือ +forwardRandomness
        // หากต้องการให้สุ่มได้ทั้งหน้าและหลัง: Random.Range(-forwardRandomness, forwardRandomness);
        // หากต้องการให้สุ่มแค่ไปด้านหน้า (บวก Z) หรือด้านหลัง (ลบ Z)
        float randomZ = Random.Range(-forwardRandomness, 0);

        // **แกน Y (ขึ้น):** บังคับให้เป็นบวกเสมอ
        float forcedY = verticalLift;

        // 3. สร้างเวกเตอร์ทิศทางรวม
        Vector3 bounceDirection = new Vector3(randomX, forcedY, randomZ);

        // 4. Normalize และผลักไอเทม
        Vector3 normalizedDirection = bounceDirection.normalized;

        rb.AddForce(normalizedDirection * _currentBounceForce, ForceMode.Impulse);

        // ตั้งค่าให้เริ่มตรวจสอบพื้น
        hasBounced = true;
    }

    private void CheckGround()
    {
        // ตำแหน่งเริ่มต้นของ Raycast: จุดกึ่งกลางของไอเทม
        Vector3 raycastOrigin = transform.position;
        RaycastHit hit;

        // 1. ยิง Raycast ยาวๆ ลงไป
        if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, raycastDistance, groundLayer))
        {
                StopMovement(true);
        }

        // (Optional) แสดง Raycast ใน Scene View
        Debug.DrawRay(raycastOrigin, Vector3.down * raycastDistance, Color.red);
    }

    private void StopMovement(bool shouldStop)
    {
        // ถ้า shouldStop เป็น True (หยุด)
        if (shouldStop)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            rb.isKinematic = true;
            hasBounced = false;
            
        }
        else // ถ้า shouldStop เป็น False (เริ่ม)
        {
            rb.isKinematic = false;
            hasBounced = true;
        }
    }
}