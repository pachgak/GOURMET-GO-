using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SmartEnemyAI : MonoBehaviour
{
    // --- 1. ENUM FOR FSM STATES ---
    public enum EnemyState
    {
        Roaming,    // เดินเตร่หาจุดสุ่ม (แบบธรรมชาติ)
        Chase,      // ไล่ล่า/รักษาระยะ (Kiting)
        Standoff,   // หยุดดูเชิงก่อนโจมตี
        Attack      // โจมตี
    }

    [Header("State Settings")]
    public EnemyState currentState;

    [Header("References")]
    private NavMeshAgent agent;
    public Transform playerTarget;
    private Rigidbody _rb;

    [Header("Ranges & Speeds")]
    public float sightRange = 10f;          // ระยะที่มองเห็น Player
    public float attackRange = 2f;          // ระยะโจมตี
    public float roamRadius = 15f;          // รัศมีการสุ่มจุดโรม (ใช้สำหรับถ่วงน้ำหนักกลับ Home)
    public float maxChaseSpeed = 5f;        // ความเร็วในการไล่ล่า
    public float maxRoamSpeed = 3f;         // ความเร็วในการโรม

    // --- การเคลื่อนที่แบบธรรมชาติ (Natural Roaming) ---
    [Header("Natural Roaming")]
    public float noiseScale = 0.5f;         // ความเร็วในการเปลี่ยนทิศทาง (ยิ่งน้อยยิ่งช้า)
    private float noiseOffset;
    private Vector3 homePosition;           // จุดเริ่มต้นของศัตรู

    // --- การต่อสู้แบบดูเชิง (Engaging Combat) ---
    [Header("Engaging Combat")]
    public float optimalCombatRange = 4f;   // ระยะที่ต้องการรักษาระยะระหว่างต่อสู้ (Kite/Strafe)

    [Header("Standoff State")]
    public float standoffDuration = 1.5f;   // ระยะเวลาดูเชิง (วินาที)
    private float standoffTimer;
    public float standoffMovementRange = 1.0f; // ระยะเดินดูเชิงเล็กน้อย

    // ตัวแปรภายใน
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

        // พยายามหา Player โดยใช้ Tag "Player"
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        // ตั้งค่าเริ่มต้นสำหรับ Roaming และ Home Position
        homePosition = transform.position;
        // สุ่มค่าเริ่มต้นเพื่อไม่ให้ศัตรูทุกตัวเดินตามแพทเทิร์นเดียวกัน
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
            // เพิ่มการใช้ Raycast เพื่อตรวจจับสิ่งกีดขวาง (ตามความเป็นจริง ศัตรูควรเห็นแค่ในแนวสายตา)
            playerInSightRange = distanceToPlayer <= sightRange && HasLineOfSight(playerTarget.position);
            playerInAttackRange = distanceToPlayer <= attackRange;
        }
        else
        {
            playerInSightRange = false;
            playerInAttackRange = false;
        }
    }

    // ตรวจสอบแนวสายตา (Line of Sight)
    private bool HasLineOfSight(Vector3 targetPosition)
    {
        RaycastHit hit;
        Vector3 direction = targetPosition - transform.position;
        // ตรวจสอบว่ามีสิ่งกีดขวางระหว่างศัตรูกับผู้เล่นหรือไม่
        if (Physics.Raycast(transform.position, direction.normalized, out hit, sightRange))
        {
            // ตรวจสอบว่า Raycast ชน Player หรือไม่
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
        // ใช้ Perlin Noise (จำลอง Simplex Noise) เพื่อให้ได้ค่าการเปลี่ยนแปลงทิศทางที่ราบรื่น
        float angle = Mathf.PerlinNoise(noiseOffset, Time.time * noiseScale) * 360f;
        Vector3 direction = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));

        // ตรรกะถ่วงน้ำหนักกลับสู่จุดเกิด (Home Position) เมื่ออยู่ไกลเกินรัศมี (RoamRadius)
        float distanceToHome = Vector3.Distance(transform.position, homePosition);
        if (distanceToHome > roamRadius)
        {
            Vector3 homeDirection = (homePosition - transform.position).normalized;
            // ใช้ Lerp เพื่อถ่วงน้ำหนัก: ยิ่งไกลจาก Home มากเท่าไหร่ ก็ยิ่งถูกดึงกลับมากเท่านั้น
            direction = Vector3.Lerp(direction, homeDirection, (distanceToHome - roamRadius) / roamRadius);
        }

        return direction.normalized;
    }


    // --- STATE FUNCTIONS ---

    private void RoamingState()
    {
        agent.speed = maxRoamSpeed;
        agent.isStopped = false;

        // Transition: เห็น Player
        if (playerInSightRange)
        {
            currentState = EnemyState.Chase;
            return;
        }

        // Action: เคลื่อนที่ตามทิศทางที่คำนวณจาก Noise (เดินเตร่แบบธรรมชาติ)
        Vector3 roamDirection = GetRoamDirection();
        // ตั้งจุดหมายเป็นทิศทางข้างหน้าเล็กน้อยเพื่อให้ NavMeshAgent คำนวณเส้นทาง
        agent.SetDestination(transform.position + roamDirection * 5f);
    }

    private void ChaseState()
    {
        agent.speed = maxChaseSpeed;

        // Transition 1: เข้าสู่ระยะโจมตี -> ดูเชิง/โจมตี
        if (playerInAttackRange)
        {
            currentState = EnemyState.Standoff;
            standoffTimer = standoffDuration;
            return;
        }

        // Transition 2: Player หนีออกนอกระยะสายตา -> โรม
        if (!playerInSightRange)
        {
            currentState = EnemyState.Roaming;
            return;
        }

        // Action: ไล่ล่า/Kiting (รักษาระยะให้เข้าสู่ Optimal Combat Range)
        if (playerTarget != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

            if (distanceToPlayer > optimalCombatRange)
            {
                // ถ้าไกลเกินไป: วิ่งเข้าหา Player
                agent.SetDestination(playerTarget.position);
            }
            else // (อยู่ในช่วง AttackRange ถึง OptimalCombatRange)
            {
                // ถ้าอยู่ในระยะที่เหมาะสม (Optimal Range): เดินโฉบ/ถอยหลัง (Kite/Strafe) 
                // เพื่อรักษาระยะและเตรียมเข้า Standoff
                Vector3 targetDirection = (playerTarget.position - transform.position).normalized;
                // คำนวณจุดหมายที่อยู่ห่างจาก Player ออกมาเล็กน้อย
                Vector3 kiteDestination = playerTarget.position - targetDirection * optimalCombatRange * 0.8f;

                agent.SetDestination(kiteDestination);
            }
            agent.isStopped = false;
        }
    }

    private void StandoffState()
    {
        // Transition 1: เวลาหมด -> โจมตี
        if (standoffTimer <= 0)
        {
            currentState = EnemyState.Attack;
            agent.isStopped = true; // หยุดเคลื่อนที่เพื่อโจมตี
            return;
        }

        // Transition 2: Player หนีออกนอกระยะสายตา -> โรม
        if (!playerInSightRange)
        {
            currentState = EnemyState.Roaming;
            agent.isStopped = false;
            return;
        }

        // Action: ดูเชิง
        standoffTimer -= Time.deltaTime;
        FaceTarget(playerTarget.position);

        // เดินดูเชิงเล็กน้อย (SetNewStandoffMovePoint จะต้องถูกเรียกเมื่อถึงจุดหมายเดิมแล้ว)
        if (agent.remainingDistance < 0.1f && !agent.pathPending)
        {
            SetNewStandoffMovePoint();
        }
    }

    private void SetNewStandoffMovePoint()
    {
        // สุ่มจุดเล็กๆ รอบตัวศัตรู เพื่อให้เกิดการ "โฉบ/Strafe" ในระยะสั้น
        Vector3 randomOffset = Random.insideUnitSphere * standoffMovementRange;
        randomOffset.y = 0;
        Vector3 targetPosition = transform.position + randomOffset;

        NavMeshHit hit;
        // ใช้ SamplePosition เพื่อให้มั่นใจว่าจุดนั้นอยู่บน NavMesh
        if (NavMesh.SamplePosition(targetPosition, out hit, standoffMovementRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void AttackState()
    {
        agent.isStopped = true; // หยุดเคลื่อนที่เมื่ออยู่ใน Attack State

        // Transition 1: Player หนีออกนอกระยะโจมตี -> ไล่ล่า
        if (!playerInAttackRange && playerInSightRange)
        {
            currentState = EnemyState.Chase;
            agent.isStopped = false;
            return;
        }

        // Transition 2: Player หนีออกนอกระยะสายตาไปเลย -> โรม
        if (!playerInSightRange)
        {
            currentState = EnemyState.Roaming;
            agent.isStopped = false;
            return;
        }

        // Action: โจมตี
        FaceTarget(playerTarget.position);
        // TODO: ใส่ Logic การโจมตี (ยิง, ฟัน) ที่นี่
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


        //กลับไป stest เดิน
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
        // กำหนดตำแหน่งเริ่มต้น
        Vector3 position = transform.position;

        // 1. วาด Roam Radius (รัศมีการเดินเตร่)
        // แสดงอาณาเขตการเดินเตร่ที่ถูกถ่วงน้ำหนัก
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(homePosition, roamRadius);

        // 2. วาด Sight Range (ระยะการมองเห็น)
        // เมื่อเลือก Object ศัตรูใน Scene จะเห็นเป็นวงกลมสีฟ้า
        Gizmos.color = new Color(0.2f, 0.2f, 1f, 0.5f); // สีฟ้าอ่อน
        Gizmos.DrawWireSphere(position, sightRange);

        // 3. วาด Optimal Combat Range (ระยะรักษาระยะต่อสู้)
        // ระยะที่ศัตรูจะเริ่ม Kiting/Strafing
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(position, optimalCombatRange);

        // 4. วาด Attack Range (ระยะโจมตี)
        // ระยะที่ศัตรูจะเปลี่ยนไป State Standoff/Attack
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(position, attackRange);
    }
}