using UnityEngine;

public class KumongaHealth : EnemyHealth // สมมติว่าคลาสเลือดเดิมชื่อนี้
{
    [Header("Kumonga Armor")]
    public float armorDamageReduction = 0.2f; // โดนดาเมจแค่ 20% (ลดไป 80%)

    private KumongaAI _kumongaAI;

    protected override void Awake()
    {
        base.Awake();
        _kumongaAI = GetComponent<KumongaAI>(); // ดึง AI มาเพื่อเช็คว่ามึนไหม
    }

    // สมมติว่าคลาสแม่มีฟังก์ชัน TakeDamage ให้ override
    public override void TakeDamage(float damage, GameObject customHitVFX = null)
    {
        float finalDamage = damage;

        // ถ้า AI ไม่ได้ติดสตันอยู่ ให้ลดดาเมจลง!
        if (_kumongaAI != null && !_kumongaAI.IsStunned)
        {
            finalDamage *= armorDamageReduction;
            Debug.Log($"Kumonga Armor Active! ลดดาเมจเหลือ {finalDamage}");
        }
        else
        {
            Debug.Log($"Kumonga is Stunned! รับดาเมจเต็มๆ {finalDamage}");
        }

        // ส่งดาเมจที่คำนวณแล้วไปให้คลาสแม่จัดการต่อ (ลดหลอดเลือด, แสดงเลขดาเมจ)
        base.TakeDamage(finalDamage);
    }
}