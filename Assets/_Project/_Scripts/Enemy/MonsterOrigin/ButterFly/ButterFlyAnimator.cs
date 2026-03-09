using UnityEngine;
using UnityEngine.AI;

public class ButterFlyAnimator : MonoBehaviour
{
    [Header("References")]
    public Animator _animator;
    public Transform flipXRoot; // ลากออบเจกต์ภาพผีเสื้อ (ตัวลูก) มาใส่ช่องนี้

    private NavMeshAgent _agent;
    private EnemyHealth _enemyHealth;

    private void Awake()
    {
        // ดึง Component ที่จำเป็น
        _agent = GetComponent<NavMeshAgent>();
        _enemyHealth = GetComponent<EnemyHealth>();

        // ถ้าลืมใส่ Animator ใน Inspector ให้มันพยายามหาจากตัวลูกให้
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
        if (_animator != null)
        {
            _animator.SetBool("isDead", true);
        }

        // ปิดสคริปต์นี้ทิ้งไปเลยตอนตาย จะได้ไม่พยายามอัปเดตแอนิเมชันเดินอีก
        this.enabled = false;
    }

    private void Update()
    {
        // ถ้าตายแล้ว หรือไม่มีคอมโพเนนต์ให้หยุดทำงาน
        if (_enemyHealth != null && _enemyHealth.isDead) return;
        if (_animator == null || _agent == null) return;

        UpdateAnimationState();
        UpdateSpriteFlip();
    }

    private void UpdateAnimationState()
    {
        // เช็คว่ากำลังเคลื่อนที่อยู่หรือไม่ (มีความเร็ว > 0.1 และไม่ได้ถูกสั่งเบรก)
        bool isMoving = _agent.velocity.magnitude > 0.1f && !_agent.isStopped;

        // ส่งค่าไปบอก Animator
        _animator.SetBool("isMove", isMoving);
    }

    private void UpdateSpriteFlip()
    {
        if (flipXRoot == null) return;

        // ถ้าผีเสื้อขยับ (มีความเร็ว)
        if (_agent.velocity.magnitude > 0.1f)
        {
            Vector3 currentScale = flipXRoot.localScale;

            // ถ้าบินไปทางซ้าย (แกน X ติดลบ)
            if (_agent.velocity.x < -0.01f)
            {
                // บังคับสเกลแกน X ให้ติดลบ
                currentScale.x = -Mathf.Abs(currentScale.x);
            }
            // ถ้าบินไปทางขวา (แกน X เป็นบวก)
            else if (_agent.velocity.x > 0.01f)
            {
                // บังคับสเกลแกน X ให้เป็นบวก
                currentScale.x = Mathf.Abs(currentScale.x);
            }

            flipXRoot.localScale = currentScale;
        }
    }
}