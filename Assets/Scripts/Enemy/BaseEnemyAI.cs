// BaseEnemyAI.cs
using UnityEngine;
using UnityEngine.AI;
using System;
using static UnityEngine.CullingGroup;

// ต้องการ NavMeshAgent เท่านั้น Component อื่นจะหาและสมัครรับ Event เอง
[RequireComponent(typeof(NavMeshAgent))]
public class BaseEnemyAI : MonoBehaviour 
{
    public enum EnemyState { Roaming, Chase, Attack }

    [Header("Base AI State")]
    public EnemyState currentState;

    // Events สำหรับการสั่ง Component อื่น
    public event Action<EnemyState> OnStateChange;
    public event Action<Vector3> OnStartChase;
    public event Action OnStopMovement;
    public event Action<bool> OnAttackStateChange; // สำหรับ Animation/Visuals
    public event Action<bool> OnStartAttackSequence; // สั่ง Combat ให้เริ่มโจมตี (พร้อม flag พิเศษ)

    // Events ภายในสำหรับ Logic การต่อสู้ (Combat จะ Invoke กลับมา)
    public event Action OnAttackFinished;

    [Header("Base References")]
    protected NavMeshAgent _agent;
    public Transform playerTarget;

    [Header("Base AI Settings")]
    public float sightRange = 15f;
    public float attackRange = 2f;

    protected bool _playerInSightRange;
    protected bool _playerInAttackRange;
    private BaseEnemyCombat _enemyCombat;

    protected virtual void Awake()
    {
        // AI หา Agent ที่จำเป็นต้องใช้เอง
        _agent = GetComponent<NavMeshAgent>();

        // หา Combat Component เพื่อสมัครรับ Event จบการโจมตี (ถ้ามี)
        _enemyCombat = GetComponent<BaseEnemyCombat>();
        
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
                //if (enemyCombat == null) AttackChangeStateLogic();
                // ไม่มี Logic ใน Update() แล้ว
                // AI จะติดค้างที่นี่จนกว่า HandleAttackFinished() จะถูกเรียก

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

    // --- Event Handlers (Subscriber) ---

    protected virtual void HandleAttackFinished()
    {
        // 1. ถ้ายังอยู่ในระยะโจมตี -> โจมตีซ้ำ
        if (_playerInAttackRange)
        {
            ChangeState(EnemyState.Attack);
        }
        // 2. ถ้าหลุดระยะโจมตี แต่ยังเห็นอยู่ -> ไล่ล่า
        else if (_playerInSightRange)
        {
            ChangeState(EnemyState.Chase);
        }
        // 3. ถ้ามองไม่เห็น -> Roam
        else
        {
            ChangeState(EnemyState.Roaming);
        }
    }
}