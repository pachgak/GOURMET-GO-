using UnityEngine;

// ถ้าคุณมี Interface IDelayable อยู่แล้ว สามารถเอาคอมเมนต์ออกได้เลยครับ
public class DelayVFX : MonoBehaviour // , IDelayable 
{
    [Header("Delay Settings (ตั้งเวลา)")]
    public float delayTime = 0.5f; // เวลาหน่วงก่อนเปิด VFX ตัวลูก

    [Header("Delayed Object (VFX ตัวลูก)")]
    [Tooltip("ลาก GameObject ตัวลูก (เช่น สายฟ้าผ่า) มาใส่ช่องนี้")]
    public GameObject delayObject;

    private float _timer;
    private bool _isCounting = false;

    private void OnEnable()
    {
        _isCounting = false;

        // 1. ปิด VFX ตัวลูกไว้ก่อนตั้งแต่เริ่มเสก
        if (delayObject != null)
        {
            delayObject.SetActive(false);
        }

        // 2. สำหรับ VFX เราให้มันเริ่มนับเวลาทันทีที่ถูกเสกเลย (ต่างจาก Hitbox ที่รอ PerformAttack)
        StartDelay();
    }

    public void StartDelay()
    {
        _timer = delayTime;
        _isCounting = true;
    }

    private void Update()
    {
        if (_isCounting)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                ExecuteVFX(); // หมดเวลาชาร์จ -> เปิด VFX ทำงาน!
            }
        }
    }

    private void ExecuteVFX()
    {
        _isCounting = false;

        // เปิด VFX ตัวลูกขึ้นมา (เพื่อให้ Particle System หรือ สคริปต์ข้างในเริ่มทำงาน)
        if (delayObject != null)
        {
            delayObject.SetActive(true);
        }
    }

    // *** ฟังก์ชันสำหรับให้ Modifier หรือสคริปต์อื่นเรียกแก้เวลาได้ (เหมือนใน DelayedHitBox) ***
    public void SetDelayTime(float newDelay)
    {
        delayTime = newDelay;
        // ถ้านับเวลาไปแล้ว ให้แก้เวลาใหม่เลย
        if (_isCounting)
        {
            _timer = delayTime;
        }
    }
}