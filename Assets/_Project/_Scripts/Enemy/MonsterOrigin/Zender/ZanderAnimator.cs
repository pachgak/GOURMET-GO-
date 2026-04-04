using UnityEngine;
using UnityEngine.AI;

public class ZanderAnimator : MonoBehaviour
{
    [Header("References")]
    public Animator _animator;
    [Tooltip("ลากออบเจกต์ภาพ Zander มาใส่ช่องนี้ (ใช้สเกลพลิกซ้ายขวา)")]
    public Transform flipXRoot;

    [Header("Sprite Overcharge Settings")]
    public SpriteRenderer spriteRenderer; // ลาก SpriteRenderer ตัวที่จะเปลี่ยนภาพมาใส่
    public Sprite normalSprite;           // ภาพตอนปกติ
    public Sprite overchargeSprite;       // ภาพตอนเข้าโหมด Overcharge

    private NavMeshAgent _agent;
    private EnemyHealth _enemyHealth;
    private BaseEnemyCombat _enemyCombat;
    private BaseEnemyAI _enemyAI;

    // ดึง ZanderCombat มาเพื่อเช็คสถานะ isOvercharge โดยเฉพาะ
    private ZanderCombat _zanderCombat;

    private bool _isSkilling = false;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _enemyHealth = GetComponent<EnemyHealth>();
        _enemyCombat = GetComponent<BaseEnemyCombat>();
        _enemyAI = GetComponent<BaseEnemyAI>();
        _zanderCombat = GetComponent<ZanderCombat>();

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

        if (_enemyAI != null)
        {
            _enemyAI.OnStunStateChanged += HandleStunStateChanged;
        }

        // *** 1. สมัครรับ Event การเปลี่ยน Overcharge ***
        if (_zanderCombat != null)
        {
            _zanderCombat.OnOverchargeChanged += HandleOverchargeChanged;
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

        // *** 2. ยกเลิกการรับ Event ***
        if (_zanderCombat != null)
        {
            _zanderCombat.OnOverchargeChanged -= HandleOverchargeChanged;
        }
    }

    // *** 3. ฟังก์ชันใหม่สำหรับจัดการเรื่องสลับภาพโดยเฉพาะ ***
    private void HandleOverchargeChanged(bool isOvercharged)
    {
        if (spriteRenderer != null)
        {
            // สลับภาพทันที (ทำแค่ 1 ครั้งตอนสถานะเปลี่ยน ไม่ต้องทำทุกเฟรมแล้ว!)
            spriteRenderer.sprite = isOvercharged ? overchargeSprite : normalSprite;
        }

        if (_animator != null)
        {
            _animator.SetBool("isOvercharge", isOvercharged);
        }
    }

    // ==========================================
    // 1. จัดการสถานะพิเศษ (สตัน / ตาย)
    // ==========================================
    private void HandleStunStateChanged(bool isStunned)
    {
        if (_animator != null)
        {
            _animator.SetBool("isStun", isStunned);
        }

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

    // ==========================================
    // 2. จัดการเรื่องสกิลและการโจมตี
    // ==========================================
    private void HandleSkillUsed(int skillIndex, float speedMultiplier)
    {
        if (_enemyAI != null && _enemyAI.IsStunned) return;

        _isSkilling = true;
        _animator.speed = speedMultiplier;

        FlipTowardsDirection(_enemyCombat.currentDiractionSkill);

        switch (skillIndex)
        {
            case 0: _animator.SetTrigger("atSkill1"); break;
            case 1: _animator.SetTrigger("atSkill2"); break;
            case 2: _animator.SetTrigger("atSkill3"); break;
            case 3: _animator.SetTrigger("atSkill4"); break;
            case 4: _animator.SetTrigger("atSkill5"); break;
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

    // ==========================================
    // 3. การเดิน, พลิกตัว และ สลับภาพ (Update)
    // ==========================================
    private void Update()
    {
        if (_enemyHealth != null && _enemyHealth.isDead) return;
        if (_animator == null || _agent == null) return;

        // *** ลบโค้ดสลับภาพออกจาก Update() ตรงนี้ทิ้งไปได้เลย โค้ดโล่งขึ้นเยอะ! ***

        bool isMoving = _agent.velocity.magnitude > 0.1f && !_agent.isStopped;
        _animator.SetBool("isMove", isMoving);

        bool isStunned = _enemyAI != null && _enemyAI.IsStunned;
        if (!_isSkilling && !isStunned && isMoving)
        {
            FlipTowardsDirection(_agent.velocity);
        }
    }

    // --- ฟังก์ชันพลิกหน้าแบบใช้แกน Scale (เหมือน HogAnimator) ---
    private void FlipTowardsDirection(Vector3 dir)
    {
        if (flipXRoot == null) return;

        Vector3 currentScale = flipXRoot.localScale;

        if (dir.x < -0.01f)
        {
            currentScale.x = Mathf.Abs(currentScale.x); // หันซ้าย
        }
        else if (dir.x > 0.01f)
        {
            currentScale.x = -Mathf.Abs(currentScale.x); // หันขวา
        }

        flipXRoot.localScale = currentScale;
    }
}