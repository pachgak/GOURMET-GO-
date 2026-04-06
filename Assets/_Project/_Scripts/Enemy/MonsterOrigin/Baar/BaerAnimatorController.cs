using System;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class BaerAnimatorController : MonoBehaviour
{
    public Animator _animator;

    [Tooltip("ลากออบเจกต์ภาพ Bear (ตัวที่มี Sprite) มาใส่ช่องนี้เพื่อใช้ Scale พลิกซ้ายขวา")]
    public Transform flipXRoot;

    private NavMeshAgent _agent;
    private BaseEnemyCombat _enemyCombat;
    private BaseEnemyAI _aiController;
    private EnemyHealth _enemyHealth;

    private void Awake()
    {
        // ...
        _aiController = GetComponent<BaseEnemyAI>();
        _enemyCombat = GetComponent<BaseEnemyCombat>();
        _agent = GetComponent<NavMeshAgent>();
        _enemyHealth = GetComponent<EnemyHealth>();
        // ...
    }

    private void OnEnable()
    {
        _enemyCombat.OnSkillUesd += HandleSkillUesd;
        _enemyCombat.OnAttackFinished += HandleSkillEnd;
        _enemyCombat.OnSkillActionExecuted += HandleSkillActionExecuted;
        _enemyHealth.OnDie += HandleOnDie;
    }

    private void OnDisable()
    {
        _enemyCombat.OnSkillUesd -= HandleSkillUesd;
        _enemyCombat.OnAttackFinished -= HandleSkillEnd;
        _enemyCombat.OnSkillActionExecuted -= HandleSkillActionExecuted;
        _enemyHealth.OnDie -= HandleOnDie;
    }

    private void HandleOnDie()
    {
        _animator.speed = 1f;
        _animator.SetBool("isDead", true);
    }

    private void HandleSkillUesd(int skillNumber, float speedMultiplier)
    {
        _animator.speed = speedMultiplier;

        UpdateSpriteFlipAndAnimation(_enemyCombat.currentDiractionSkill);

        if (skillNumber == 0)
        {
            _animator.SetTrigger("atSkill1");
        }
        else if (skillNumber == 1)
        {
            _animator.SetTrigger("atSkill2");
        }
        else if (skillNumber == 2)
        {
            _animator.SetTrigger("atSkill3");
        }
        else if (skillNumber == 3)
        {
            _animator.SetTrigger("atSkill4");
        }
    }

    // *** ทำงานตอน Event ในคลิปแอนิเมชันทำงาน (เช่น จังหวะพุ่ง หรือ ปล่อยพลัง) ***
    private void HandleSkillActionExecuted(Vector3 actionDirection)
    {
        // อัปเดตการหันหน้าอีกครั้ง เผื่อ Action นี้เป็นประเภทหันไปทางอื่น
        UpdateSpriteFlipAndAnimation(actionDirection);
    }

    private void HandleSkillEnd()
    {
        _animator.speed = 1f;
    }

    void Start()
    {

    }

    void Update()
    {
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        // 1. ตรวจสอบความพร้อมของ Agent
        if (_agent == null || _animator == null || !_agent.enabled || _agent.isStopped)
        {
            _animator.SetBool("IsMoveing", false);
            _animator.SetFloat("MoveX", 0f);
            _animator.SetFloat("MoveZ", 0f);
            return;
        }

        // 2. ดึงความเร็วในพิกัดโลก (World Space Velocity)
        Vector3 worldVelocity = _agent.velocity;
        float totalSpeed = worldVelocity.magnitude;

        // 3. ตั้งค่า IsMoving
        bool isMoving = totalSpeed > 0.01f;
        _animator.SetBool("IsMoveing", isMoving);

        // 4. ถ้ากำลังเคลื่อนที่ ให้คำนวณทิศทาง
        if (isMoving)
        {
            // แปลงความเร็วจาก World Space ให้เป็น Local Space ของตัวละคร
            Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity.normalized);
            float moveX = localVelocity.x;
            float moveZ = localVelocity.z;

            _animator.SetFloat("MoveX", moveX);
            _animator.SetFloat("MoveZ", moveZ);

            // *** เปลี่ยนจากการตั้งค่า flipX มาเป็นการเรียกใช้ FlipTowardsDirection ***
            FlipTowardsDirection(worldVelocity);
        }
        else
        {
            // ถ้าหยุดเดิน ให้ Lerp ค่ากลับไปที่ 0 เพื่อให้ Animation กลับสู่ Idle อย่างนุ่มนวล
            _animator.SetFloat("MoveX", Mathf.Lerp(_animator.GetFloat("MoveX"), 0f, 0.1f));
            _animator.SetFloat("MoveZ", Mathf.Lerp(_animator.GetFloat("MoveZ"), 0f, 0.1f));
        }
    }

    private void UpdateSpriteFlipAndAnimation(Vector3 direction)
    {
        // ส่งค่าเข้า Blend Tree ถ้ามี
        _animator.SetFloat("ActionX", direction.x);
        _animator.SetFloat("ActionZ", direction.z);

        // *** เปลี่ยนจากการตั้งค่า flipX มาเป็นการเรียกใช้ FlipTowardsDirection ***
        FlipTowardsDirection(direction);
    }

    // --- ฟังก์ชันพลิกหน้าแบบใช้แกน Scale (เพิ่มเข้ามาใหม่) ---
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