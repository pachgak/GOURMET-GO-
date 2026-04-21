using UnityEngine;

public class TreeWindSway : MonoBehaviour
{
    [Header("Wind Settings")]
    [Tooltip("ความเร็วในการพริ้วไหว")]
    public float swaySpeed = 2f;
    [Tooltip("องศาที่โยกไปซ้ายขวา (ยิ่งเยอะยิ่งโยกแรง)")]
    public float swayAmount = 3f;

    private Quaternion startRotation;
    private float randomOffset; // ตัวแปรใหม่สำหรับเก็บค่าสุ่มของแต่ละต้น

    void Start()
    {
        startRotation = transform.localRotation;

        // สุ่มตัวเลขขึ้นมาสักค่า (0 ถึง 100 ก็เหลือเฟือ) 
        // เพื่อให้ต้นไม้นี้มี "จุดเริ่มต้น" ของลมที่ไม่เหมือนต้นอื่น
        randomOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        // เอา randomOffset ไปบวกกับ Time.time 
        // ทำให้คลื่น Sine ของแต่ละต้นเหลื่อมกันแบบสุ่ม
        float angle = Mathf.Sin((Time.time + randomOffset) * swaySpeed) * swayAmount;

        transform.localRotation = startRotation * Quaternion.Euler(0, 0, angle);
    }
}