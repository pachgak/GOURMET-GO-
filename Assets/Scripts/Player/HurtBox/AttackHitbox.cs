// � Script AttackHitbox.cs
using UnityEngine;

public class AttackHitbox : MonoBehaviour , IHurtBox
{
    // ��˹���Ҵ��Ф�� Offset �ͧ Hitbox ��� Inspector
    public Vector3 attackOffset = new Vector3(0, 0, 1f);
    public Vector3 attackSize = new Vector3(1.5f, 1.5f, 1.5f);

    private float damage = 10f; // ��Ҵ�����ͧ�������
    private float knockbackForce = 5f; // �ç��ѡ
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

    // �ѧ��ѹ����Ѻ�Ҵ Hitbox � Unity Editor
    void OnDrawGizmos()
    {
        // �ӹǳ���˹���з�ȷҧ�ͧ Hitbox
        Vector3 position = transform.position + transform.rotation * attackOffset;

        // ��駤���բͧ Gizmos ����ͧ�����Ѵ
        Gizmos.color = new Color(1f, 0, 0, 0.5f);

        // �Ҵ���ͧ Hitbox
        Gizmos.matrix = Matrix4x4.TRS(position, transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, attackSize);
        Gizmos.matrix = Matrix4x4.identity;
    }
}