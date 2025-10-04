using UnityEngine;

public abstract class BaseHitBox : MonoBehaviour , IHurtBox
{
    protected float damage = 10f; // ค่าดาเมจของการโจมตี
    protected float knockbackForce = 5f; // แรงผลัก
    [HideInInspector] public Vector3 knockbackDirection; // ทิศทางการผลัก
    public LayerMask targetLayer; // ตั้งค่า Layer ของศัตรูใน Inspector

    public GameObject ownerHit;
    //public IHurtBox.DamageType damageType;

    //IHurtBox.DamageType IHurtBox._damageType { get => damageType; set => damageType = value; }
    GameObject IHurtBox._ownerHit { get => ownerHit; set => ownerHit = value; }
    float IHurtBox._damage { get => damage; set => damage = value; }
    float IHurtBox._knockbackForce { get => knockbackForce; set => knockbackForce = value; }
    Vector3 IHurtBox._knockbackDirection { get => knockbackDirection; set => knockbackDirection = value; }
    LayerMask IHurtBox._targetLayer { get => targetLayer; set => targetLayer = value; }

    public virtual void PerformAttack()
    {
        throw new System.NotImplementedException();
    }
}
