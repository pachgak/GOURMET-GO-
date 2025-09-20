using UnityEngine;
using TMPro; // อย่าลืมว่าคุณต้องมี TextMeshPro ในโปรเจกต์ด้วย

public class DamagePrompt : MonoBehaviour
{
    // ตัวแปรสาธารณะที่คุณสามารถตั้งค่าได้ใน Unity Inspector
    public float destroyTime = 2f; // ระยะเวลาที่ข้อความความเสียหายจะอยู่บนหน้าจอ
    public float risingSpeed = 1f; // ความเร็วพื้นฐานที่ข้อความความเสียหายจะลอยขึ้น
    public float randomXRange = 1f; // ช่วงการสุ่มในแกน X
    public float randomYRange = 1f; // ช่วงการสุ่มในแกน Y
    public Color damageColor = Color.red; // สีของข้อความความเสียหาย

    private Vector3 _offSet;
    private Transform _targetPos;
    public TMP_Text damageText;
    private Vector3 _randomDirection;

    void Awake()
    {
        // ตั้งค่าสีเริ่มต้น
        damageText.color = damageColor;

        // สุ่มทิศทางการเคลื่อนที่
        _randomDirection = new Vector3(Random.Range(-randomXRange, randomXRange), Random.Range(0, randomYRange), 0);

        // ทำลายวัตถุข้อความโดยอัตโนมัติหลังจากผ่านไปตามเวลาที่กำหนด
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // ทำให้ข้อความลอยขึ้นไปตามกาลเวลาด้วยทิศทางที่สุ่มไว้
        transform.position = _targetPos.position + _offSet;
        damageText.transform.position += (_randomDirection.normalized * risingSpeed + Vector3.up) * Time.deltaTime;

        // ค่อยๆ ทำให้ข้อความจางหายไป
        damageText.color = new Color(damageText.color.r, damageText.color.g, damageText.color.b, damageText.color.a - (Time.deltaTime / destroyTime));
    }

    // เมธอดสาธารณะสำหรับตั้งค่าข้อความความเสียหาย
    public void SetDamageText(float damageAmount, Vector3 offSet, Transform targetPos)
    {
        damageText.text = damageAmount.ToString();
        _offSet = offSet;
        _targetPos = targetPos;
    }
}