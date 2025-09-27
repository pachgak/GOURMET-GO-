using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour , IKnockbackable
{
    public bool canMove;
    [Header("Enemy Stats")]
    [SerializeField] private float lookRadius = 10f; // ���С���ͧ��繢ͧ������

    [Range(0.001f, 0.1f)][SerializeField] private float StillThreshold = 0.05f;
    [SerializeField] private float MaxKnockbackTime = 0.5f;

    [SerializeField] private Transform target; // ������� (������)
    [SerializeField] private NavMeshAgent agent; // Component NavMeshagent
    private Rigidbody rb;

    [SerializeField] private bool canKnockback = true;

    [SerializeField] private Coroutine KnockbackCoroutine;

    void Start()
    {
        // �� GameObject �ͧ������
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
        else
        {
            Debug.LogWarning("PlayerMovement instance not found. Enemy will not have a target.");
        }

        // �Ѻ NavMeshagent Component
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (target == null || !agent.enabled || canMove) return;

        // �ӹǳ������ҧ�����ҧ Enemy �Ѻ ������
        float distance = Vector3.Distance(target.position, transform.position);

        // ��Ҽ���������������ͧ���
        if (distance <= lookRadius)
        {
            // ��� NavMeshagent ����͹�����Ҽ�����
            agent.SetDestination(target.position);

            // ��Ҽ����������������ش����
            if (distance <= agent.stoppingDistance)
            {
                // ������Ѻ����������͡�á�з���� � �����
                FaceTarget();
            }
        }
    }

    // �ѧ��ѹ����Ѻ����� Enemy �ѹ˹����Ҽ�����
    private void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    public void GetKnockedBack(Vector3 direction, float force)
    {
        if (!canKnockback) return;

        if(KnockbackCoroutine != null) StopCoroutine(KnockbackCoroutine);
        KnockbackCoroutine = StartCoroutine(ApplyKnockback(direction,force));
    }

    private IEnumerator ApplyKnockback(Vector3 direction, float force)
    {
        Debug.Log($"ApplyKnockback : {direction} | {force}");

        yield return null;
        agent.enabled = false;
        rb.useGravity = true;
        rb.isKinematic = false;

        rb.AddForce(direction * force, ForceMode.Impulse);

        yield return new WaitForFixedUpdate();
        float knockbackTime = Time.time;
        yield return new WaitUntil(
            () => rb.linearVelocity.magnitude < StillThreshold || Time.time > knockbackTime + MaxKnockbackTime
        );
        yield return new WaitForSeconds(0.25f);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;
        agent.Warp(transform.position);
        agent.enabled = true;

        yield return null;


        //��Ѻ� stest �Թ
        //if (Player != null)
        //{
        //    KnockbackCoroutine = StartCoroutine(ChasePlayer(Player));
        //}
        //else
        //{
        //    KnockbackCoroutine = StartCoroutine(Roam());
        //}
    }

    void OnDrawGizmosSelected()
    {
        // �Ҵǧ�����ᴧ� Scene ��������ͧ������С���ͧ��繢ͧ Enemy
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }

}