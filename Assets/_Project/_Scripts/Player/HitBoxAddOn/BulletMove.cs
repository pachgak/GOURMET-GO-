using UnityEngine;

// เพิ่ม IDelayable เข้ามาเพื่อให้รองรับ Modifier ของเรา
public class BulletMove : MonoBehaviour, ISpeed, IDelayable
{
    [Header("Movement Settings")]
    [Tooltip("ความเร็วของกระสุน")]
    public float speed = 20f;

    [Header("Delay Settings")]
    [Tooltip("หน่วงเวลาก่อนเริ่มพุ่ง (วินาที)")]
    public float moveDelay = 0f;

    private float _timer;
    private bool _canMove = false;

    // Implement จาก ISpeed
    float ISpeed._speed { get => speed; set => speed = value; }

    private void OnEnable()
    {
        // รีเซ็ตค่าเวลาทุกครั้งที่ถูกดึงมาจาก Object Pool
        _timer = moveDelay;
        _canMove = (_timer <= 0); // ถ้าไม่ได้ตั้ง delay (คือ 0) ให้ขยับได้ทันที
    }

    void Update()
    {
        // 1. ระบบนับเวลาหน่วง
        if (!_canMove)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                _canMove = true; // หมดเวลาชาร์จ ปล่อยกระสุนพุ่ง!
            }
            return; // ยังขยับไม่ได้ ให้ออกจาก Update ไปก่อน
        }

        // 2. ทำให้กระสุนเคลื่อนที่ไปข้างหน้าอย่างต่อเนื่องในทิศทาง Z-axis ของมัน
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    // ===============================================
    // Implement จาก IDelayable (รองรับ DelayTimeModifier)
    // ===============================================
    public void SetDelayTime(float delay)
    {
        moveDelay = delay;
        // ถ้ากระสุนกำลังรอเวลาอยู่ ให้แก้เป็นเวลาใหม่ที่เพิ่งรับมาเลย
        if (!_canMove)
        {
            _timer = moveDelay;
        }
    }
}