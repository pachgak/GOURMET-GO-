// ใน Script AttackHitbox.cs
using UnityEngine;
using static IHitBox;

public class AttackHitbox : BaseHitBox
{
    // กำหนดขนาดและค่า Offset ของ Hitbox ได้ใน Inspector
    public Vector3 attackOffset = new Vector3(0, 0, 1f);
    public Vector3 attackSize = new Vector3(1.5f, 1.5f, 1.5f);

    private void OnEnable()
    {
        //PerformAttack();
    }

    public override Collider[] GetCollidersInArea(LayerMask mask)
    {
        Vector3 frontOffset = new Vector3(0, attackSize.y / 2, attackSize.z / 2) + attackOffset;
        Vector3 position = transform.position + transform.rotation * frontOffset;
        return Physics.OverlapBox(position, attackSize / 2, transform.rotation, mask);
    }

    // ฟังก์ชันนี้จะถูกเรียกจาก Player เมื่อทำการโจมตี
    public override void PerformAttack()
    {
        // โค้ดสั้นลง! ดึงเป้าหมายจากฟังก์ชันใหม่
        Collider[] hitColliders = GetCollidersInArea(targetLayer);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent(out ITakeDamage canTakeDamage))
            {
                canTakeDamage.TakeDamage(damage, customHitVFX);
            }

            if (hitCollider.TryGetComponent(out IKnockbackable knockbackable))
            {
                knockbackable.GetKnockedBack(knockbackDirection, knockbackForce, knockbackTime);
            }
        }

        if (hitColliders.Length > 0) //&& ownerHit == CameraShakeManager.instance.playerGameObject)
        {
            //CameraShakeManager.instance.ShakePlayerAttack();
            OnAttackHit?.Invoke(hitColliders);
        }
        else
        {
            OnNoHit?.Invoke();
        }
    }

    // ฟังก์ชันสำหรับวาด Hitbox ใน Unity Editor
    void OnDrawGizmos()
    {
        // คำนวณตำแหน่งและทิศทางของ Hitbox
        //Vector3 position = transform.position + transform.rotation * attackOffset;

        Vector3 frontOffset = new Vector3(0, attackSize.y / 2, attackSize.z / 2) + attackOffset;
        Vector3 position = transform.position + transform.rotation * frontOffset;

        // ตั้งค่าสีของ Gizmos ให้มองเห็นได้ชัด
        Gizmos.color = new Color(1f, 0, 0, 0.5f);

        // วาดกล่อง Hitbox
        Gizmos.matrix = Matrix4x4.TRS(position, transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, attackSize);

        // วาดเส้นขอบให้ดูคมชัดขึ้น
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, attackSize);

        Gizmos.matrix = Matrix4x4.identity;
    }
}