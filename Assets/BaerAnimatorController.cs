using System;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class BaerAnimatorController : MonoBehaviour
{
    public Animator _animator;
    public SpriteRenderer spriteRenderer;
    private NavMeshAgent _agent;
    private BaseEnemyCombat _enemyCombat;
    private BaseEnemyAI _aiController;

    private bool isSkilling;
    private void Awake()
    {
        // ...
        _aiController = GetComponent<BaseEnemyAI>();
        _enemyCombat = GetComponent<BaseEnemyCombat>();
        _agent = GetComponent<NavMeshAgent>();
        // ...
    }

    private void OnEnable()
    {
        _enemyCombat.OnSkillUesd += HandleSkillUesd;
        _enemyCombat.OnSkillEnd += HandleSkillEnd;
    }

    private void OnDisable()
    {
        _enemyCombat.OnSkillUesd -= HandleSkillUesd;
        _enemyCombat.OnSkillEnd -= HandleSkillEnd;
    }
    private void HandleSkillUesd(int skillNumber)
    {
        isSkilling = true;

        Vector3 attackDirection = (_aiController.playerTarget.position - transform.position).normalized;
        _animator.SetFloat("ActionX", attackDirection.x);
        _animator.SetFloat("ActionZ", attackDirection.z);

        //spriteRenderer.flipX = (attackDirection.x <= 0) ? true : false;

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
    }

    private void HandleSkillEnd()
    {
        isSkilling = false  ;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        // 1. ตรวจสอบความพร้อมของ Agent
        // ใช้ _agent.enabled และ _agent.isStopped เพื่อให้แน่ใจว่า Agent กำลังทำงานและไม่ได้ถูกสั่งให้หยุด
        if (_agent == null || _animator == null || !_agent.enabled || _agent.isStopped)
        {
            // ถ้า Agent ถูกปิด/หยุด ให้ตั้งค่าเป็น Idle ทันที
            _animator.SetBool("IsMoveing", false);
            _animator.SetFloat("MoveX", 0f);
            _animator.SetFloat("MoveZ", 0f);
            return;
        }

        // 2. ดึงความเร็วในพิกัดโลก (World Space Velocity)
        Vector3 worldVelocity = _agent.velocity;
        
        float totalSpeed = worldVelocity.magnitude;

        // 3. ตั้งค่า IsMoving
        // ใช้ค่าที่สูงกว่า 0.01f เพื่อหลีกเลี่ยง Jittering เมื่อ Agent หยุดนิ่งสนิท
        bool isMoving = totalSpeed > 0.01f;
        _animator.SetBool("IsMoveing", isMoving);

        // 4. ถ้ากำลังเคลื่อนที่ ให้คำนวณทิศทาง
        if (isSkilling)
        {
            Vector3 attackDirection = (_aiController.playerTarget.position - transform.position).normalized;
            _animator.SetFloat("ActionX", attackDirection.x);
            _animator.SetFloat("ActionZ", attackDirection.z);
        }
        else if (isMoving)
        {
            spriteRenderer.flipX = false;

            // 4a. แปลงความเร็วจาก World Space ให้เป็น Local Space ของตัวละคร
            // นี่คือขั้นตอนสำคัญ: มันบอกว่าความเร็วนี้เมื่อเทียบกับทิศทางที่ตัวละครกำลังหันหน้าไปเป็นอย่างไร
            Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity.normalized);
            // 4b. ดึงค่าสำหรับ Blend Tree (แกน X คือด้านข้าง, แกน Z คือเดินหน้า/ถอยหลัง)
            // ใช้ Math.Clamp เพื่อจำกัดค่าให้อยู่ระหว่าง -1 ถึง 1
            float moveX = localVelocity.x; // ด้านข้าง (Strafe Left/Right)
            float moveZ = localVelocity.z; // เดินหน้า/ถอยหลัง (Forward/Backward)

            // 4c. ส่งค่าให้ Animator
            // ใช้ Mathf.Lerp เพื่อให้การเปลี่ยน Animation ดูนุ่มนวลขึ้น (Smooth)
            float currentMoveX = _animator.GetFloat("MoveX");
            float currentMoveZ = _animator.GetFloat("MoveZ");
            //float dampTime = 0.1f; // ค่าความหน่วง

            //_animator.SetFloat("MoveX", Mathf.Lerp(currentMoveX, moveX, dampTime));
            //_animator.SetFloat("MoveZ", Mathf.Lerp(currentMoveZ, moveZ, dampTime));
            _animator.SetFloat("MoveX", moveX);
            _animator.SetFloat("MoveZ", moveZ);
        }
        else
        {
            // ถ้าหยุดเดิน ให้ Lerp ค่ากลับไปที่ 0 เพื่อให้ Animation กลับสู่ Idle อย่างนุ่มนวล
            _animator.SetFloat("MoveX", Mathf.Lerp(_animator.GetFloat("MoveX"), 0f, 0.1f));
            _animator.SetFloat("MoveZ", Mathf.Lerp(_animator.GetFloat("MoveZ"), 0f, 0.1f));
        }
    }
}
