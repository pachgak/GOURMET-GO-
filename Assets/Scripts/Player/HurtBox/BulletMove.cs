using UnityEngine;

public class BulletMove : MonoBehaviour , ISpeed
{
    // ความเร็วของกระสุน
    public float speed = 20f;

    // ระยะเวลาที่กระสุนจะทำลายตัวเอง
    public float destroyTime = 10f;

    float ISpeed._speed { get => speed; set => speed = value; }

    void Start()
    {
        // สั่งให้กระสุนทำลายตัวเองเมื่อผ่านไปตามเวลาที่กำหนด
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // ทำให้กระสุนเคลื่อนที่ไปข้างหน้าอย่างต่อเนื่องในทิศทาง Z-axis ของมัน
        // โดยจะปรับความเร็วให้คงที่ตาม Time.deltaTime
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}