using UnityEngine;
using UnityEngine.AI;

public class ShamakiriCloneAnimator : MonoBehaviour
{
    [Header("References")]
    public Animator _animator;
    public Transform flipXRoot;

    [Header("Clone Setup")]
    //[Tooltip("ใส่เลขอนิเมชันของโคลนตัวนี้ (เช่น 2=Melee, 3=Range, 4=AoE)")]
    //public int cloneAnimIndex = 2; // *** พระเอกของเราอยู่ตรงนี้ครับ! ***
    [Tooltip("ใส่ชื่ออนิเมชันของโคลนตัวนี้ (เช่น atSkill2=Melee, atSkill3=Range, atSkill4=AoE)")]
    public string cloneSkillAnimName = "atSkill2"; // *** พระเอกของเราอยู่ตรงนี้ครับ! ***

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
        if (_enemyAI != null && _enemyAI.IsStunned) return;

        _isSkilling = true;
        _animator.speed = speedMultiplier;

        FlipTowardsDirection(_enemyCombat.currentDiractionSkill);

        // โคลนทุกตัวจะใช้สกิล Index 0 เสมอ
        if (skillIndex == 0)
        {
            // ประกอบร่างข้อความ "atSkill" + "เลขที่ตั้งไว้ใน Inspector"
            // ถ้าตั้งไว้ 2 มันก็จะส่งคำสั่ง _animator.SetTrigger("atSkill2"); อัตโนมัติ!
            _animator.SetTrigger(cloneSkillAnimName);
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