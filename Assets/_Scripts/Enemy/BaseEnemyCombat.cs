// BaseEnemyCombat.cs
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.AI;
using Unity.VisualScripting;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

public class BaseEnemyCombat : MonoBehaviour
{
    public LayerMask attackMask;

    public float attackCooldown;
    public float attackTimer;

    // Events ที่จะถูก Invoke กลับไปหา AI เมื่อโจมตีเสร็จ
    public event Action OnAttackFinished;
    public event Action<int, float> OnSkillUesd;
    public event Action<Vector3> OnSkillActionExecuted;
    //public event Action OnSkillEnd;

    public EnemySkillSO[] enemySkills;
    //public EnemySkill[] enemySkills;

    // ตัวแปรใหม่: เก็บ Skill ที่กำลังใช้อยู่ เพื่อให้รู้ว่าต้องดึง Action ตัวไหน
    protected EnemySkillSO currentActiveSkill;
    protected bool isSkillAnimating = false; // ตัวเช็คว่าจบหรือยัง
    public Vector3 currentDiractionSkill;
    protected float currentSpeedMultiplier;

    protected int _currentSkillIndex = 0;

    //[System.Serializable]
    //public class EnemySkill
    //{
    //    public AttacksSkill.SkillSetp[] _skillSetp;
    //}

    protected NavMeshAgent _agent;
    //protected Transform _playerTarget;
    protected Coroutine _attackSequenceCoroutine;
    protected BaseEnemyAI _aiController;
    protected BaseEnemyMovement _enemyMovement;
    protected EnemyHealth _enemyHealth;



    protected virtual void Awake()
    {
        // *** จัดการตัวเอง: หา Reference ที่จำเป็นทั้งหมด ***
        _agent = GetComponent<NavMeshAgent>();
        _aiController = GetComponent<BaseEnemyAI>();
        _enemyMovement = GetComponent<BaseEnemyMovement>();
        _enemyHealth = GetComponent<EnemyHealth>();
        // *Note: ใน Production Game, playerTarget ควรถูกกำหนดค่าใน Start/Setup*
        // สำหรับตอนนี้ สันนิษฐานว่า aiController.playerTarget ถูกกำหนดไว้แล้ว
        if (_aiController == null)
        {
            Debug.LogError($"{GetType().Name} requires BaseEnemyAI on the same GameObject.");
            enabled = false;
        }

    }

    protected virtual void OnEnable()
    {
        if (_aiController != null)
        {
            //_playerTarget = _aiController.playerTarget; // ดึง Target จาก AI (เพื่อความง่าย)
            //_aiController.OnStartAttackSequence += HandleStartAttackSequence;
        }
        if (_enemyHealth != null) _enemyHealth.OnDie += HandleOnDie;
    }

    protected virtual void OnDisable()
    {
        if (_aiController != null)
        {
            //_aiController.OnStartAttackSequence -= HandleStartAttackSequence;
        }
        if (_enemyHealth != null) _enemyHealth.OnDie -= HandleOnDie;
    }

    private void HandleOnDie()
    {
        if (_attackSequenceCoroutine != null) StopCoroutine(_attackSequenceCoroutine);
    }
    protected virtual void Update()
    {
        if (_enemyHealth != null && _enemyHealth.isDead) return;

        if (attackTimer > 0 && _attackSequenceCoroutine == null)
        {
            attackTimer -= Time.deltaTime;

            //if (_agent.isActiveAndEnabled)
            //{
            //    if (attackTimer <= 0)
            //    {
            //        _agent.isStopped = false;
            //    }
            //    else
            //    {
            //        _agent.isStopped = true;
            //    }

            //}
        }

        // ---  State Logi ---
        switch (_aiController.currentState)
        {
            case BaseEnemyAI.EnemyState.Attack:


                if (attackTimer <= 0 && _attackSequenceCoroutine == null && _agent.isActiveAndEnabled)
                {
                    StartAttackSequence();
                    attackTimer = attackCooldown;
                }

                if (_attackSequenceCoroutine == null) _aiController.TriggerChangeState(BaseEnemyAI.EnemyState.Chase);

                break;

        }

    }

    // --- Event Handler (Subscriber) ---

    // ถูกเรียกเมื่อ OnStartAttackSequence ถูก Invoke
    public virtual void StartAttackSequence()
    {
        _enemyMovement.canKnockback = false;
        // Base Combat ไม่ใช้ forceUseSkill3 แต่คลาสลูกสามารถนำไปใช้ได้
        if (_attackSequenceCoroutine != null) StopCoroutine(_attackSequenceCoroutine);
        _attackSequenceCoroutine = StartCoroutine(CoroutineAttackSequence());
    }

    // --- Combat Logic ---

    protected virtual IEnumerator CoroutineAttackSequence()
    {
        //if (_agent != null) _agent.isStopped = true;

        yield return AttackLogic();

        // แจ้ง AI ว่าโจมตีเสร็จแล้ว
        AttackFinished();
    }

