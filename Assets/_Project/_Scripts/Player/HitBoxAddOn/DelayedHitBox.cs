using UnityEngine;

public class DelayedHitBox : BaseHitBox , IDelayable
{
    [Header("Telegraph Settings (ตั้งเวลา)")]
    public float delayTime = 1.0f; // เวลาชาร์จก่อนเปิด Hitbox

    [Header("Actual HitBox (Hitbox ตัวจริง)")]
    [Tooltip("ลาก Hitbox ตัวลูก (เช่น AttactHitRadius) มาใส่ช่องนี้")]
    public BaseHitBox actualHitBox;

    private float timer;
    private bool isCounting = false;

    private void OnEnable()
    {
        isCounting = false;

        // ปิด Hitbox ไว้ก่อนตั้งแต่เริ่ม
        if (actualHitBox != null) 
        {
            actualHitBox.gameObject.SetActive(false);
        }
    }

    // ฟังก์ชันนี้จะถูกเรียกทันทีที่ SpawnHitAction_M เสก Prefab ออกมา
    public override void PerformAttack()
    {
        // 1. ส่งต่อค่า Stats (Damage, Owner, Layer) ไปให้ Hitbox ตัวลูกเตรียมไว้เลย
        if (actualHitBox != null)
        {
            actualHitBox.targetLayer = this.targetLayer;
            actualHitBox.ownerHit = this.ownerHit;
            actualHitBox.damage = this.damage;
            actualHitBox.knockbackDirection = this.knockbackDirection;
            actualHitBox.knockbackForce = this.knockbackForce;
            actualHitBox.knockbackTime = this.knockbackTime;
        }

        // 2. เริ่มนับเวลาชาร์จ
        timer = delayTime;
        isCounting = true;
    }

    private void Update()
    {
        if (isCounting)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                ExecuteStrike(); // หมดเวลาชาร์จ -> เปิด Hitbox ทำงาน!
            }
        }
    }

    private void ExecuteStrike()
    {
        isCounting = false;

        // เปิด Hitbox จริงขึ้นมาและสั่งให้มันทำดาเมจ!
        if (actualHitBox != null)
        {
            actualHitBox.gameObject.SetActive(true);
            actualHitBox.PerformAttack(); 
        }
    }

    // *** ฟังก์ชันใหม่สำหรับให้ Modifier เรียกใช้ ***
    public void SetDelayTime(float newDelay)
    {
        delayTime = newDelay;
        // ถ้านับเวลาไปแล้ว (เพราะ PerformAttack รันไปก่อนหน้า) ให้แก้เวลาใหม่เลย
        if (isCounting)
        {
            timer = delayTime;
        }
    }
}