// BaseEnemyAI.cs
using UnityEngine;
using UnityEngine.AI;
using System;
using static UnityEngine.CullingGroup;

// ต้องการ NavMeshAgent เท่านั้น Component อื่นจะหาและสมัครรับ Event เอง
[RequireComponent(typeof(NavMeshAgent))]
public class BaseEnemyAI : MonoBehaviour 
{
    public enum EnemyState { Roaming, Chase, Attack, Standby }

    [Header("Base AI State")]
    public EnemyState currentState;

    // Events สำหรับการสั่ง Component อื่น
    public event Action<EnemyState> OnStateChange;
    public event Action<Vector3> OnStartChase;
    public event Action OnStopMovement;
    //public event Action<bool> OnAttackStateChange; // สำหรับ Animation/Visuals
    //public event Action<bool> OnStartAttackSequence; // สั่ง Combat ให้เริ่มโจมตี (พร้อม flag พิเศษ)

    [Header("Base References")]
    protected NavMeshAgent _agent;
    public Transform playerTarget;

    [Header("Base AI Settings")]
    public float sightRange = 15f;
    public float attackRange = 2f;

    //[field: Header("Status Effects")]
    [field: SerializeField]
    public bool IsStunned { get; protected set; } = false; // ให้ลูกๆ อ่านค่าได้
    protected Coroutine _stunCoroutine;
    // *** 1. เพิ่ม Event ใหม่ตรงนี้ ***
    public event Action<bool> OnStunStateChanged;

    [SerializeField] protected bool _playerInSightRange;
    [SerializeField] protected bool _playerInAttackRange;
    protected BaseEnemyCombat _enemyCombat;
    protected EnemyHealth _enemyHealth;

    protected virtual void Awake()
    {
        // AI หา Agent ที่จำเป็นต้องใช้เอง
        _agent = GetComponent<NavMeshAgent>();
        _enemyHealth = GetComponent<EnemyHealth>();
        // หา Combat Component เพื่อสมัครรับ Event จบการโจมตี (ถ้ามี)
        _enemyCombat = GetComponent<BaseEnemyCombat>();
        
        if(playerTarget == null) playerTarget = GameObject.FindWithTag("Player").transform;
    }

    protected virtual void OnEnable()
    {
        if (_enemyCombat != null)
        {
            _enemyCombat.OnAttackFinished += HandleAttackFinished;
        }
    }

    protected virtual void OnDisable()
    {
        // ยกเลิกการสมัครรับเมื่อปิดการใช้งาน
        if (_enemyCombat != null)
        {
            _enemyCombat.OnAttackFinished -= HandleAttackFinished;
        }
    }


    protected virtual void Start()
    {
        // ... (สมมติว่ามีการหา playerTarget ที่นี่) ...
        ChangeState(EnemyState.Roaming);
    }

    protected virtual void Update()
    {
        if (_enemyHealth != null && _enemyHealth.isDead) return;

        if (!_agent.isActiveAndEnabled) return;

        // ถ้ามึนอยู่ ห้ามคิด ห้ามเปลี่ยน State ห้ามเดิน
        if (IsStunned) return;

        CheckPlayerDistance();

        switch (currentState)
        {
            case EnemyState.Roaming:
                if (_playerInSightRange) ChangeState(EnemyState.Chase);
                break;
            case EnemyState.Chase:
                ChaseChangeStateLogic();
                break;
            case EnemyState.Attack:
                // รอจนกว่าจะตีเสร็จ
                break;
            case EnemyState.Standby:
                // *** 2. เรียกใช้ Logic ของ Standby ตรงนี้ ***
                StandbyChangeStateLogic();
                break;
        }
    }

    // ----------------------------------------------------------------------
    // ตรวจสอบระยะทาง (ตามที่ร้องขอ)
    // ----------------------------------------------------------------------

