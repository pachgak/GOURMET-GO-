using UnityEngine;
using UnityEngine.AI;

public class HogAnimator : MonoBehaviour
{
    [Header("References")]
    public Animator _animator;
    public Transform flipXRoot; // ลากออบเจกต์ภาพหมูป่ามาใส่ช่องนี้

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
        _enemyAI = GetComponent<BaseEnemyAI>(); // ดึง AI มาเพื่อฟังสตัน

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

        // *** สมัครรับ Event สตัน ***
        if (_enemyAI != null)
        {
            _enemyAI.OnStunStateChanged += HandleStunStateChanged;
        }
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

        if (_enemyAI != null)
        {
            _enemyAI.OnStunStateChanged -= HandleStunStateChanged;
        }
    }

    // --- จัดการเรื่องสตัน ---
    private void HandleStunStateChanged(bool isStunned)
    {
        if (_animator != null)
        {
            _animator.SetBool("isStun", isStunned);
        }

        // ถ้ายกเลิกการสตัน หรือติดสตัน ให้คืนค่าความเร็ว Animation กลับเป็น 1 เสมอ
        // (เผื่อติดสตันตอนกำลังร่ายสกิลที่ Speed แตกต่างจากปกติ)
        if (isStunned)
        {
            _isSkilling = false;
            _animator.speed = 1f;
        }
    }

    // --- จัดการเรื่องความตาย ---
    private void HandleDeath()
    {
        if (_animator != null) _animator.SetBool("isDead", true);
        this.enabled = false;
    }

    // --- จัดการเรื่องสกิล ---
    private void HandleSkillUsed(int skillNumber, float speedMultiplier)
    {
        if (_enemyAI != null && _enemyAI.IsStunned) return; // ถ้ามึนอยู่ ห้ามร่ายสกิล

        _isSkilling = true;
        _animator.speed = speedMultiplier;

        FlipTowardsDirection(_enemyCombat.currentDiractionSkill);
        _animator.SetTrigger("atSkill1");
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

    // --- การเดินและ Update ---
    private void Update()
    {
        if (_enemyHealth != null && _enemyHealth.isDead) return;
        if (_animator == null || _agent == null) return;

        UpdateAnimationState();

        // จะหันหน้าตามการเดินได้ ก็ต่อเมื่อ "ไม่ได้ร่ายสกิล" และ "ไม่ได้ติดสตัน" เท่านั้น
        bool isStunned = _enemyAI != null && _enemyAI.IsStunned;
        if (!_isSkilling && !isStunned)
        {
            UpdateSpriteFlipByVelocity();
        }
    }

    private void UpdateAnimationState()
    {
        bool isMoving = _agent.velocity.magnitude > 0.1f && !_agent.isStopped;
        _animator.SetBool("isMove", isMoving);
    }

    private void UpdateSpriteFlipByVelocity()
    {
        if (_agent.velocity.magnitude > 0.1f)
        {
            FlipTowardsDirection(_agent.velocity);
        }
    }

    // --- ฟังก์ชันพลิกหน้า (FlipX แกน Scale) ---
    private void FlipTowardsDirection(Vector3 dir)
    {
        if (flipXRoot == null) return;

        Vector3 currentScale = flipXRoot.localScale;

        if (dir.x < -0.01f)
        {
            currentScale.x = Mathf.Abs(currentScale.x);
        }
        else if (dir.x > 0.01f)
        {
            currentScale.x = -Mathf.Abs(currentScale.x);
        }

        flipXRoot.localScale = currentScale;
    }
}