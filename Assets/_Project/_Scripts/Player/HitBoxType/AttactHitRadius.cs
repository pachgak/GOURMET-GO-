// ใน Script AttackHitbox.cs
using UnityEngine;
using static IHitBox;

public class AttactHitRadius : BaseHitBox
{
    // กำหนดขนาดและค่า Offset ของ Hitbox ได้ใน Inspector
    public Vector3 attackOffset = new Vector3(0, 0, 1f);
    public float attackRadius = 1.5f;

    private void OnEnable()
    {
        //PerformAttack();
    }

    private void Start()
    {
        //PerformAttack();
    }

    // ฟังก์ชันนี้จะถูกเรียกจาก Player เมื่อทำการโจมตี
    public override void PerformAttack()
    {
        // คำนวณตำแหน่งและทิศทางของ Hitbox
        Vector3 position = transform.position + transform.rotation * attackOffset;

        // ใช้ Physics.OverlapBox เพื่อหา Collider ทั้งหมดที่อยู่ใน Hitbox
        Collider[] hitColliders = Physics.OverlapSphere(position, attackRadius, targetLayer);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent(out ITakeDamage canTakeDamage))
            {
                canTakeDamage.TakeDamage(damage);
            }

            if (hitCollider.TryGetComponent(out IKnockbackable knockbackable))
            {
                knockbackDirection = (hitCollider.transform.position - transform.position).normalized;
                knockbackable.GetKnockedBack(knockbackDirection, knockbackForce, knockbackTime);
            }
        }

        //playerSound and CameraShack
        if (hitColliders.Length > 0 && ownerHit == CameraShakeManager.instance.playerGameObject)
        {
            CameraShakeManager.instance.ShakePlayerAttack();
        }
    }

    // ฟังก์ชันสำหรับวาด Hitbox ใน Unity Editor
    //void OnDrawGizmos()
    //{
    //    // คำนวณตำแหน่งและทิศทางของ Hitbox
    //    Vector3 position = transform.position + transform.rotation * attackOffset;

    //    // ตั้งค่าสีของ Gizmos ให้มองเห็นได้ชัด
    //    Gizmos.color = new Color(1f, 0, 0, 0.5f);

    //    // วาดทรงกลม Hitbox
    //    Gizmos.DrawWireSphere(position, attackRadius);
    //}

    private void OnDrawGizmos()
    {
        // 1. คำนวณตำแหน่งจริงที่เกิดการโจมตี (World Space)
        // โดยเอาตำแหน่งตัวเอง + (การหมุน * ระยะ Offset)
        Vector3 position = transform.position + (transform.rotation * attackOffset);

        // 2. ตั้งค่าสี
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f); // สีเขียวโปร่งแสง

        // 3. ใช้ Matrix เพื่อรองรับการเคลื่อนที่และ Scale
        // สำหรับ Sphere เราเน้นไปที่ Position และ Scale
        Gizmos.matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);

        // 4. วาดทรงกลม
        // วาดที่ Vector3.zero เพราะเราย้ายตำแหน่งไปไว้ใน Matrix แล้ว
        Gizmos.DrawSphere(Vector3.zero, attackRadius);

        // 5. วาดเส้นขอบเพื่อให้เห็นมิติ (WireSphere)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, attackRadius);

        // 6. คืนค่า Matrix
        Gizmos.matrix = Matrix4x4.identity;
    }
}