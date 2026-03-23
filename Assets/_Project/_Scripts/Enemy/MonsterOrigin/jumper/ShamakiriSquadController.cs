using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShamakiriSquadController : MonoBehaviour
{
    public enum SquadTurn { Melee, Range, AoE, Combo }

    public List<GameObject> graphicesParent;

    [Header("Squad Status")]
    public SquadTurn currentTurn = SquadTurn.Melee; // คิวปัจจุบัน
    public List<BaseEnemyAI> activeClones = new List<BaseEnemyAI>();

    [Header("Specific Clones")]
    private BaseEnemyAI _meleeClone;
    private BaseEnemyAI _rangeClone;
    private BaseEnemyAI _aoeClone;

    [Header("Turn Sequence Settings")]
    public float turnCooldown = 4f; // เวลาพักระหว่างการสั่งแต่ละตัว
    private float _turnTimer;
    private bool _isDoingCombo = false;

    // ฟังก์ชันนี้ถูกเรียกจาก SplitSelfAction หลังจากเสกมอน 3 ตัวเสร็จ
    public void InitializeSquad(List<GameObject> clones, GameObject target)
    {
        activeClones.Clear();

        // *** สำคัญ: ใน List ของ SplitSelfAction ต้องใส่ Prefab เรียงตามนี้เสมอ:
        // Index 0 = Melee, Index 1 = Range, Index 2 = AoE
        if (clones.Count >= 3)
        {
            _meleeClone = clones[0].GetComponent<BaseEnemyAI>();
            _rangeClone = clones[1].GetComponent<BaseEnemyAI>();
            _aoeClone = clones[2].GetComponent<BaseEnemyAI>();

            activeClones.Add(_meleeClone);
            activeClones.Add(_rangeClone);
            activeClones.Add(_aoeClone);

            // สมัครรับ Event ตอนตาย
            foreach (var clone in activeClones)
            {
                if (clone.TryGetComponent(out EnemyHealth health))
                {
                    health.OnDie += () => HandleCloneDeath(clone);
                }
            }
        }

        foreach (var graphice in graphicesParent)
        {
            graphice.SetActive(false);
        }

        // ซ่อนตัว Container (ตัวแม่ล่องหน)

        if (TryGetComponent(out Collider col)) col.enabled = false;
        if (TryGetComponent(out UnityEngine.AI.NavMeshAgent agent)) agent.enabled = false;

        // 3. *** เพิ่มตรงนี้: ปิดสคริปต์ AI และ Combat ตัวแม่จะได้ไม่ไปแย่งลูกน้องตี ***
        if (TryGetComponent(out BaseEnemyAI ai)) ai.enabled = false;
        if (TryGetComponent(out BaseEnemyMovement move)) move.enabled = false;
        if (TryGetComponent(out BaseEnemyCombat combat)) combat.enabled = false;

        // 4. *** เพิ่มตรงนี้: ทำให้ตัวแม่เป็นอมตะ (กันโดนผู้เล่นสาดสกิลหมู่มาโดน) ***
        // หรือถ้าใน EnemyHealth มีคำสั่งปิด ก็ปิดได้เลย
        if (TryGetComponent(out EnemyHealth healthUser))
        {
            healthUser.enabled = false;
        }

        // เริ่มจับเวลาคิวแรก
        _turnTimer = turnCooldown;
        currentTurn = SquadTurn.Melee; // เซ็ตให้เริ่มที่ Melee เสมอ
        Debug.Log("Shamakiri Puppet Master: เริ่มคุมเกม! ลำดับแรก: Melee");
    }

    private void Update()
    {
        // ถ้าร่างโคลนตายไม่ครบ 3 ตัว ลูปคอมโบจะพัง ให้หยุดระบบคิว แล้วไปพึ่งโหมด Enrage แทน
        if (activeClones.Count < 3 || _isDoingCombo) return;

        _turnTimer -= Time.deltaTime;
        if (_turnTimer <= 0)
        {
            ExecuteNextTurn();
            _turnTimer = turnCooldown; // รีเซ็ตเวลาสำหรับตาถัดไป
        }
    }

    // --- ระบบรันคิว ---
    private void ExecuteNextTurn()
    {
        switch (currentTurn)
        {
            case SquadTurn.Melee:
                CommandAttack(_meleeClone, "Melee");
                currentTurn = SquadTurn.Range; // เปลี่ยนคิวถัดไปเป็น Range
                break;

            case SquadTurn.Range:
                CommandAttack(_rangeClone, "Range");
                currentTurn = SquadTurn.AoE;   // เปลี่ยนคิวถัดไปเป็น AoE
                break;

            case SquadTurn.AoE:
                CommandAttack(_aoeClone, "AoE");
                currentTurn = SquadTurn.Combo; // เปลี่ยนคิวถัดไปเป็น Combo
                break;

            case SquadTurn.Combo:
                StartCoroutine(UltimateComboAttack());
                currentTurn = SquadTurn.Melee; // จบคอมโบ วนลูปกลับไปเริ่มที่ Melee ใหม่
                break;
        }
    }

    // --- ฟังก์ชันสั่งลูกน้องโจมตี ---
    private void CommandAttack(BaseEnemyAI clone, string roleName)
    {
        if (clone == null) return;

        Debug.Log($"[Shamakiri] ผู้กำกับสั่งลูกน้อง {roleName} โจมตี!");

        // บังคับให้ AI เข้า State Attack เพื่อเริ่มการโจมตี
        // (ถึงแม้ Attack Range จะเป็น 0 การสั่ง Trigger ตรงๆ ก็จะทำให้มันร่ายสกิลได้ครับ)
        clone.TriggerChangeState(BaseEnemyAI.EnemyState.Attack);

        // ถ้าคุณมีระบบ Command อื่นๆ ใน BaseEnemyCombat สามารถนำมาเรียกตรงนี้ได้เลย
        // เช่น clone.GetComponent<BaseEnemyCombat>().ExecuteSkillAction(0);
    }

    // --- จัดการเวลาโคลนตาย ---
    private void HandleCloneDeath(BaseEnemyAI deadClone)
    {
        if (activeClones.Contains(deadClone))
        {
            activeClones.Remove(deadClone);
            Debug.Log($"โคลนตาย! เหลือ {activeClones.Count} ตัว ระบบคิว(Sequence) ถูกยกเลิก!");

            if (activeClones.Count == 1)
            {
                TriggerEnrageMode(activeClones[0]);
            }
            else if (activeClones.Count == 0)
            {
                Debug.Log("Shamakiri Defeated!");
                Destroy(gameObject);
            }
        }
    }

    private void TriggerEnrageMode(BaseEnemyAI lastClone)
    {
        Debug.Log("ร่างสุดท้าย โกรธแล้ว! ปลดล็อคความสามารถตีเอง!");

        // พอเหลือตัวสุดท้าย เราต้องคืนค่า Attack Range ให้มันกลับมาตีเองได้
        // สมมติว่าระยะตีคือ 2f
        lastClone.attackRange = 2f;
    }

    // --- ท่าผสานกระโดด 3 ตัว ---
    private IEnumerator UltimateComboAttack()
    {
        _isDoingCombo = true;
        Debug.Log("เริ่มท่าผสาน: Shamakiri Triple Strike!");

        foreach (var clone in activeClones)
        {
            if (clone == null) continue;
            clone.TriggerChangeState(BaseEnemyAI.EnemyState.Roaming);
            clone.GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;
        }

        yield return new WaitForSeconds(0.5f);

        foreach (var clone in activeClones)
        {
            if (clone != null && clone.TryGetComponent(out BaseEnemyMovement movement))
            {
                movement.SkillJump(clone.transform.position, 10f, 1.5f);
            }
        }

        yield return new WaitForSeconds(0.75f);
        Debug.Log("ปล่อยพลังจากฟ้าฟาดลงมา 3 เส้น!");
        yield return new WaitForSeconds(0.75f);

        foreach (var clone in activeClones)
        {
            if (clone != null)
            {
                clone.GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = false;
                clone.TriggerChangeState(BaseEnemyAI.EnemyState.Chase);
            }
        }

        _isDoingCombo = false;
    }
}