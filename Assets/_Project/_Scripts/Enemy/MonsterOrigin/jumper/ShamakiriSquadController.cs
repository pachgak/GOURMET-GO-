using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ShamakiriSquadController : MonoBehaviour
{
    public List<GameObject> graphicesParent;

    [Header("Squad Status")]
    public List<BaseEnemyAI> activeClones = new List<BaseEnemyAI>();

    [Header("Turn Sequence Settings")]
    public float turnCooldown = 4f;
    private float _turnTimer;
    private bool _isDoingCombo = false;

    // ตัวแปรใหม่สำหรับระบบคิว (Index)
    private int _currentAttackerIndex = 0;
    private BaseEnemyAI _currentAttackingClone = null; // เก็บตัวที่กำลังวิ่งไปตี
    private bool _isAttacking = false;

    [Header("Formation Settings")]
    public float formationRadius = 6f;
    public float formationRotateSpeed = 20f;
    private float _formationAngle = 0f;
    private Transform _playerTarget;

    public void InitializeSquad(List<GameObject> clones, GameObject target)
    {
        activeClones.Clear();
        _playerTarget = target.transform;

        if (clones.Count >= 3)
        {
            foreach (var cloneObj in clones)
            {
                if (cloneObj.TryGetComponent(out BaseEnemyAI cloneAI))
                {
                    activeClones.Add(cloneAI);

                    if (cloneObj.TryGetComponent(out EnemyHealth health))
                    {
                        health.OnDie += () => HandleCloneDeath(cloneAI);
                    }

                    // เกิดมาปุ๊บ เข้าโหมด Standby เพื่อจัด 3 เหลี่ยม
                    cloneAI.TriggerChangeState(BaseEnemyAI.EnemyState.Standby);
                }
            }
        }

        // ปิดการแสดงผลและการทำงานของตัวแม่
        foreach (var graphice in graphicesParent) graphice.SetActive(false);
        if (TryGetComponent(out Collider col)) col.enabled = false;
        if (TryGetComponent(out NavMeshAgent agent)) agent.enabled = false;
        if (TryGetComponent(out BaseEnemyAI ai)) ai.enabled = false;
        if (TryGetComponent(out BaseEnemyMovement move)) move.enabled = false;
        if (TryGetComponent(out BaseEnemyCombat combat)) combat.enabled = false;
        if (TryGetComponent(out EnemyHealth healthUser)) healthUser.enabled = false;

        _turnTimer = turnCooldown;
        _currentAttackerIndex = 0; // เริ่มที่คิวแรก
        Debug.Log("Shamakiri Puppet Master: เริ่มค่ายกล 3 เหลี่ยม!");
    }

    private void Update()
    {
        if (activeClones.Count == 0 || _isDoingCombo) return;

        // 1. จัดค่ายกล 3 เหลี่ยมตลอดเวลา (ขยับเฉพาะตัวที่ Standby)
        MaintainTriangleFormation();

        // 2. เช็คว่าตัวที่ส่งไปตี กลับมาหรือยัง
        if (_isAttacking)
        {
            // ถ้าตัวที่ตีอยู่ ตายไปแล้ว หรือ กลับมา Standby เรียบร้อยแล้ว (ตีเสร็จ)
            if (_currentAttackingClone == null || _currentAttackingClone.currentState == BaseEnemyAI.EnemyState.Standby)
            {
                _isAttacking = false;
                _currentAttackingClone = null;

                _currentAttackerIndex++; // เลื่อนบัตรคิวไปคนถัดไป
                _turnTimer = turnCooldown; // รีเซ็ตเวลาพัก
            }
            return; // ยังตีไม่เสร็จ ให้รอไปก่อน ไม่ต้องนับเวลา
        }

        // 3. ระบบนับเวลาเพื่อส่งคิวถัดไปออกไปตี
        _turnTimer -= Time.deltaTime;
        if (_turnTimer <= 0)
        {
            ExecuteNextTurn();
        }
    }

    // --- ระบบค่ายกล 3 เหลี่ยม ---
    private void MaintainTriangleFormation()
    {
        if (_playerTarget == null) return;

        _formationAngle += formationRotateSpeed * Time.deltaTime;

        // คำนวณองศาแบ่งตามจำนวนโคลนที่เหลืออยู่ (ถ้าเหลือ 3 ก็ 120 องศา, ถ้าเหลือ 2 ก็ 180 องศา)
        float angleStep = 360f / activeClones.Count;

        for (int i = 0; i < activeClones.Count; i++)
        {
            BaseEnemyAI clone = activeClones[i];

            if (clone == null || clone.currentState != BaseEnemyAI.EnemyState.Standby) continue;

            float angle = _formationAngle + (i * angleStep);
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * formationRadius;
            Vector3 targetPos = _playerTarget.position + offset;

            if (clone.TryGetComponent(out NavMeshAgent agent) && agent.isActiveAndEnabled)
            {
                agent.isStopped = false;
                agent.SetDestination(targetPos);
            }
        }
    }

    // --- ระบบรันคิวแบบใหม่ (ใช้ Index) ---
    private void ExecuteNextTurn()
    {
        // 1. ถ้าทุกคนตีครบ 1 รอบแล้ว (Index ทะลุจำนวนคน)
        if (_currentAttackerIndex >= activeClones.Count)
        {
            _currentAttackerIndex = 0; // รีเซ็ตคิวกลับมาเริ่มที่คนแรก

            // ถ้ามีครบ 3 ตัว ให้ใช้ท่าคอมโบผสานก่อนเริ่มลูปใหม่
            if (activeClones.Count == 3)
            {
                StartCoroutine(UltimateComboAttack());
                return;
            }
        }

        // 2. สั่งตัวคิวปัจจุบันให้ไปตี
        if (_currentAttackerIndex < activeClones.Count)
        {
            _currentAttackingClone = activeClones[_currentAttackerIndex];
            _isAttacking = true;

            Debug.Log($"[Shamakiri] ส่งลูกน้องคิวที่ {_currentAttackerIndex} ลุย!");

            // สั่งให้มันเปลี่ยนเป็น Chase แล้วปล่อยให้ AI มันจัดการที่เหลือเอง!
            _currentAttackingClone.TriggerChangeState(BaseEnemyAI.EnemyState.Chase);
        }
    }

    // --- จัดการเวลาโคลนตาย ---
    private void HandleCloneDeath(BaseEnemyAI deadClone)
    {
        if (activeClones.Contains(deadClone))
        {
            activeClones.Remove(deadClone);
            Debug.Log($"โคลนตาย! เหลือ {activeClones.Count} ตัว (หมดสิทธิ์ใช้ท่าคอมโบ)");

            // ป้องกันบั๊ก Index เกินจำนวนใน List เมื่อมีตัวตาย
            if (_currentAttackerIndex >= activeClones.Count)
            {
                _currentAttackerIndex = 0;
            }

            if (activeClones.Count == 1)
            {
                TriggerEnrageMode(activeClones[0]);
            }
            else if (activeClones.Count == 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private void TriggerEnrageMode(BaseEnemyAI lastClone)
    {
        Debug.Log("ร่างสุดท้าย โกรธแล้ว! คืนค่าการไล่ล่าอิสระ");
        // สั่งให้ตัวสุดท้ายบ้าคลั่ง ไล่ตีเองตลอดเวลา ไม่ต้องกลับมา 3 เหลี่ยมแล้ว
        lastClone.TriggerChangeState(BaseEnemyAI.EnemyState.Chase);

        // เราสามารถล้าง ShamakiriCloneAI ออก แล้วใส่ลูกเล่นเพิ่มได้
        // หรือปล่อยให้มัน Chase ไปเรื่อยๆ ก็ได้เพราะมันจะไม่กลับมา Standby แล้ว (ถ้าเราไม่สั่ง)
    }

    // --- ท่าผสานกระโดด 3 ตัว (ยังใช้เหมือนเดิม) ---
    private IEnumerator UltimateComboAttack()
    {
        _isDoingCombo = true;
        Debug.Log("เริ่มท่าผสาน: Shamakiri Triple Strike!");

        foreach (var clone in activeClones)
        {
            if (clone == null) continue;
            clone.TriggerChangeState(BaseEnemyAI.EnemyState.Standby);
            if (clone.TryGetComponent(out NavMeshAgent agent)) agent.isStopped = true;
        }

        yield return new WaitForSeconds(0.5f);

        foreach (var clone in activeClones)
        {
            if (clone != null && clone.TryGetComponent(out BaseEnemyMovement movement))
            {
                movement.SkillJump(clone.transform.position, 10f, 1.5f);
            }
        }

        yield return new WaitForSeconds(1.5f);
        Debug.Log("ปล่อยพลังจากฟ้าฟาดลงมา 3 เส้น!");
        yield return new WaitForSeconds(0.75f);

        // กลับสู่ค่ายกล 3 เหลี่ยม
        foreach (var clone in activeClones)
        {
            if (clone != null)
            {
                clone.TriggerChangeState(BaseEnemyAI.EnemyState.Standby);
            }
        }

        _isDoingCombo = false;
        // รีเซ็ตเวลาเพื่อเริ่มคิวของคนแรกในรอบใหม่
        _turnTimer = turnCooldown;
    }
}