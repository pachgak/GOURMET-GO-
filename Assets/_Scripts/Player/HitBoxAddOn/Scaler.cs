using UnityEngine;

public class Scaler : MonoBehaviour, ISpeed
{
    // กำหนดความเร็วในการขยาย
    // คุณสามารถปรับค่านี้ได้ใน Inspector ของ Unity
    public float speed = 20f;
    public Vector3 originScal = Vector3.zero;

    float ISpeed._speed { get => speed; set => speed = value; }

    private void Awake()
    {
        originScal = transform.localScale;
    }

    private void OnEnable()
    {
        transform.localScale = originScal;
    }

    void Update()
    {
        // 1. ดึงค่า Scale ปัจจุบันของวัตถุ
        Vector3 currentScale = transform.localScale;

        // 2. คำนวณค่าที่จะเพิ่มเข้าไปใน Scale
        // Time.deltaTime คือเวลาที่ผ่านไประหว่างเฟรมล่าสุดและเฟรมปัจจุบัน
        // การคูณด้วย Time.deltaTime ทำให้การเปลี่ยนแปลงเป็นไปอย่างราบรื่นและไม่ขึ้นอยู่กับ Framerate
        float deltaScale = speed * Time.deltaTime;

        // 3. ปรับเปลี่ยนค่า Scale สำหรับแกน X และ Z
        currentScale.x += deltaScale;
        currentScale.z += deltaScale;

        // 4. กำหนดค่า Scale ใหม่ให้กับวัตถุ
        transform.localScale = currentScale;
    }
}