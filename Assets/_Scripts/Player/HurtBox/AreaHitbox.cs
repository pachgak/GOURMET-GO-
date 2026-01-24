// ใน Script AttackHitbox.cs
using UnityEngine;
using static IHitBox;

public class AreaHitbox : BaseHitBox
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
    void OnDrawGizmos()
    {
        // คำนวณตำแหน่งและทิศทางของ Hitbox
        Vector3 position = transform.position + transform.rotation * attackOffset;

        // ตั้งค่าสีของ Gizmos ให้มองเห็นได้ชัด
        Gizmos.color = new Color(1f, 0, 0, 0.5f);

        // วาดทรงกลม Hitbox
        Gizmos.DrawWireSphere(position, attackRadius);
    }
}