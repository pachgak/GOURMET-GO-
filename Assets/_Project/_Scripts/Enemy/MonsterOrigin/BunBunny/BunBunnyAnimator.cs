using UnityEngine;
using UnityEngine.AI;

public class BunBunnyAnimator : MonoBehaviour
{
    [Header("References")]
    public Animator _animator;
    public Transform flipXRoot; // ตัวแม่ของกราฟิกที่จะโดนสั่งสลับด้าน (-1, 1)

    private NavMeshAgent _agent;
    private BunBunnyAI _aiController;
    private EnemyHealth _enemyHealth;

    private void Awake()
    {
        // ดึงคอมโพเนนต์ที่จำเป็น
        _agent = GetComponent<NavMeshAgent>();
        _aiController = GetComponent<BunBunnyAI>();
        _enemyHealth = GetComponent<EnemyHealth>();

        if (_animator == null) _animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (_enemyHealth != null)
        {
            _enemyHealth.OnDie += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (_enemyHealth != null)
        {
            _enemyHealth.OnDie -= HandleDeath;
        }
    }

    private void HandleDeath()
    {
        // สั่งตาย
        _animator.SetBool("isDead", true);

        // ปิดการทำงานของ Animator ส่วนอื่นเพื่อไม่ให้มันกลับมาเดินตอนตาย
        this.enabled = false;
    }

    private void Update()
    {
        if (_enemyHealth != null && _enemyHealth.isDead) return;
        if (_animator == null || _agent == null) return;

        UpdateAnimationState();
        UpdateSpriteFlip();
    }

    private void UpdateAnimationState()
    {
        // 1. เช็คว่ากำลังเคลื่อนที่อยู่หรือไม่ (เช็คจากความเร็วของ Agent)
        bool isMoving = _agent.velocity.magnitude > 0.1f && !_agent.isStopped;
        _animator.SetBool("isMove", isMoving);

        // 2. เช็คว่ากำลังวิ่งหนี (Chase) อยู่หรือไม่
        bool isRunning = false;
        if (_aiController != null)
        {
            isRunning = (_aiController.currentState == BaseEnemyAI.EnemyState.Chase);
        }

        // ถ้าหยุดเดินแล้ว ก็ไม่ควรเล่นท่าวิ่ง (เผื่อติดบั๊กตอนชนกำแพง)
        _animator.SetBool("isRun", isRunning && isMoving);
    }

    private void UpdateSpriteFlip()
    {
        // ถ้ากระต่ายขยับ (เช็คความเร็วรวม)
        if (_agent.velocity.magnitude > 0.1f)
        {
            // ดึงค่าสเกลปัจจุบันมาเก็บไว้ก่อน
            Vector3 currentScale = flipXRoot.localScale;

            // ถ้าเดินไปทางซ้าย (ติดลบ)
            if (_agent.velocity.x < -0.01f)
            {
                // บังคับให้แกน X ติดลบ (หันซ้าย)
                currentScale.x = Mathf.Abs(currentScale.x);
            }
            // ถ้าเดินไปทางขวา (เป็นบวก)
            else if (_agent.velocity.x > 0.01f)
            {
                // บังคับให้แกน X เป็นบวก (หันขวา)
                currentScale.x = -Mathf.Abs(currentScale.x);
            }

            // นำค่าสเกลที่คำนวณเสร็จแล้ว ใส่กลับคืน
            flipXRoot.localScale = currentScale;
        }
    }
}