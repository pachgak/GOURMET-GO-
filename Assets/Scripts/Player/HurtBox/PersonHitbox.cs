using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PersonHitbox : MonoBehaviour , IHurtBox
{
    // กำหนดขนาดและค่า Offset ของ Hitbox ได้ใน Inspector
    public Vector3 attackOffset = new Vector3(0, 0, 1f);
    public Vector3 attackSize = new Vector3(1.5f, 1.5f, 1.5f);

    public float damage = 10f; // ค่าดาเมจของการโจมตี
    public float knockbackForce = 5f; // แรงผลัก
    [HideInInspector] public Vector3 knockbackDirection; // ทิศทางการผลัก
    public LayerMask targetLayer; // ตั้งค่า Layer ของศัตรูใน Inspector
    public LayerMask wallLayer; // ตั้งค่า Layer ของศัตรูใน Inspector

    float IHurtBox._damage { get => damage; set => damage = value; }
    float IHurtBox._knockbackForce { get => knockbackForce; set => knockbackForce = value; }
    Vector3 IHurtBox._knockbackDirection { get => knockbackDirection; set => knockbackDirection = value; }

    private void Start()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Mathf.Log(targetLayer.value, 2))
        {
            Collider hitCollider = other;

            if (hitCollider.TryGetComponent(out ITakeDamage canTakeDamage))
            {
                canTakeDamage.TakeDamage(damage);
            }

            if (hitCollider.TryGetComponent(out IKnockbackable knockbackable))
            {
                knockbackable.GetKnockedBack(knockbackDirection, knockbackForce);
            }

            //playerSound and CameraShack

            Destroy(gameObject);
        }
        if (other.gameObject.layer == Mathf.Log(wallLayer.value))
        {
            Destroy(gameObject);
        }
    }
}
