using UnityEngine;
using UnityEngine.AI;

public class SkyChickenAnimator : MonoBehaviour
{
    [Header("References")]
    public Animator _animator;
    public Transform flipXRoot; // ลากตัวลูกที่เป็นรูปไก่มาใส่

    private NavMeshAgent _agent;
    private EnemyHealth _enemyHealth;
    private BaseEnemyCombat _enemyCombat;
    private LatchController _latchController; // ดึงมาฟังสัญญาณตอนเกาะหัว

    private bool _isSkilling = false;
    private bool _isLatched = false;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _enemyHealth = GetComponent<EnemyHealth>();
        _enemyCombat = GetComponent<BaseEnemyCombat>();
        _latchController = GetComponent<LatchController>();

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

        // *** สมัครรับฟัง Event จาก LatchController ***
        if (_latchController != null)
        {
            _latchController.OnLatchStateChanged += HandleLatchStateChanged;
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

        if (_latchController != null)
        {
            _latchController.OnLatchStateChanged -= HandleLatchStateChanged;
        }
    }

    // --- จัดการสถานะการเกาะหัว ---
    private void HandleLatchStateChanged(bool isLatched)
    {
        _isLatched = isLatched;

        if (_animator != null)
        {
            _animator.SetBool("isLatch", isLatched);
        }

        // ตอนเกาะหัว ไก่จะหันหน้าทางเดียวกับผู้เล่น (แต่จริงๆ เราล็อคด้วย SetParent ไปแล้ว)
        // เพื่อความชัวร์ เราบังคับให้สปีดกลับมาปกติเผื่อมันค้างตอนพุ่ง
        if (isLatched)
        {
            _isSkilling = false;
            _animator.speed = 1f;
        }
    }

    // --- จัดการความตาย ---
    private void HandleDeath()
    {
        if (_animator != null) _animator.SetBool("isDead", true);
        this.enabled = false;
    }

    // --- จัดการสกิลพุ่งชน ---
    private void HandleSkillUsed(int skillNumber, float speedMultiplier)
    {
        if (_isLatched) return; // ถ้าเกาะหัวอยู่ ห้ามร่ายสกิลซ้อน

        _isSkilling = true;
        _animator.speed = speedMultiplier;

        FlipTowardsDirection(_enemyCombat.currentDiractionSkill);
        _animator.SetTrigger("atSkill1");
    }

    private void HandleSkillActionExecuted(Vector3 actionDirection)
    {
        if (_isLatched) return;
        FlipTowardsDirection(actionDirection);
    }

    private void HandleAttackFinished()
    {
        _isSkilling = false;
        _animator.speed = 1f;
    }

    // --- การเดินและการหันหน้า ---
    private void Update()
    {
        if (_enemyHealth != null && _enemyHealth.isDead) return;
        if (_animator == null || _agent == null) return;

        UpdateAnimationState();

        // หันหน้าตอนเดินได้ ก็ต่อเมื่อไม่ได้ร่ายสกิล และไม่ได้เกาะหัวใครอยู่
        if (!_isSkilling && !_isLatched)
        {
            UpdateSpriteFlipByVelocity();
        }
    }

    private void UpdateAnimationState()
    {
        // เช็คเผื่อ Agent โดนปิด (เช่นตอนกำลังเกาะหัว) จะได้ไม่บั๊ก Error
        if (!_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
        {
            _animator.SetBool("isMove", false);
            return;
        }

        bool isMoving = _agent.velocity.magnitude > 0.1f && !_agent.isStopped;
        _animator.SetBool("isMove", isMoving);
    }

    private void UpdateSpriteFlipByVelocity()
    {
        if (_agent.isActiveAndEnabled && _agent.velocity.magnitude > 0.1f)
        {
            FlipTowardsDirection(_agent.velocity);
        }
    }

    // --- ฟังก์ชันพลิกหน้า (FlipX ด้วย Scale) ---
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