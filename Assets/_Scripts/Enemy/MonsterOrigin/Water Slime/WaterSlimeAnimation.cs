using UnityEngine;
using UnityEngine.AI;

public class WaterSlimeAnimator : MonoBehaviour
{
    [Header("References")]
    public Animator _animator;
    public Transform flipXRoot; // ลากออบเจกต์ภาพสไลม์มาใส่ช่องนี้

    private NavMeshAgent _agent;
    private EnemyHealth _enemyHealth;
    private BaseEnemyCombat _enemyCombat;

    private bool _isSkilling = false;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _enemyHealth = GetComponent<EnemyHealth>();
        _enemyCombat = GetComponent<BaseEnemyCombat>(); // ดึง Combat มาเพื่อเชื่อมสกิล

        if (_animator == null) _animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (_enemyHealth != null) _enemyHealth.OnDie += HandleDeath;

        // สมัครรับ Event การใช้สกิลจาก Combat
        if (_enemyCombat != null)
        {
            _enemyCombat.OnSkillUesd += HandleSkillUsed;
            _enemyCombat.OnSkillActionExecuted += HandleSkillActionExecuted;
            _enemyCombat.OnAttackFinished += HandleAttackFinished;
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
    }

    // --- 1. จัดการเรื่องความตาย ---
    private void HandleDeath()
    {
        if (_animator != null) _animator.SetBool("isDead", true);
        this.enabled = false;
    }

    // --- 2. จัดการเรื่องสกิล (ดึง Logic มาจาก BearAnimator) ---
    private void HandleSkillUsed(int skillNumber, float speedMultiplier)
    {
        _isSkilling = true;
        _animator.speed = speedMultiplier; // ปรับความเร็วแอนิเมชันตาม Combat

        // หันหน้าไปหาเป้าหมายตอนเริ่มง้างสกิล
        FlipTowardsDirection(_enemyCombat.currentDiractionSkill);

        // สไลม์มีสกิลเดียว สั่ง Trigger สกิล 1 ไปเลย
        _animator.SetTrigger("atSkill1");
    }

    private void HandleSkillActionExecuted(Vector3 actionDirection)
    {
        // อัปเดตการหันหน้าอีกรอบ เผื่อแอคชั่นสั่งให้หันไปทางอื่น
        FlipTowardsDirection(actionDirection);
    }

    private void HandleAttackFinished()
    {
        _isSkilling = false;
        _animator.speed = 1f; // คืนความเร็วกลับเป็นปกติ
    }

    // --- 3. การเดินและ Update ---
    private void Update()
    {
        if (_enemyHealth != null && _enemyHealth.isDead) return;
        if (_animator == null || _agent == null) return;

        UpdateAnimationState();

        // ถ้า "ไม่ได้" ใช้สกิลอยู่ ถึงจะยอมให้หันหน้าตามทิศการเดิน
        if (!_isSkilling)
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

    // --- 4. ฟังก์ชันตัวช่วยสำหรับพลิกหน้า (ใช้ร่วมกันทั้งตอนเดินและตอนร่ายสกิล) ---
    private void FlipTowardsDirection(Vector3 dir)
    {
        if (flipXRoot == null) return;

        Vector3 currentScale = flipXRoot.localScale;

        if (dir.x < -0.01f)
        {
            currentScale.x = Mathf.Abs(currentScale.x); // บังคับหันซ้าย
        }
        else if (dir.x > 0.01f)
        {
            currentScale.x = -Mathf.Abs(currentScale.x);  // บังคับหันขวา
        }

        flipXRoot.localScale = currentScale;
    }
}