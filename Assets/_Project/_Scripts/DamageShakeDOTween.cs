using UnityEngine;
// สำคัญมาก! ต้องมีบรรทัดนี้เพื่อใช้ DOTween
using DG.Tweening;

public class DamageShakeDOTween : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Script เลือด ถ้าอยู่ object เดียวกันไม่ต้องลากใส่ก็ได้")]
    private EnemyHealth _healthScript;
    public Transform visualChild; // ลากตัว Graphics มาใส่

    [Header("DOTween Shake Settings")]
    [Tooltip("ระยะเวลาที่สั่น (วินาที)")]
    public float duration = 0.3f;

    [Tooltip("ความแรงและแกนที่จะสั่น สำหรับต้นไม้ ให้เน้นแกน Z (เช่น 0,0,15)")]
    public Vector3 strength = new Vector3(0f, 0f, 20f);

    [Tooltip("ความถี่ในการสั่น ยิ่งเยอะยิ่งสั่นยิกๆ (ค่าปกติประมาณ 10-20)")]
    public int vibrato = 15;

    [Tooltip("ความสุ่มของการสั่น (0-90) ถ้า 0 จะสั่นไปทางเดียว ถ้า 90 จะสั่นมั่วทิศทาง")]
    [Range(0, 90)]
    public float randomness = 45f;

    private Tweener currentTweener;
    private Quaternion initialRotation;

    private void Awake()
    {
        // หา script เลือดอัตโนมัติถ้าไม่ได้ใส่มา
        if (_healthScript == null)
            _healthScript = GetComponent<EnemyHealth>();

        if (visualChild == null)
        {
            // ลองหาจาก SpriteRenderer ในลูกๆ
            SpriteRenderer spriteInChild = GetComponentInChildren<SpriteRenderer>();

            if (spriteInChild != null)
            {
                visualChild = spriteInChild.transform;
            }
            else
            {
                // ถ้าหาไม่เจอจริงๆ ค่อยใช้ท่าไม้ตาย GetChild(0) หรือใช้ตัวเอง
                if (transform.childCount > 0)
                    visualChild = transform.GetChild(0);
                else
                    visualChild = transform; // ไม่มีลูก ก็ขยับตัวเอง (กัน Error)
            }
        }

        // จำค่าการหมุนเริ่มต้นไว้
        initialRotation = visualChild.localRotation;
    }

    private void OnEnable()
    {
        // สมัครรับ event ตอนโดนตี
        if (_healthScript != null)
        {
            _healthScript.OnTakeDamage += PerformDotweenShake;
        }
    }

    private void OnDisable()
    {
        // ยกเลิก event เมื่อ object ถูกปิด
        if (_healthScript != null)
        {
            _healthScript.OnTakeDamage -= PerformDotweenShake;
        }

        // ถ้า object ถูกปิดกลางคัน ให้หยุด tween ทันทีเพื่อความปลอดภัย
        if (currentTweener != null && currentTweener.IsActive())
        {
            currentTweener.Kill();
        }
        // รีเซ็ตค่ากลับที่เดิม
        visualChild.localRotation = initialRotation;
    }

    private void PerformDotweenShake(float damage)
    {
        // ถ้ามีการสั่นค้างอยู่ ให้หยุดและ Reset กลับท่าเดิมก่อน เพื่อเริ่มสั่นใหม่ (Re-trigger)
        // การใช้ DOKill(true) จะบังคับให้ tween ที่ค้างอยู่ข้ามไป state จบเลยทันที
        visualChild.DOKill(true);

        // ตรวจสอบให้แน่ใจว่าก่อนสั่น เริ่มจากท่าปกติเสมอ (ป้องกันการเอียงค้างถ้าโดนตีรัวมากๆ)
        visualChild.localRotation = initialRotation;

        // --- คำสั่งมหัศจรรย์ของ DOTween ---
        // DOShakeRotation(ระยะเวลา, แรง(Vector3), ความถี่, ความสุ่ม, ค่อยๆเบาลงหรือไม่)
        currentTweener = visualChild.DOShakeRotation(duration, strength, vibrato, randomness, true)
            .SetEase(Ease.OutQuad) // ใช้ Ease OutQuad เพื่อให้ตอนจบมันค่อยๆ หยุด นุ่มนวลขึ้น
            .OnComplete(() => {
                // กันเหนียว: เมื่อสั่นเสร็จ ให้แน่ใจว่ากลับมา rotation เดิมเป๊ะๆ
                visualChild.localRotation = initialRotation;
                currentTweener = null;
            });
    }
}