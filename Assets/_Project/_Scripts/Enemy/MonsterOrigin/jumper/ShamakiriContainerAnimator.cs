using UnityEngine;
using UnityEngine.AI;

public class ShamakiriContainerAnimator : MonoBehaviour
{
    [Header("References")]
    public Animator _animator;
    [Tooltip("ลากออบเจกต์ภาพ Shamakiri มาใส่ช่องนี้ (ใช้สเกลพลิกซ้ายขวา)")]
    public Transform flipXRoot;

    private NavMeshAgent _agent;
    private EnemyHealth _enemyHealth;
    private BaseEnemyCombat _enemyCombat;
    private BaseEnemyAI _enemyAI;

    private bool _isSkilling = false;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _enemyHealth = GetComponent<EnemyHealth>();
        _enemyCombat = GetComponent<BaseEnemyCombat>();
        _enemyAI = GetComponent<BaseEnemyAI>();

        if (_animator == null) _animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (_enemyHealth != null) _enemyHealth.OnDie += HandleDeath;

        if (_enemyCombat != null)
        {
            _enemyCombat.OnSkillUesd += HandleSkillUsed;
            _enemyCombat.OnSkillActionExecuted += HandleSkillActionExecuted;
            _enemyCombat.OnAttackFinished += HandleAttackFinished;
        }

        if (_enemyAI != null) _enemyAI.OnStunStateChanged += HandleStunStateChanged;
    }

    private void OnDisable()
    {
        if (_enemyHealth != null) _enemyHealth.OnDie -= HandleDeath;

        if (_enemyCombat != null)
        {
            _enemyCombat.OnSkillUesd -= HandleSkillUsed;
            _enemyCombat.OnSkillActionExecuted -= HandleSkillActionExecuted;
            _enemyCombat.OnAttackFinished -= HandleAttackFinished;
        }

        if (_enemyAI != null) _enemyAI.OnStunStateChanged -= HandleStunStateChanged;
    }

    private void HandleStunStateChanged(bool isStunned)
    {
        if (_animator != null) _animator.SetBool("isStun", isStunned);

        if (isStunned)
        {
            _isSkilling = false;
            _animator.speed = 1f;
        }
    }

    private void HandleDeath()
    {
        if (_animator != null) _animator.SetBool("isDead", true);
        this.enabled = false;
    }

    private void HandleSkillUsed(int skillIndex, float speedMultiplier)
    {
        Debug.Log($"HandleSkillUsed Trick {skillIndex}");
        if (_enemyAI != null && _enemyAI.IsStunned) return;

        _isSkilling = true;
        _animator.speed = speedMultiplier;

        FlipTowardsDirection(_enemyCombat.currentDiractionSkill);

        // แมปปิ้ง Index สกิลของร่างแม่เข้ากับ Trigger
        switch (skillIndex)
        {
            case 0: _animator.SetTrigger("atSkill1"); break; // เผื่อตอนแยกร่างมีอนิเมชัน
            case 1: _animator.SetTrigger("atSkill2"); break; // Melee
            case 2: _animator.SetTrigger("atSkill3"); break; // Range
            case 3: _animator.SetTrigger("atSkill4"); break; // AoE
        }
    }

    private void HandleSkillActionExecuted(Vector3 actionDirection)
    {
        if (_enemyAI != null && _enemyAI.IsStunned) return;
        FlipTowardsDirection(actionDirection);
    }

    private void HandleAttackFinished()
    {
        _isSkilling = false;
        _animator.speed = 1f;
    }

    private void Update()
    {
        if (_enemyHealth != null && _enemyHealth.isDead) return;
        if (_animator == null || _agent == null) return;

        bool isMoving = _agent.velocity.magnitude > 0.1f && !_agent.isStopped;
        _animator.SetBool("isMove", isMoving);

        bool isStunned = _enemyAI != null && _enemyAI.IsStunned;
        if (!_isSkilling && !isStunned && isMoving)
        {
            FlipTowardsDirection(_agent.velocity);
        }
    }

    private void FlipTowardsDirection(Vector3 dir)
    {
        if (flipXRoot == null) return;

        Vector3 currentScale = flipXRoot.localScale;
        if (dir.x < -0.01f) currentScale.x = Mathf.Abs(currentScale.x);
        else if (dir.x > 0.01f) currentScale.x = -Mathf.Abs(currentScale.x);
        flipXRoot.localScale = currentScale;
    }
}