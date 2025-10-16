// EnemyMovement.cs
using com.cyborgAssets.inspectorButtonPro;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemyMovement : MonoBehaviour , IKnockbackable
{
    [Header("Roaming")]
    public float roamRadius = 20f;
    public float waitRoamTime = 2;
    public float roamSpeed = 3.6f;
    public float chaseSpeed = 5f;
    

    private Vector3 _roamPoint;
    private BaseEnemyAI.EnemyState _currentState;
    private NavMeshAgent _agent;
    private Rigidbody _rb;
    private BaseEnemyAI _aiController;
    private bool _isWaiting;
    private float _timerWaiting;

    //public KnockbackableStat knockbackableStat;
    //KnockbackableStat
    [Header("Knockbackable")]
    public bool canKnockback = true;
    private Coroutine KnockbackCoroutine;
    [Range(0.001f, 0.1f)][SerializeField] private float StillThreshold = 0.05f;
    [SerializeField] private float MaxKnockbackTime = 0.5f;

    private Coroutine _dashCoroutine;
    [Header("Dash Settings")]
    [SerializeField] private float _dashStoppingThreshold = 0.5f; // ค่าความเร็วต่ำสุดก่อนหยุด Dash


    private void Awake()
    {
        // *** จัดการตัวเอง: หา Reference ที่จำเป็นทั้งหมด ***
        _agent = GetComponent<NavMeshAgent>();
        _aiController = GetComponent<BaseEnemyAI>();
        _rb = GetComponent<Rigidbody>();

        // Safety Check
        if (_agent == null || _aiController == null)
        {
            Debug.LogError($"{GetType().Name} requires NavMeshAgent and BaseEnemyAI on the same GameObject.");
            enabled = false;
            return;
        }

    }

    private void OnEnable()
    {
        // *** สมัครรับ Events จาก BaseEnemyAI เพื่อรับคำสั่ง ***
        _aiController.OnStartChase += HandleStartChase;
        _aiController.OnStopMovement += HandleStopMovement;
        _aiController.OnStateChange += HandleStateChange;
    }

    private void OnDisable()
    {
        // ยกเลิกการสมัครรับ Event เมื่อ Object ถูกปิดการใช้งาน
        if (_aiController != null)
        {
            _aiController.OnStartChase -= HandleStartChase;
            _aiController.OnStopMovement -= HandleStopMovement;
        }
    }

    private void HandleStateChange(BaseEnemyAI.EnemyState state)
    {
        _currentState = state;
        if (state == BaseEnemyAI.EnemyState.Roaming)
        {
            _isWaiting = false;
            _agent.speed = roamSpeed;
        }

        if(state == BaseEnemyAI.EnemyState.Chase) _agent.speed = chaseSpeed;
    }

    private void HandleStartChase(Vector3 targetPosition)
    {
        MoveToTarget(targetPosition);
    }

    private void HandleStopMovement()
    {
        StopMovement();
    }

    private void Start()
    {
        _roamPoint = transform.position;
    }

    void Update()
    {
        // Logic การ Roaming: ถ้าอยู่ใน Roaming State และถึงจุดหมายแล้ว ให้สุ่มจุดใหม่
        if (_aiController.currentState == BaseEnemyAI.EnemyState.Roaming && IsAtDestination())
        {
            if (!_isWaiting)
            {
                _isWaiting = true;
                _timerWaiting = waitRoamTime;
            }
            else
            {
                if (_timerWaiting > 0)
                {
                    _timerWaiting -= Time.deltaTime;
                }
                else
                {
                    SetNewRoamPoint(_roamPoint);

                    _isWaiting = false;
                }
            }

            //if (_timerWaiting > 0)
            //{
            //    _timerWaiting -= Time.deltaTime;
            //}
            //else
            //{
            //    SetNewRoamPoint(_roamPoint);
            //    _timerWaiting = waitRoamTime;
            //}
        }
    }

    // --- Event Handlers (ถูกเรียกโดย BaseEnemyAI ผ่าน Events) ---

    private void MoveToTarget(Vector3 targetPosition)
    {
        _agent.isStopped = false;
        _agent.SetDestination(targetPosition);
        //Debug.LogWarning($"MoveToTarget {targetPosition}");
    }

    private void StopMovement()
    {
        _agent.isStopped = true;
    }

    // --- Roaming Logic ---

    public void StartRoaming(Vector3 centerPosition)
    {
        _agent.isStopped = false;
        SetNewRoamPoint(centerPosition);
    }

    private bool IsAtDestination()
    {
        // ตรวจสอบว่าถึงจุดหมายแล้วหรือไม่
        return _agent.remainingDistance <= _agent.stoppingDistance && !_agent.pathPending;
    }

    private void SetNewRoamPoint(Vector3 centerPosition)
    {


        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * roamRadius;
        randomDirection += centerPosition;
        NavMeshHit hit;

        // หาจุดที่ถูกต้องบน NavMesh
        if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
    }

    public void GetKnockedBack(Vector3 direction, float force)
    {
        if (!canKnockback) return;

        if (KnockbackCoroutine != null) StopCoroutine(KnockbackCoroutine);
        KnockbackCoroutine = StartCoroutine(ApplyKnockback(direction, force));
    }

    private IEnumerator ApplyKnockback(Vector3 direction, float force)
    {
        Debug.Log($"ApplyKnockback : {direction} | {force}");

        yield return null;
        _agent.isStopped = true;
        //_agent.enabled = false;
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
        _agent.Warp(transform.position);
        //_agent.enabled = true;
        _agent.isStopped = false;

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

    public void SkillDash(Vector3 direction, float speed, float duration)
    {
        // หยุด Coroutine เก่า (ถ้ามี)
        if (_dashCoroutine != null) StopCoroutine(_dashCoroutine);

        // หยุดการนำทางของ NavMeshAgent
        StopMovement();

        // เริ่ม Coroutine Dash
        _dashCoroutine = StartCoroutine(ApplySkillDash(direction, speed, duration));
    }

    private IEnumerator ApplySkillDash(Vector3 direction, float speed, float duration)
    {
        // 1. ปิด NavMeshAgent และเตรียม Rigidbody
        _agent.isStopped = true;
        //_agent.enabled = false;
        _rb.isKinematic = false;
        _rb.useGravity = false; // ปิด Gravity ชั่วคราวเพื่อให้พุ่งตรง

        // 2. กำหนดความเร็วเริ่มต้น
        Vector3 dashVelocity = direction * speed;
        _rb.linearVelocity = dashVelocity;

        float startTime = Time.time;

        // 3. Loop การพุ่ง
        while (Time.time < startTime + duration && _rb.linearVelocity.magnitude > _dashStoppingThreshold)
        {
            // รักษาความเร็วในการพุ่ง
            if (_rb.linearVelocity.magnitude > speed)
            {
                _rb.linearVelocity = _rb.linearVelocity.normalized * speed;
            }

            yield return null; // รอจนกว่าจะถึงเฟรมถัดไป
        }

        // 4. จบการพุ่ง
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.useGravity = true; // คืนค่า Gravity (ถ้าจำเป็น)
        _rb.isKinematic = true;

        // 5. เปิด NavMeshAgent คืน
        //_agent.enabled = true;
        _agent.Warp(transform.position); // Warp เพื่อปรับตำแหน่ง Agent ให้ตรงกับ Rigidbody
        _agent.isStopped = false;

        _dashCoroutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_roamPoint, roamRadius);
    }

    [ProButton]
    public void TestIsStop(bool resu)
    {
        _agent.isStopped = resu;
    }

}