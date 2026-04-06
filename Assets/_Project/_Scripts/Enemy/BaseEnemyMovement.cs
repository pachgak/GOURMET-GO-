// EnemyMovement.cs
using com.cyborgAssets.inspectorButtonPro;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

[RequireComponent(typeof(Rigidbody))]
public class BaseEnemyMovement : MonoBehaviour , IKnockbackable
{
    [Header("Roaming")]
    public float roamRadius = 20f;
    public float waitRoamTime = 2;
    public float roamSpeed = 3.6f;
    public float chaseSpeed = 5f;


    protected Vector3 _roamPoint;
    protected BaseEnemyAI.EnemyState _currentState;
    protected NavMeshAgent _agent;
    protected Rigidbody _rb;
    protected BaseEnemyAI _aiController;
    protected EnemyHealth _enemyHealth;

    [SerializeField] protected bool _isWaiting;
    protected float _timerWaiting;

    //public KnockbackableStat knockbackableStat;
    //KnockbackableStat
    [Header("Knockbackable")]
    public bool canKnockback = true;
    bool IKnockbackable._canKnockback { get => canKnockback; set => canKnockback = value; }
    protected Coroutine KnockbackCoroutine;
    protected Coroutine _jumpCoroutine;

    //[Range(0.001f, 0.1f)][SerializeField] private float StillThreshold = 0.05f;
    //[SerializeField] private float MaxKnockbackTime = 0.5f;

    [SerializeField] protected float knockbackMultiplier = 1f;
    float IKnockbackable._knockbackMultiplier { get => knockbackMultiplier; set => knockbackMultiplier = value; }

    protected Coroutine _dashCoroutine;
    [Header("Dash Settings")]
    [SerializeField] protected float _dashStoppingThreshold = 0.5f; // ค่าความเร็วต่ำสุดก่อนหยุด Dash


    protected virtual void Awake()
    {
        // *** จัดการตัวเอง: หา Reference ที่จำเป็นทั้งหมด ***
        _agent = GetComponent<NavMeshAgent>();
        _aiController = GetComponent<BaseEnemyAI>();
        _rb = GetComponent<Rigidbody>();
        _enemyHealth = GetComponent<EnemyHealth>();

        // Safety Check
        if (_agent == null || _aiController == null)
        {
            Debug.LogError($"{GetType().Name} requires NavMeshAgent and BaseEnemyAI on the same GameObject.");
            enabled = false;
            return;
        }

        _rb.isKinematic = true;
        //_rb.freezeRotation = true;
    }

    protected virtual void OnEnable()
    {
        // *** สมัครรับ Events จาก BaseEnemyAI เพื่อรับคำสั่ง ***
        if (_aiController != null)
        {
            _aiController.OnStartChase += HandleStartChase;
        _aiController.OnStopMovement += HandleStopMovement;
        _aiController.OnStateChange += HandleStateChange;

        }

        if (_enemyHealth != null)
        {
            _enemyHealth.OnDie += HandleOnDie;

        }
    }

    protected virtual void OnDisable()
    {
        // ยกเลิกการสมัครรับ Event เมื่อ Object ถูกปิดการใช้งาน
        if (_aiController != null)
        {
            _aiController.OnStartChase -= HandleStartChase;
            _aiController.OnStopMovement -= HandleStopMovement;
            _aiController.OnStateChange -= HandleStateChange;
        }
        if (_enemyHealth != null)
        {
            _enemyHealth.OnDie -= HandleOnDie;
        }

        if (KnockbackCoroutine != null) StopCoroutine(KnockbackCoroutine);

    }

    protected void HandleOnDie()
    {
        _agent.SetDestination(transform.position);
    }

    protected void HandleStateChange(BaseEnemyAI.EnemyState state)
    {
        _currentState = state;
        if (state == BaseEnemyAI.EnemyState.Roaming)
        {
            _isWaiting = false;
            _agent.speed = roamSpeed;
        }

        if(state == BaseEnemyAI.EnemyState.Chase) _agent.speed = chaseSpeed;
    }

    protected void HandleStartChase(Vector3 targetPosition)
    {
        MoveToTarget(targetPosition);
    }

    protected void HandleStopMovement()
    {
        StopMovement();
    }

    protected void Start()
    {
        _roamPoint = transform.position;
    }

