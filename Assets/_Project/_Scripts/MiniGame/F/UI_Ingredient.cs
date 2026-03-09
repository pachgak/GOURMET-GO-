using System;
using UnityEngine;

public class UI_Ingredient : MonoBehaviour
{
    [Header("Physics Settings")]
    public float gravity = 2500f; // แรงโน้มถ่วงจำลอง ดึงแกน Y ลง (ยิ่งเยอะยิ่งตกเร็ว)
    public float bottomYLimit = -800f; // ตำแหน่งขอบจอด้านล่าง (ถ้าต่ำกว่านี้คือตกจอ)

    private RectTransform rectTransform;
    private Vector2 currentVelocity;

    // Property ให้ Manager เรียกดู RectTransform ได้ง่ายๆ
    public RectTransform Rect => rectTransform;

    // ประกาศ Action ที่ส่งค่า UI_Ingredient ออกไปด้วย
    public Action<UI_Ingredient> OnMissTarget;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // ฟังก์ชันรับค่าความเร็วต้น จาก Manager ตอนโดนเสก
    public void SetVelocity(Vector2 startVelocity)
    {
        currentVelocity = startVelocity;
    }

    void Update()
    {
        // 1. จำลองแรงโน้มถ่วง (หักลบความเร็วแกน Y ลงเรื่อยๆ ตามเวลา)
        currentVelocity.y -= gravity * Time.deltaTime;

        // 2. เคลื่อนที่แบบโปรเจกไทล์ (อัปเดตตำแหน่งจากความเร็ว)
        rectTransform.anchoredPosition += currentVelocity * Time.deltaTime;

        // 3. เช็คว่าหลุดออกนอก Canvas ด้านล่างหรือยัง
        if (rectTransform.anchoredPosition.y < bottomYLimit)
        {
            MissTarget();
        }
    }

    void MissTarget()
    {
        OnMissTarget?.Invoke(this);

        // ลบตัวเองทิ้ง
        Destroy(gameObject);
    }

    public void DestroySelf()
    {
        // ตรงนี้คุณสามารถใส่โค้ดเล่น Effect ระเบิด หรือ Animation แตกกระจายได้ก่อนลบตัวเอง
        Destroy(gameObject);
    }
}