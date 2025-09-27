using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SmartEnemyAI : MonoBehaviour
{
    // --- 1. ENUM FOR FSM STATES ---
    public enum EnemyState
    {
        Roaming,    // �Թ����Ҩش���� (Ẻ�����ҵ�)
        Chase,      // ������/�ѡ������ (Kiting)
        Standoff,   // ��ش���ԧ��͹����
        Attack      // ����
    }

    [Header("State Settings")]
    public EnemyState currentState;

    [Header("References")]
    private NavMeshAgent agent;
    public Transform playerTarget;
    private Rigidbody _rb;

    [Header("Ranges & Speeds")]
    public float sightRange = 10f;          // ���з���ͧ��� Player
    public float attackRange = 2f;          // ��������
    public float roamRadius = 15f;          // ����ա�������ش��� (������Ѻ��ǧ���˹ѡ��Ѻ Home)
    public float maxChaseSpeed = 5f;        // ��������㹡��������
    public float maxRoamSpeed = 3f;         // ��������㹡�����

    // --- �������͹���Ẻ�����ҵ� (Natural Roaming) ---
    [Header("Natural Roaming")]
    public float noiseScale = 0.5f;         // ��������㹡������¹��ȷҧ (��觹�����觪��)
    private float noiseOffset;
    private Vector3 homePosition;           // �ش������鹢ͧ�ѵ��

    // --- ��õ�����Ẻ���ԧ (Engaging Combat) ---
    [Header("Engaging Combat")]
    public float optimalCombatRange = 4f;   // ���з���ͧ����ѡ�����������ҧ������ (Kite/Strafe)

    [Header("Standoff State")]
    public float standoffDuration = 1.5f;   // �������Ҵ��ԧ (�Թҷ�)
    private float standoffTimer;
    public float standoffMovementRange = 1.0f; // �����Թ���ԧ��硹���

    // ���������
    private bool playerInSightRange, playerInAttackRange;

    [Header("KnockBack")]

    [Range(0.001f, 0.1f)][SerializeField] private float StillThreshold = 0.05f;
    [SerializeField] private float MaxKnockbackTime = 0.5f;

    [SerializeField] private bool _canKnockback = true;
    [SerializeField] private Coroutine _KnockbackCoroutine;
    // --- UNITY LIFE CYCLE ---

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        currentState = EnemyState.Roaming;

        // �������� Player ���� Tag "Player"
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        // ��駤�������������Ѻ Roaming ��� Home Position
        homePosition = transform.position;
        // �������������������������ѵ�ٷء����Թ���ᾷ�������ǡѹ
        noiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        CheckPlayerDistance();

        // Finite State Machine (FSM)
        switch (currentState)
        {
            case EnemyState.Roaming:
                RoamingState();
                break;
            case EnemyState.Chase:
                ChaseState();
                break;
            case EnemyState.Standoff:
                StandoffState();
                break;
            case EnemyState.Attack:
                AttackState();
                break;
        }
    }

    // --- HELPER METHODS ---

    private void CheckPlayerDistance()
    {
        if (playerTarget != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
            // ��������� Raycast ���͵�Ǩ�Ѻ��觡մ��ҧ (��������繨�ԧ �ѵ�٤������������µ�)
            playerInSightRange = distanceToPlayer <= sightRange && HasLineOfSight(playerTarget.position);
            playerInAttackRange = distanceToPlayer <= attackRange;
        }
        else
        {
            playerInSightRange = false;
            playerInAttackRange = false;
        }
    }

    // ��Ǩ�ͺ����µ� (Line of Sight)
    private bool HasLineOfSight(Vector3 targetPosition)
    {
        RaycastHit hit;
        Vector3 direction = targetPosition - transform.position;
        // ��Ǩ�ͺ�������觡մ��ҧ�����ҧ�ѵ�١Ѻ�������������
        if (Physics.Raycast(transform.position, direction.normalized, out hit, sightRange))
        {
            // ��Ǩ�ͺ��� Raycast �� Player �������
            if (hit.transform == playerTarget)
            {
                return true;
            }
        }
        return false;
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    // --- NATURAL ROAMING LOGIC ---

    private Vector3 GetRoamDirection()
    {
        // �� Perlin Noise (���ͧ Simplex Noise) ����������ҡ������¹�ŧ��ȷҧ����Һ���
        float angle = Mathf.PerlinNoise(noiseOffset, Time.time * noiseScale) * 360f;
        Vector3 direction = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));

        // ��áж�ǧ���˹ѡ��Ѻ���ش�Դ (Home Position) ������������Թ����� (RoamRadius)
        float distanceToHome = Vector3.Distance(transform.position, homePosition);
        if (distanceToHome > roamRadius)
        {
            Vector3 homeDirection = (homePosition - transform.position).normalized;
            // �� Lerp ���Ͷ�ǧ���˹ѡ: ����Ũҡ Home �ҡ������� ����觶١�֧��Ѻ�ҡ��ҹ��
            direction = Vector3.Lerp(direction, homeDirection, (distanceToHome - roamRadius) / roamRadius);
        }

        return direction.normalized;
    }


    // --- STATE FUNCTIONS ---

    private void RoamingState()
    {
        agent.speed = maxRoamSpeed;
        agent.isStopped = false;

        // Transition: ��� Player
        if (playerInSightRange)
        {
            currentState = EnemyState.Chase;
            return;
        }

        // Action: ����͹�������ȷҧ���ӹǳ�ҡ Noise (�Թ���Ẻ�����ҵ�)
        Vector3 roamDirection = GetRoamDirection();
        // ��駨ش�����繷�ȷҧ��ҧ˹����硹���������� NavMeshAgent �ӹǳ��鹷ҧ
        agent.SetDestination(transform.position + roamDirection * 5f);
    }

    private void ChaseState()
    {
        agent.speed = maxChaseSpeed;

        // Transition 1: �������������� -> ���ԧ/����
        if (playerInAttackRange)
        {
            currentState = EnemyState.Standoff;
            standoffTimer = standoffDuration;
            return;
        }

        // Transition 2: Player ˹��͡�͡������µ� -> ���
        if (!playerInSightRange)
        {
            currentState = EnemyState.Roaming;
            return;
        }

        // Action: ������/Kiting (�ѡ��������������� Optimal Combat Range)
        if (playerTarget != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

            if (distanceToPlayer > optimalCombatRange)
            {
                // ������Թ�: �������� Player
                agent.SetDestination(playerTarget.position);
            }
            else // (����㹪�ǧ AttackRange �֧ OptimalCombatRange)
            {
                // �����������з��������� (Optimal Range): �Թ⩺/�����ѧ (Kite/Strafe) 
                // �����ѡ����������������� Standoff
                Vector3 targetDirection = (playerTarget.position - transform.position).normalized;
                // �ӹǳ�ش���·��������ҧ�ҡ Player �͡����硹���
                Vector3 kiteDestination = playerTarget.position - targetDirection * optimalCombatRange * 0.8f;

                agent.SetDestination(kiteDestination);
            }
            agent.isStopped = false;
        }
    }

    private void StandoffState()
    {
        // Transition 1: ������� -> ����
        if (standoffTimer <= 0)
        {
            currentState = EnemyState.Attack;
            agent.isStopped = true; // ��ش����͹�����������
            return;
        }

        // Transition 2: Player ˹��͡�͡������µ� -> ���
        if (!playerInSightRange)
        {
            currentState = EnemyState.Roaming;
            agent.isStopped = false;
            return;
        }

        // Action: ���ԧ
        standoffTimer -= Time.deltaTime;
        FaceTarget(playerTarget.position);

        // �Թ���ԧ��硹��� (SetNewStandoffMovePoint �е�ͧ�١���¡����Ͷ֧�ش�����������)
        if (agent.remainingDistance < 0.1f && !agent.pathPending)
        {
            SetNewStandoffMovePoint();
        }
    }

    private void SetNewStandoffMovePoint()
    {
        // �����ش���� �ͺ����ѵ�� ��������Դ��� "⩺/Strafe" ��������
        Vector3 randomOffset = Random.insideUnitSphere * standoffMovementRange;
        randomOffset.y = 0;
        Vector3 targetPosition = transform.position + randomOffset;

        NavMeshHit hit;
        // �� SamplePosition �������������Ҩش������躹 NavMesh
        if (NavMesh.SamplePosition(targetPosition, out hit, standoffMovementRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void AttackState()
    {
        agent.isStopped = true; // ��ش����͹������������� Attack State

        // Transition 1: Player ˹��͡�͡�������� -> ������
        if (!playerInAttackRange && playerInSightRange)
        {
            currentState = EnemyState.Chase;
            agent.isStopped = false;
            return;
        }

        // Transition 2: Player ˹��͡�͡������µ����� -> ���
        if (!playerInSightRange)
        {
            currentState = EnemyState.Roaming;
            agent.isStopped = false;
            return;
        }

        // Action: ����
        FaceTarget(playerTarget.position);
        // TODO: ��� Logic ������� (�ԧ, �ѹ) �����
    }

    public void GetKnockedBack(Vector3 direction, float force)
    {
        if (!_canKnockback) return;

        if (_KnockbackCoroutine != null) StopCoroutine(_KnockbackCoroutine);
        _KnockbackCoroutine = StartCoroutine(ApplyKnockback(direction, force));
    }

    private IEnumerator ApplyKnockback(Vector3 direction, float force)
    {
        Debug.Log($"ApplyKnockback : {direction} | {force}");

        yield return null;
        agent.enabled = false;
        _rb.useGravity = true;
        _rb.isKinematic = false;

        _rb.AddForce(direction * force, ForceMode.Impulse);

        yield return new WaitForFixedUpdate();
        float knockbackTime = Time.time;
        yield return new WaitUntil(
            () => _rb.linearVelocity.magnitude < StillThreshold || Time.time > knockbackTime + MaxKnockbackTime
        );
        yield return new WaitForSeconds(0.25f);

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.useGravity = false;
        _rb.isKinematic = true;
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

    // --- GIZMOS FOR VISUAL DEBUGGING ---

    private void OnDrawGizmosSelected()
    {
        // ��˹����˹��������
        Vector3 position = transform.position;

        // 1. �Ҵ Roam Radius (����ա���Թ���)
        // �ʴ��ҳ�ࢵ����Թ�����١��ǧ���˹ѡ
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(homePosition, roamRadius);

        // 2. �Ҵ Sight Range (���С���ͧ���)
        // ��������͡ Object �ѵ��� Scene �������ǧ����տ��
        Gizmos.color = new Color(0.2f, 0.2f, 1f, 0.5f); // �տ����͹
        Gizmos.DrawWireSphere(position, sightRange);

        // 3. �Ҵ Optimal Combat Range (�����ѡ�����е�����)
        // ���з���ѵ�٨������ Kiting/Strafing
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(position, optimalCombatRange);

        // 4. �Ҵ Attack Range (��������)
        // ���з���ѵ�٨�����¹� State Standoff/Attack
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(position, attackRange);
    }
}