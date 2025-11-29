// BaseEnemyCombat.cs
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.AI;
using Unity.VisualScripting;

public class BaseEnemyCombat : MonoBehaviour
{
    public float attackCooldown;
    public float attackTimer;
    // Events ที่จะถูก Invoke กลับไปหา AI เมื่อโจมตีเสร็จ
    public event Action OnAttackFinished;
    public event Action<int,float> OnSkillUesd;
    //public event Action OnSkillEnd;

    public EnemySkillSO[] enemySkills;
    //public EnemySkill[] enemySkills;

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
        _enemyHealth.OnDie += HandleOnDie;
    }

    protected virtual void OnDisable()
    {
        if (_aiController != null)
        {
            //_aiController.OnStartAttackSequence -= HandleStartAttackSequence;
        }
        _enemyHealth.OnDie -= HandleOnDie;
    }

    private void HandleOnDie()
    {
        if (_attackSequenceCoroutine != null) StopCoroutine(_attackSequenceCoroutine);
    }
    protected virtual void Update()
    {
        if (_enemyHealth.isDead) return;

        if (attackTimer > 0 && _attackSequenceCoroutine == null) attackTimer -= Time.deltaTime;


        if (_aiController.currentState == BaseEnemyAI.EnemyState.Attack && attackTimer <= 0 && _attackSequenceCoroutine == null)
        {
            HandleStartAttackSequence(false);
            attackTimer = attackCooldown;
        }
        
        if(_aiController.currentState == BaseEnemyAI.EnemyState.Attack &&_attackSequenceCoroutine == null) _aiController.TriggerChangeState(BaseEnemyAI.EnemyState.Chase);
    }

    // --- Event Handler (Subscriber) ---

    // ถูกเรียกเมื่อ OnStartAttackSequence ถูก Invoke
    public virtual void HandleStartAttackSequence(bool forceUseSkill3)
    {
        _enemyMovement.canKnockback = false;
        // Base Combat ไม่ใช้ forceUseSkill3 แต่คลาสลูกสามารถนำไปใช้ได้
        if (_attackSequenceCoroutine != null) StopCoroutine(_attackSequenceCoroutine);
        _attackSequenceCoroutine = StartCoroutine(AttackSequence(forceUseSkill3));
    }

    // --- Combat Logic ---

    protected virtual IEnumerator AttackSequence(bool forceUseSkill3)
    {
        //if (_agent != null) _agent.isStopped = true;

        yield return AttackLogic(forceUseSkill3);

        // แจ้ง AI ว่าโจมตีเสร็จแล้ว
        TriggerAttackFinished();
        _attackSequenceCoroutine = null;
    }

    protected virtual IEnumerator AttackLogic(bool forceUseSkill3)
    {
        // 1. Logic การหันหน้าไปหา Player
        TriggerSkillUesd(0);
        yield return enemySkills[0].UseSkill(this.gameObject, _aiController.playerTarget);

        // 3. Apply Damage (TODO)
    }

    protected void TriggerAttackFinished()
    {
        _enemyMovement.canKnockback = true;

        OnAttackFinished?.Invoke();
    }
    protected void TriggerSkillUesd(int index , float speedMultiplier = 1f)
    {
        OnSkillUesd?.Invoke(index, speedMultiplier);
    }

    //protected void SkillEnd()
    //{
    //    OnSkillEnd?.Invoke();
    //}

    protected void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
    }
}