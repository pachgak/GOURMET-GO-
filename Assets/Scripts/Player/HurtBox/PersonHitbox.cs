using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PersonHitbox : MonoBehaviour , IHurtBox
{
    // ��˹���Ҵ��Ф�� Offset �ͧ Hitbox ��� Inspector
    public Vector3 attackOffset = new Vector3(0, 0, 1f);
    public Vector3 attackSize = new Vector3(1.5f, 1.5f, 1.5f);

    public float damage = 10f; // ��Ҵ�����ͧ�������
    public float knockbackForce = 5f; // �ç��ѡ
    [HideInInspector] public Vector3 knockbackDirection; // ��ȷҧ��ü�ѡ
    public LayerMask targetLayer; // ��駤�� Layer �ͧ�ѵ��� Inspector
    public LayerMask wallLayer; // ��駤�� Layer �ͧ�ѵ��� Inspector

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
