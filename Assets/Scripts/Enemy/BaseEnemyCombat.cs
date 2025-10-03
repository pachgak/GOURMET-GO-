// BaseEnemyCombat.cs
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.AI;

public class BaseEnemyCombat : MonoBehaviour
{
    // Events ที่จะถูก Invoke กลับไปหา AI เมื่อโจมตีเสร็จ
    public event Action OnAttackFinished;

    protected NavMeshAgent _agent;
    protected Transform _playerTarget;
    protected Coroutine _attackSequenceCoroutine;
    protected BaseEnemyAI _aiController;

    protected virtual void Awake()
    {
        // *** จัดการตัวเอง: หา Reference ที่จำเป็นทั้งหมด ***
        _agent = GetComponent<NavMeshAgent>();
        _aiController = GetComponent<BaseEnemyAI>();

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
            _playerTarget = _aiController.playerTarget; // ดึง Target จาก AI (เพื่อความง่าย)
            _aiController.OnStartAttackSequence += HandleStartAttackSequence;
        }
    }

    protected virtual void OnDisable()
    {
        if (_aiController != null)
        {
            _aiController.OnStartAttackSequence -= HandleStartAttackSequence;
        }
    }

    // --- Event Handler (Subscriber) ---

    // ถูกเรียกเมื่อ OnStartAttackSequence ถูก Invoke
    public virtual void HandleStartAttackSequence(bool forceUseSkill3)
    {
        // Base Combat ไม่ใช้ forceUseSkill3 แต่คลาสลูกสามารถนำไปใช้ได้
        if (_attackSequenceCoroutine != null) StopCoroutine(_attackSequenceCoroutine);
        _attackSequenceCoroutine = StartCoroutine(SimpleMeleeAttack());
    }

    // --- Combat Logic ---

    protected virtual IEnumerator SimpleMeleeAttack()
    {
        Debug.Log("Base Combat: Simple Melee Attack!");
        if (_agent != null) _agent.isStopped = true;

        // 1. Logic การหันหน้าไปหา Player
        FaceTarget(_playerTarget.position);

        // 2. Play Animation และรอ
        yield return new WaitForSeconds(1.0f);

        // 3. Apply Damage (TODO)

        // แจ้ง AI ว่าโจมตีเสร็จแล้ว
        OnAttackFinished?.Invoke();
        _attackSequenceCoroutine = null;
    }

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