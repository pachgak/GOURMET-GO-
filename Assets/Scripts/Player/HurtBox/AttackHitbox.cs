// ใน Script AttackHitbox.cs
using UnityEngine;

public class AttackHitbox : MonoBehaviour , IHurtBox
{
    // กำหนดขนาดและค่า Offset ของ Hitbox ได้ใน Inspector
    public Vector3 attackOffset = new Vector3(0, 0, 1f);
    public Vector3 attackSize = new Vector3(1.5f, 1.5f, 1.5f);

    private float damage = 10f; // ค่าดาเมจของการโจมตี
    private float knockbackForce = 5f; // แรงผลัก
    [HideInInspector] public Vector3 knockbackDirection; // ทิศทางการผลัก
    public LayerMask targetLayer; // ตั้งค่า Layer ของศัตรูใน Inspector

    float IHurtBox._damage { get => damage; set => damage = value; }
    float IHurtBox._knockbackForce { get => knockbackForce; set => knockbackForce = value; }
    Vector3 IHurtBox._knockbackDirection { get => knockbackDirection; set => knockbackDirection = value; }

    private void Start()
    {
        PerformAttack();
    }

    // ฟังก์ชันนี้จะถูกเรียกจาก Player เมื่อทำการโจมตี
    public void PerformAttack()
    {
        // คำนวณตำแหน่งและทิศทางของ Hitbox
        Vector3 position = transform.position + transform.rotation * attackOffset;

        // ใช้ Physics.OverlapBox เพื่อหา Collider ทั้งหมดที่อยู่ใน Hitbox
        Collider[] hitColliders = Physics.OverlapBox(position, attackSize / 2, transform.rotation, targetLayer);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent(out ITakeDamage canTakeDamage))
            {
                canTakeDamage.TakeDamage(damage);
            }

            if (hitCollider.TryGetComponent(out IKnockbackable knockbackable))
            {
                knockbackable.GetKnockedBack(knockbackDirection, knockbackForce);
            }
        }

        //playerSound and CameraShack
        if (hitColliders != null)
        {

        }
    }

    // ฟังก์ชันสำหรับวาด Hitbox ใน Unity Editor
    void OnDrawGizmos()
    {
        // คำนวณตำแหน่งและทิศทางของ Hitbox
        Vector3 position = transform.position + transform.rotation * attackOffset;

        // ตั้งค่าสีของ Gizmos ให้มองเห็นได้ชัด
        Gizmos.color = new Color(1f, 0, 0, 0.5f);

        // วาดกล่อง Hitbox
        Gizmos.matrix = Matrix4x4.TRS(position, transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, attackSize);
        Gizmos.matrix = Matrix4x4.identity;
    }
}