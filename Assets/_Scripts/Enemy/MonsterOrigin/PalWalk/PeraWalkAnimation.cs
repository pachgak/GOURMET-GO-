using UnityEngine;
using UnityEngine.AI;

public class PeraWalkAnimation : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    private NavMeshAgent _agent;

    [Header("Walk Animation Settings")]
    [Tooltip("ความเร็วรอบของจังหวะการเดิน")]
    public float walkRhythmSpeed = 10f;

    [Tooltip("ความสูงที่เด้งขึ้น (แกน Y)")]
    public float bobAmount = 0.2f;

    [Tooltip("องศาการเอียงซ้ายขวา (แกน Z)")]
    public float tiltAngle = 5f;

    [Tooltip("ความนุ่มนวลตอนหยุดเดิน")]
    public float stopSmoothing = 5f;

    private float _timer;
    private Vector3 _defaultSpritePos;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        // จำค่าตำแหน่งเริ่มต้นของ Sprite (Local Position) ไว้ เพื่อให้เด้งจากจุดนี้
        if (spriteRenderer != null)
        {
            _defaultSpritePos = spriteRenderer.transform.localPosition;
        }
    }

    void Update()
    {
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        Vector3 worldVelocity = _agent.velocity;
        float totalSpeed = worldVelocity.magnitude;

        // --- Logic การเดิน (เด้ง + เอียง) ---
        if (totalSpeed > 0.1f) // ถ้ามีการเคลื่อนที่
        {
            // เพิ่มเวลาตามความเร็วที่เราตั้งไว้
            _timer += Time.deltaTime * walkRhythmSpeed;

            // 1. คำนวณการเด้ง (Bobbing Y)
            // ใช้ Mathf.Abs เพื่อให้ค่าเป็นบวกเสมอ (0 ถึง 1) คือเด้งขึ้นอย่างเดียว ไม่จมดิน
            float newY = _defaultSpritePos.y + Mathf.Abs(Mathf.Sin(_timer)) * bobAmount;

            // 2. คำนวณการเอียง (Tilting Z)
            // ใช้ Sin ธรรมดา (-1 ถึง 1) เพื่อให้เอียงซ้ายสลับขวา
            float rotZ = Mathf.Sin(_timer) * tiltAngle;

            // Apply ค่าไปที่ SpriteRenderer (ตัวลูก)
            spriteRenderer.transform.localPosition = new Vector3(_defaultSpritePos.x, newY, _defaultSpritePos.z);
            spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, rotZ);
        }
        else // ถ้าหยุดเดิน
        {
            // Reset ค่า timer (เพื่อให้เริ่มก้าวใหม่สวยๆ ตอนออกเดินครั้งหน้า)
            _timer = 0;

            // ค่อยๆ คืนค่ากลับสู่ท่าปกติ (Lerp) เพื่อไม่ให้หยุดกึกทันที
            spriteRenderer.transform.localPosition = Vector3.Lerp(spriteRenderer.transform.localPosition, _defaultSpritePos, Time.deltaTime * stopSmoothing);
            spriteRenderer.transform.localRotation = Quaternion.Lerp(spriteRenderer.transform.localRotation, Quaternion.identity, Time.deltaTime * stopSmoothing);
        }

        // --- Logic การหันหน้า (Flip) ---
        Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity.normalized);
        float moveX = localVelocity.x;

        // เช็คว่ามีความเร็วพอสมควรค่อยหัน (กันกระพริบ)
        if (totalSpeed > 0.1f)
        {
            if (moveX > 0.1f) spriteRenderer.flipX = true;
            else if (moveX < -0.1f) spriteRenderer.flipX = false;
        }
    }
}