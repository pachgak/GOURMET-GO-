using UnityEngine;

public class DestroyEndVFX : MonoBehaviour
{
    public ParticleSystem _particleSystem;

    // เพิ่มตัวแปรเช็คว่าเอฟเฟกต์เริ่มเล่นไปหรือยัง
    private bool hasStartedPlaying = false;

    private void OnEnable()
    {
        // *** สำคัญมากสำหรับ Object Pool ***
        // ต้องรีเซ็ตค่าเป็น false ทุกครั้งที่ดึงกลับมาใช้ใหม่
        hasStartedPlaying = false;
    }

    void Start()
    {
        // เปลี่ยนเป็น GetComponentInChildren (ใส่ true เพื่อให้หาเจอแม้ GameObject ลูกจะปิด Active อยู่)
        if (_particleSystem == null)
        {
            _particleSystem = GetComponentInChildren<ParticleSystem>(true);
        }
    }

    void Update()
    {
        if (_particleSystem == null) return;

        // Phase 1: รอจนกว่า Particle จะเริ่มเล่น (Hitbox ถูกเปิด Active)
        if (!hasStartedPlaying)
        {
            // ถ้า Particle ถูกเปิดและเริ่มเล่นแล้ว ให้เปลี่ยนสถานะ
            if (_particleSystem.isPlaying)
            {
                hasStartedPlaying = true;
            }
        }
        // Phase 2: ถ้ามันเริ่มเล่นไปแล้ว ให้รอจนกว่ามันจะตาย (เล่นจบ)
        else
        {
            // IsAlive(true) จะเช็คครอบคลุมไปถึง Particle ลูกๆ ด้วย (ถ้ามี)
            if (!_particleSystem.IsAlive(true))
            {
                ReturnObjectToPool();
            }
        }
    }

    private void ReturnObjectToPool()
    {
        // กันเหนียว เผื่อ Update รันซ้ำช่วงจังหวะคาบเกี่ยว
        hasStartedPlaying = false;
        ObjectPoolingManager.Instance.Respawn(gameObject);
    }
}