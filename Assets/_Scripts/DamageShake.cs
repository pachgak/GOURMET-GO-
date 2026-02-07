using UnityEngine;
using System.Collections;

public class DamageShake : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("ลาก script EnemyHealth มาใส่ตรงนี้ หรือถ้าอยู่ object เดียวกันมันจะหาเอง")]
    public EnemyHealth healthScript;

    [Header("Shake Settings")]
    public float duration = 0.2f;   // ระยะเวลาที่สั่น (วินาที)
    public float strength = 10f;    // ความแรงในการสั่น (องศา)

    private Quaternion originalRotation;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        // ถ้าไม่ได้ลากมาใส่ ให้ลองหาจาก component ในตัวเดียวกัน
        if (healthScript == null)
            healthScript = GetComponent<EnemyHealth>();

        // จำค่าการหมุนเริ่มต้นไว้ (เช่น ต้นไม้อาจจะเอียงอยู่นิดหน่อย)
        originalRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        // สมัครรับ Event เมื่อมีการโจมตี
        if (healthScript != null)
        {
            healthScript.OnTakeDamage += StartShake;
        }
    }

    private void OnDisable()
    {
        // ยกเลิกรับ Event เพื่อป้องกัน Error เมื่อ Object ถูกทำลาย/ปิด
        if (healthScript != null)
        {
            healthScript.OnTakeDamage -= StartShake;
        }
    }

    // ฟังก์ชันนี้จะถูกเรียกเมื่อ OnTakeDamage ทำงาน
    private void StartShake(float damage)
    {
        // ถ้ากำลังสั่นอยู่ ให้หยุดอันเก่าก่อน แล้วเริ่มใหม่ (Re-trigger)
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        
        shakeCoroutine = StartCoroutine(ShakeProcess());
    }

    IEnumerator ShakeProcess()
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // คำนวณค่าสั่นแบบสุ่ม ระหว่าง -strength ถึง +strength
            float zRotation = Random.Range(-1f, 1f) * strength;

            // ปรับการหมุนที่แกน Z (รักษาแกน X, Y เดิมไว้)
            // ใช้ localRotation เพื่อให้หมุนเทียบกับตัวเอง
            Vector3 currentRot = originalRotation.eulerAngles;
            transform.localRotation = Quaternion.Euler(currentRot.x, currentRot.y, originalRotation.eulerAngles.z + zRotation);

            elapsed += Time.deltaTime;

            yield return null; // รอเฟรมถัดไป
        }

        // เมื่อสั่นเสร็จ ให้กลับมาท่าเดิมเป๊ะๆ
        transform.localRotation = originalRotation;
        shakeCoroutine = null;
    }
}