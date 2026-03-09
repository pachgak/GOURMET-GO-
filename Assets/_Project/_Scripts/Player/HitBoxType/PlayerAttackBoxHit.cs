using UnityEngine;
using UnityEngine.Timeline;

public class PlayerAttackBoxHit : BaseHitBox
{
    // กำหนดขนาดและค่า Offset ของ Hitbox ได้ใน Inspector
    public Vector3 attackOffset = new Vector3(0, 0, 1f);
    public Vector3 attackSize = new Vector3(1.5f, 1.5f, 1.5f);

    private void OnEnable()
    {
        //PerformAttack();
    }

    // ฟังก์ชันนี้จะถูกเรียกจาก Player เมื่อทำการโจมตี
    public override void PerformAttack()
    {
        // คำนวณตำแหน่งและทิศทางของ Hitbox
        Vector3 position = transform.position + transform.rotation * attackOffset;

        // ใช้ Physics.OverlapBox เพื่อหา Collider ทั้งหมดที่อยู่ใน Hitbox
        Collider[] hitColliders = Physics.OverlapBox(position, attackSize / 2, transform.rotation, targetLayer);

        float minKnockbackMultiplier = 0;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent(out ITakeDamage canTakeDamage))
            {
                canTakeDamage.TakeDamage(damage);
            }

            if (hitCollider.TryGetComponent(out IKnockbackable knockbackable))
            {
                knockbackable.GetKnockedBack(knockbackDirection, knockbackForce, knockbackTime);
                
                if (minKnockbackMultiplier < knockbackable._knockbackMultiplier)
                {
                    minKnockbackMultiplier = knockbackable._knockbackMultiplier;
                }
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
        Gizmos.matrix = Matrix4x4.identity;
    }
}