    protected void Update()
    {

        if (_enemyHealth != null && _enemyHealth.isDead) return;

        if (!_agent.isActiveAndEnabled) return;

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

    protected void MoveToTarget(Vector3 targetPosition)
    {
        // *** เพิ่ม Safety Check: ทำงานเมื่อ Agent เปิดอยู่และอยู่บน NavMesh เท่านั้น ***
        if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = false;
            _agent.SetDestination(targetPosition);
        }
    }

    protected void StopMovement()
    {
        // *** เพิ่ม Safety Check: ป้องกันบั๊ก "Stop" can only be called on an active agent... ***
        if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
        }
    }

    // --- Roaming Logic ---

    public void StartRoaming(Vector3 centerPosition)
    {
        _agent.isStopped = false;
        SetNewRoamPoint(centerPosition);
    }

    protected bool IsAtDestination()
    {
        // เพิ่มการเช็ค isOnNavMesh เพื่อป้องกัน Error GetRemainingDistance
        if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
            return false;

        // ตรวจสอบว่าถึงจุดหมายแล้วหรือไม่
        return _agent.remainingDistance <= _agent.stoppingDistance && !_agent.pathPending;
    }

    protected void SetNewRoamPoint(Vector3 centerPosition)
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

    public void GetKnockedBack(Vector3 direction, float force, float time)
    {
        //Debug.Log($"CanKnockback : {canKnockback}");

        if (!canKnockback) return;

        if (!gameObject.activeSelf) return;
        if (_enemyHealth != null && _enemyHealth.isDead) return;

        if (KnockbackCoroutine != null) StopCoroutine(KnockbackCoroutine);
        KnockbackCoroutine = StartCoroutine(ApplyKnockback(direction, force, time));
    }

    protected IEnumerator ApplyKnockback(Vector3 direction, float force,float time)
    {

        float finalForce = force * knockbackMultiplier;

        // ถ้า multiplier เป็น 0 หรือแรงเป็น 0 ไม่ต้องทำอะไร
        if (finalForce <= 0) yield break;

        _agent.enabled = false;
        _rb.isKinematic = false;
        // ปิด Gravity ชั่วคราวเพื่อให้กระเด็นในแนวราบได้อย่างแม่นยำ
        _rb.useGravity = false;

        float timer = 0;
        Vector3 knockbackVelocity = direction.normalized * finalForce;

        while (timer < time)
        {
            // คุมความเร็วให้คงที่ตลอดเวลาที่กำหนด
            _rb.linearVelocity = knockbackVelocity;

            timer += Time.deltaTime;
            yield return null;
        }

        // จบการพุ่ง
        _rb.linearVelocity = Vector3.zero;
        _rb.isKinematic = true;

        // วาง Agent ลงตำแหน่งปัจจุบันก่อนเปิดใช้งาน
        _agent.Warp(transform.position);
        _agent.enabled = true;

        KnockbackCoroutine = null;

        //yield return null;
        //_agent.isStopped = true;
        ////_agent.enabled = false;
        //_rb.useGravity = true;
        //_rb.isKinematic = false;

        //_rb.AddForce(direction * force, ForceMode.Impulse);

        //yield return new WaitForFixedUpdate();
        //float knockbackTime = Time.time;
        //yield return new WaitUntil(
        //    () => _rb.linearVelocity.magnitude < StillThreshold || Time.time > knockbackTime + MaxKnockbackTime
        //);
        //yield return new WaitForSeconds(0.25f);

        //_rb.linearVelocity = Vector3.zero;
        //_rb.angularVelocity = Vector3.zero;
        //_rb.useGravity = false;
        //_rb.isKinematic = true;
        //_agent.Warp(transform.position);
        ////_agent.enabled = true;
        //_agent.isStopped = false;

        //yield return null;


        ////กลับไป stest เดิน
        ////if (Player != null)
        ////{
        ////    KnockbackCoroutine = StartCoroutine(ChasePlayer(Player));
        ////}
        ////else
        ////{
        ////    KnockbackCoroutine = StartCoroutine(Roam());
        ////}
    }

    // แก้ไข Signature ของ SkillDash ให้รับค่า isInvincibleDash เพิ่มเติม (ค่าเริ่มต้นเป็น false)
    public void SkillDash(Vector3 direction, float speed, float duration, bool isInvincibleDash = false)
    {
        // หยุด Coroutine เก่า (ถ้ามี)
        if (_dashCoroutine != null) StopCoroutine(_dashCoroutine);

        // หยุดการนำทางของ NavMeshAgent
        StopMovement();

        // เริ่ม Coroutine Dash
        _dashCoroutine = StartCoroutine(ApplySkillDash(direction, speed, duration, isInvincibleDash));
    }

    // แก้ไข Coroutine ให้รับค่ามาด้วย
    protected IEnumerator ApplySkillDash(Vector3 direction, float speed, float duration, bool isInvincibleDash)
    {
        // 1. ปิด NavMeshAgent และเตรียม Rigidbody
        _agent.isStopped = true;
        _rb.isKinematic = false;
        _rb.useGravity = false;

        // *** เพิ่ม Logic: ป้องกัน Knockback ระหว่าง Dash ถ้า isInvincibleDash เป็น true ***
        bool originalCanKnockback = canKnockback;
        if (isInvincibleDash)
        {
            canKnockback = false;
        }

        // 2. กำหนดความเร็วเริ่มต้น
        Vector3 dashVelocity = direction * speed;
        _rb.linearVelocity = dashVelocity;

        float startTime = Time.time;

        // 3. Loop การพุ่ง
        while (Time.time < startTime + duration && _rb.linearVelocity.magnitude > _dashStoppingThreshold)
        {
            if (_rb.linearVelocity.magnitude > speed)
            {
                _rb.linearVelocity = _rb.linearVelocity.normalized * speed;
            }

            yield return null;
        }

        // 4. จบการพุ่ง
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.useGravity = true;
        _rb.isKinematic = true;

        // 5. เปิด NavMeshAgent คืน
        _agent.Warp(transform.position);
        _agent.isStopped = false;

        // *** เพิ่ม Logic: คืนค่าการรับ Knockback หลังพุ่งจบ ***
        if (isInvincibleDash)
        {
            canKnockback = originalCanKnockback;
        }

        _dashCoroutine = null;
    }

    public void StopDashImmediately()
    {
        // 1. หยุด Coroutine Dash
        if (_dashCoroutine != null) StopCoroutine(_dashCoroutine);
        _dashCoroutine = null;

        // 2. หยุดแรงฟิสิกส์
        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.useGravity = true;
            _rb.isKinematic = true;
        }

        // 3. คืนค่า Agent
        if (_agent != null)
        {
            _agent.Warp(transform.position);
            _agent.enabled = true;
            _agent.isStopped = true; // หยุดเดินด้วย
        }
    }

    // ฟังก์ชันสั่งกระโดด
    public void SkillJump(Vector3 targetPosition, float jumpHeight, float duration)
    {
        if (_jumpCoroutine != null) StopCoroutine(_jumpCoroutine);
        _jumpCoroutine = StartCoroutine(JumpRoutine(targetPosition, jumpHeight, duration));
    }

    protected System.Collections.IEnumerator JumpRoutine(Vector3 targetPosition, float jumpHeight, float duration)
    {
        // 1. ปิด Agent และเก็บค่าเริ่มต้น
        if (_agent != null) _agent.enabled = false;

        Vector3 startPos = transform.position;
        float timePassed = 0f;

        // 2. ลูปคำนวณตำแหน่งแบบ Parabola
        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            // คำนวณความคืบหน้า (0 ถึง 1)
            float percent = Mathf.Clamp01(timePassed / duration);

            // คำนวณตำแหน่ง X, Z (พุ่งไปข้างหน้า) แบบ Linear
            Vector3 currentPos = Vector3.Lerp(startPos, targetPosition, percent);

            // คำนวณแกน Y (ความสูง) ด้วยสมการ Parabola (โค้งระฆังคว่ำ)
            // สูตร: 4 * h * p * (1 - p) -> จะได้ค่า 0 ตอนเริ่ม, พุ่งสูงสุดตอนกลาง, และ 0 ตอนจบ
            float parabolaY = jumpHeight * (percent * (1f - percent) * 4f);
            currentPos.y += parabolaY;

            // อัปเดตตำแหน่ง
            transform.position = currentPos;

            yield return null;
        }

        // 3. จบการกระโดด ตกถึงพื้นพอดี
        transform.position = targetPosition;
        if (_agent != null)
        {
            _agent.Warp(targetPosition); // Sync ตำแหน่งกับ NavMesh
            _agent.enabled = true;
        }
    }

    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if(_roamPoint != Vector3.zero) Gizmos.DrawWireSphere(_roamPoint, roamRadius);
    }

    [ProButton] 
    public void TestIsStop(bool resu)
    {
        _agent.isStopped = resu;
    }



}