    protected virtual IEnumerator AttackLogic()
    {
        // 1. ป้องกัน Error ในกรณีที่ลืมใส่สกิลใน Inspector
        if (enemySkills == null || enemySkills.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name} ไม่มีสกิลใน enemySkills!");
            yield break; // จบการทำงานทันที
        }

        // 2. เริ่มใช้สกิลตาม Index ปัจจุบัน
        if (enemySkills[_currentSkillIndex] != null)
        {
            yield return UseSkill(_currentSkillIndex);
        }

        // 3. เลื่อนคิว Index ไปสกิลถัดไป สำหรับการโจมตีรอบหน้า
        // เครื่องหมาย % (หารเอาเศษ) จะช่วยให้มันวนกลับไปที่ 0 อัตโนมัติเมื่อครบจำนวน Array
        // เช่น สมมติมี 3 สกิล: (2 + 1) % 3 = 0 (วนกลับจุดเริ่มต้นพอดี)
        _currentSkillIndex = (_currentSkillIndex + 1) % enemySkills.Length;
    }

    // ฟังก์ชันนี้มาแทนที่ UseSkill แบบเก่า
    protected virtual IEnumerator UseSkill(int index, float speedMultiplier = 1.0f)
    {
        if (index >= enemySkills.Length) yield break;

        currentActiveSkill = enemySkills[index];
        isSkillAnimating = true;
        currentDiractionSkill = (_aiController.playerTarget.position - transform.position).normalized;
        currentSpeedMultiplier = speedMultiplier;

        // 1. สั่ง Animator ให้เล่นท่า (ผ่าน Event เดิมที่คุณมี)
        // ส่ง index ไปให้ BaerAnimatorController รู้ว่าจะเล่นท่าไหน
        OnSkillUesd?.Invoke(index, speedMultiplier);

        // 2. *** รอจนกว่า Animation จะเล่นจบ ***
        // แทนที่จะ WaitForSeconds เราจะรอตัวแปร isSkillAnimating เป็น false
        // *** เพิ่มส่วนนี้: ตัวจับเวลากันค้าง ***
        float safetyTimer = 0f;
        float maxWaitTime = 5.0f; // สมมติว่าไม่มีท่าไหนยาวเกิน 5 วิ

        while (isSkillAnimating)
        {
            safetyTimer += Time.deltaTime;

            // ถ้าผ่านไป 5 วิแล้วยังไม่จบ แสดงว่าบั๊กแล้ว ให้สั่งจบเลย
            if (safetyTimer > maxWaitTime)
            {
                Debug.LogWarning($"Animation Skill {index} ใช้เวลานานผิดปกติ! สั่ง Force Stop");
                //isSkillAnimating = false;
            }

            yield return null;
        }

        // เคลียร์ค่าเมื่อจบ
        currentActiveSkill = null;
        currentDiractionSkill = Vector3.forward;
        currentSpeedMultiplier = 1.0f;

    }

    protected void AttackFinished()
    {
        _enemyMovement.canKnockback = true;

        OnAttackFinished?.Invoke();

        _attackSequenceCoroutine = null;
    }
    //protected void TriggerSkillUesd(int index, float speedMultiplier = 1f)
    //{
    //    OnSkillUesd?.Invoke(index, speedMultiplier);
    //}

    // ใส่ฟังก์ชันนี้ใน Animation Event: Int (0, 1, 2...)
    public void ExecuteSkillAction(int actionIndex)
    {
        if (currentActiveSkill == null) return;
        if (actionIndex >= currentActiveSkill.actions.Count) return;

        // ดึง Action ออกมาจาก List ตาม Index ที่ Animation ส่งมา
        var action = currentActiveSkill.actions[actionIndex];

        // สั่งรัน Action! (โดยส่ง User, Target, Direction เข้าไป)
        // สมมติว่า Target คือ _aiController.playerTarget
        Transform target = _aiController != null ? _aiController.playerTarget : null;

        // คำนวณทิศทาง (ใช้ทิศที่ตัวละครหันหน้าอยู่)
        Vector3 dir = currentDiractionSkill;
        float speed = currentSpeedMultiplier;
        LayerMask mask = attackMask;

        if (target != null)
        {
            action.Execute(this.gameObject, target.gameObject, dir, speed , mask);
        }

        OnSkillActionExecuted?.Invoke(currentDiractionSkill);
    }

    // ใส่ฟังก์ชันนี้ใน Animation Event: ที่เฟรมสุดท้ายของ Animation
    public void FinishSkillAnimation()
    {
        isSkillAnimating = false; // ปลดล็อค Loop ให้ทำงานต่อ
    }

    //protected void SkillEnd()
    //{
    //    OnSkillEnd?.Invoke();
    //}

    //protected void FaceTarget(Vector3 targetPosition)
    //{
    //    Vector3 direction = (targetPosition - transform.position).normalized;
    //    if (direction != Vector3.zero)
    //    {
    //        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
    //        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    //    }
    //}
}