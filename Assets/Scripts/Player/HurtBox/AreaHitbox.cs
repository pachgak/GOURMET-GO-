// � Script AttackHitbox.cs
using UnityEngine;

public class AreaHitbox : MonoBehaviour , IHurtBox
{
    // ��˹���Ҵ��Ф�� Offset �ͧ Hitbox ��� Inspector
    public Vector3 attackOffset = new Vector3(0, 0, 1f);
    public float attackRadius = 1.5f;

    public float damage = 10f; // ��Ҵ�����ͧ�������
    public float knockbackForce = 5f; // �ç��ѡ
    [HideInInspector] public Vector3 knockbackDirection; // ��ȷҧ��ü�ѡ
    public LayerMask targetLayer; // ��駤�� Layer �ͧ�ѵ��� Inspector

    float IHurtBox._damage { get => damage; set => damage = value; }
    float IHurtBox._knockbackForce { get => knockbackForce; set => knockbackForce = value; }
    Vector3 IHurtBox._knockbackDirection { get => knockbackDirection; set => knockbackDirection = value; }

    private void Start()
    {
        PerformAttack();
    }

    // �ѧ��ѹ���ж١���¡�ҡ Player ����ͷӡ������
    public void PerformAttack()
    {
        // �ӹǳ���˹���з�ȷҧ�ͧ Hitbox
        Vector3 position = transform.position + transform.rotation * attackOffset;

        // �� Physics.OverlapBox ������ Collider �������������� Hitbox
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
                knockbackable.GetKnockedBack(knockbackDirection, knockbackForce);
            }
        }

        //playerSound and CameraShack
        if (hitColliders != null)
        {

        }
    }

    // �ѧ��ѹ����Ѻ�Ҵ Hitbox � Unity Editor
    void OnDrawGizmos()
    {
        // �ӹǳ���˹���з�ȷҧ�ͧ Hitbox
        Vector3 position = transform.position + transform.rotation * attackOffset;

        // ��駤���բͧ Gizmos ����ͧ�����Ѵ
        Gizmos.color = new Color(1f, 0, 0, 0.5f);

        // �Ҵ�ç��� Hitbox
        Gizmos.DrawWireSphere(position, attackRadius);
    }
}