    protected virtual void CheckPlayerDistance()
    {
        if (playerTarget == null)
        {
            _playerInSightRange = false;
            _playerInAttackRange = false;
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        _playerInSightRange = distance <= sightRange;
        _playerInAttackRange = distance <= attackRange;
    }

    // ----------------------------------------------------------------------
    // Logic การเปลี่ยน State
    // ----------------------------------------------------------------------

    protected virtual void ChaseChangeStateLogic()
    {
        if (_playerInAttackRange)
        {
            ChangeState(EnemyState.Attack);
        }
        else if (!_playerInSightRange)
        {
            ChangeState(EnemyState.Roaming);
        }
        else
        {
            TriggerStartChase(playerTarget.position);
            //OnStartChase?.Invoke(playerTarget.position); // สั่ง Movement ไล่ล่า
        }
    }

    protected void TriggerStartChase(Vector3 targetPos)
    {
        OnStartChase?.Invoke(targetPos);
    }

    //protected virtual void AttackChangeStateLogic()
    //{
    //    if (playerInAttackRange)
    //    {
    //        //Not thing happan
    //        //ChangeState(EnemyState.Attack);
    //    }
    //    else if (playerInSightRange)
    //    {
    //        ChangeState(EnemyState.Chase);
    //    }
    //    else
    //    {
    //        ChangeState(EnemyState.Roaming);
    //    }
    //}

    public virtual void TriggerChangeState(EnemyState newState)
    {
        ChangeState(newState);
    }

    protected virtual void ChangeState(EnemyState newState)
    {

        currentState = newState;
        OnStateChange?.Invoke(currentState);

        switch (newState)
        {
            case EnemyState.Roaming:
                //OnAttackStateChange?.Invoke(false);
                break;
            case EnemyState.Chase:
                //OnAttackStateChange?.Invoke(false);
                break;
            case EnemyState.Attack:
                OnStopMovement?.Invoke();
                //OnAttackStateChange?.Invoke(true);

                // 1. ตรวจสอบ: ถ้าไม่มี Combat Component
                if (_enemyCombat == null)
                {
                    Debug.LogWarning($"{gameObject.name} entered AttackState but has no BaseEnemyCombat. Exiting immediately.");
                    // เรียก HandleAttackFinished ทันที เพื่อให้ Update() รอบถัดไปเปลี่ยน State
                    //HandleAttackFinished();
                    if (_playerInSightRange)
                    {
                        ChangeState(EnemyState.Chase);
                    }
                    else
                    {
                        ChangeState(EnemyState.Roaming);
                    }
                }
                else
                {
                    //// 2. ถ้ามี: สั่ง Combat ให้เริ่มโจมตี (ผ่าน Event)
                    //OnStartAttackSequence?.Invoke(false);
                    //// AI จะค้างอยู่ใน Attack State รอ OnAttackFinished จาก Combat
                }
                break;
        }
    }

    protected virtual void StandbyChangeStateLogic()
    {
        // พฤติกรรมมาตรฐานของ Base (เช่น หมี BearAI ก็ให้ใช้พฤติกรรมนี้เลย ไม่ต้องเขียนทับ)
        // คือ "หยุดเดินแล้วยืนจ้องหน้า" จนกว่าจะพร้อมโจมตีอีกรอบ
        TriggerStopMovement();

        // ตัวอย่างการออกจาก Standby: ถ้า Attack Cooldown ใน Combat หมดแล้ว ก็ให้กลับไป Chase/Attack
        if (_enemyCombat != null && _enemyCombat.attackTimer <= 0)
        {
            ChangeState(EnemyState.Chase);
        }
    }


    // --- Event Handlers (Subscriber) ---

    protected virtual void HandleAttackFinished()
    {
        //// 1. ถ้ายังอยู่ในระยะโจมตี -> โจมตีซ้ำ
        //if (_playerInAttackRange)
        //{
        //    ChangeState(EnemyState.Attack);
        //}
        //// 2. ถ้าหลุดระยะโจมตี แต่ยังเห็นอยู่ -> ไล่ล่า
        //else if (_playerInSightRange)
        //{
        //    ChangeState(EnemyState.Chase);
        //}
        //// 3. ถ้ามองไม่เห็น -> Roam
        //else
        //{
        //    ChangeState(EnemyState.Roaming);
        //}

        ChangeState(EnemyState.Standby);
    }

    protected void TriggerStopMovement()
    {
        OnStopMovement?.Invoke();
    }

    // ประกาศเป็น virtual เผื่อลูกบางตัวอยากสตันแบบแปลกๆ
    public virtual void ApplyStun(float duration = 3.0f)
    {
        if (_stunCoroutine != null) StopCoroutine(_stunCoroutine);
        _stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    protected virtual System.Collections.IEnumerator StunRoutine(float duration)
    {
        IsStunned = true;

        // *** 2. แจ้งเตือนทุกคนว่า "ติดสตันแล้วนะ!" ***
        OnStunStateChanged?.Invoke(true);

        TriggerStopMovement(); // สั่งหยุดเดิน 

        Debug.Log($"<color=cyan>{gameObject.name} is STUNNED for {duration} secs!</color>");

        yield return new WaitForSeconds(duration);

        IsStunned = false;

        // *** 3. แจ้งเตือนทุกคนว่า "ตื่นจากสตันแล้ว!" ***
        OnStunStateChanged?.Invoke(false);

        Debug.Log($"<color=cyan>{gameObject.name} woke up!</color>");

        ChangeState(EnemyState.Chase); // กลับไปไล่ล่า
    }

    protected virtual void OnDrawGizmosSelected()
    {
        // วงกลมระยะการมองเห็น (Sight Range)
        Gizmos.color = Color.yellow; // สีเหลืองสำหรับระยะการมองเห็น
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // วงกลมระยะการโจมตี (Attack Range)
        Gizmos.color = Color.red; // สีแดงสำหรับระยะการโจมตี
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}