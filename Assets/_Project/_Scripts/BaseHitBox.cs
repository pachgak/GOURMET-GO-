using UnityEngine;
using System;

public abstract class BaseHitBox : MonoBehaviour , IHitBox
{
    [HideInInspector] public GameObject ownerHit;
    [HideInInspector] public LayerMask targetLayer; // ตั้งค่า Layer ของศัตรูใน Inspector
    [HideInInspector] public float damage = 0f; // ค่าดาเมจของการโจมตี

    // *** เพิ่มบรรทัดนี้: ให้แต่ละ Hitbox ใส่ VFX เฉพาะตัวได้ ***
    [Header("Hit VFX (Optional)")]
    public GameObject customHitVFX; 

    [HideInInspector] public Vector3 knockbackDirection; // ทิศทางการผลัก
    [HideInInspector] public float knockbackForce = 0f; // แรงผลัก
    [HideInInspector] public float knockbackTime = 0f; // แรงผลัก

    public Action<Collider[]> OnAttackHit;
    public Action OnNoHit;

    //public IHurtBox.DamageType damageType;

    public virtual void OnDisable()
    {
        OnAttackHit = null;
        OnNoHit = null;
    }

    //IHurtBox.DamageType IHurtBox._damageType { get => damageType; set => damageType = value; }
    GameObject IHitBox._ownerHit { get => ownerHit; set => ownerHit = value; }
    LayerMask IHitBox._targetLayer { get => targetLayer; set => targetLayer = value; }
    float IHitBox._damage { get => damage; set => damage = value; }
    float IHitBox._knockbackForce { get => knockbackForce; set => knockbackForce = value; }
    float IHitBox._knockbackTime { get => knockbackTime; set => knockbackTime = value; }
    Vector3 IHitBox._knockbackDirection { get => knockbackDirection; set => knockbackDirection = value; }

    Action<Collider[]> IHitBox._OnAttackHit { get => OnAttackHit; set => OnAttackHit = value; }
    Action IHitBox._OnNoHit { get => OnNoHit; set => OnNoHit = value; }

    public virtual void PerformAttack()
    {
        throw new System.NotImplementedException();
    }

    // ==========================================
    // ระบบดึง Collider ในพื้นที่ (รองรับทุกรูปทรง)
    // ==========================================
    public virtual Collider[] GetCollidersInArea(LayerMask mask)
    {
        // 1. ถ้าเป็น Box Collider -> ใช้ OverlapBox
        if (TryGetComponent(out BoxCollider box))
        {
            Vector3 center = transform.TransformPoint(box.center);
            Vector3 extents = Vector3.Scale(box.size, transform.lossyScale) / 2f;
            return Physics.OverlapBox(center, extents, transform.rotation, mask);
        }

        // 2. ถ้าเป็น Sphere Collider -> ใช้ OverlapSphere
        else if (TryGetComponent(out SphereCollider sphere))
        {
            Vector3 center = transform.TransformPoint(sphere.center);
            float maxScale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Max(Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.z)));
            float radius = sphere.radius * maxScale;
            return Physics.OverlapSphere(center, radius, mask);
        }

        // 3. ถ้าเป็น Collider ชนิดอื่นๆ ให้ใช้ Bounding Box (กล่องครอบ) เป็นท่าไม้ตายสำรอง
        else if (TryGetComponent(out Collider col))
        {
            return Physics.OverlapBox(col.bounds.center, col.bounds.extents, transform.rotation, mask);
        }

        // ถ้าไม่มี Collider แปะอยู่เลย
        return new Collider[0];
    }
}
