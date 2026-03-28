using UnityEngine;
using System;

public abstract class BaseHitBox : MonoBehaviour , IHitBox
{
    [HideInInspector] public GameObject ownerHit;
    [HideInInspector] public LayerMask targetLayer; // ตั้งค่า Layer ของศัตรูใน Inspector
    [HideInInspector] public float damage = 0f; // ค่าดาเมจของการโจมตี
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

    
}
