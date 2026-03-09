using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShamakiriSquadController : MonoBehaviour
{
    [Header("Squad Settings")]
    public List<BaseEnemyAI> activeClones = new List<BaseEnemyAI>();

    [Header("Combo Attack Setup")]
    public float comboCooldown = 15f; // เวลาคูลดาวน์ท่าผสานกระโดด
    private float _comboTimer;
    private bool _isDoingCombo = false;

    // ฟังก์ชันนี้ถูกเรียกจาก SplitSelfAction หลังจากเสกมอน 3 ตัวเสร็จ
    public void InitializeSquad(List<GameObject> clones, GameObject target)
    {
        activeClones.Clear(); // ล้างของเก่าทิ้งเผื่อเรียกจาก Pool

        foreach (var cloneObj in clones)
        {
            if (cloneObj.TryGetComponent(out BaseEnemyAI ai))
            {
                activeClones.Add(ai);

                // *** ดักจับ Event ตอนโคลนตาย ***
                if (cloneObj.TryGetComponent(out EnemyHealth health))
                {
                    // พอตายปุ๊บ ให้เรียกฟังก์ชัน HandleCloneDeath
                    health.OnDie += () => HandleCloneDeath(ai);
                }
            }
        }

        // ซ่อนตัว Container (ตัวแม่ล่องหน)
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sp in sprites) sp.enabled = false;

        if (TryGetComponent(out Collider col)) col.enabled = false;
        if (TryGetComponent(out UnityEngine.AI.NavMeshAgent agent)) agent.enabled = false;

        // เริ่มจับเวลาท่าผสาน
        _comboTimer = comboCooldown;
        Debug.Log("Shamakiri Puppet Master: เริ่มคุมเกม!");
    }

    private void Update()
    {
        // ถ้าเหลือ 3 ตัวครบ และคูลดาวน์เสร็จ ให้ใช้ท่าผสาน!
        if (activeClones.Count == 3 && !_isDoingCombo)
        {
            _comboTimer -= Time.deltaTime;
            if (_comboTimer <= 0)
            {
                StartCoroutine(UltimateComboAttack());
                _comboTimer = comboCooldown;
            }
        }
    }

    // --- จัดการเวลาโคลนตาย ---
    private void HandleCloneDeath(BaseEnemyAI deadClone)
    {
        if (activeClones.Contains(deadClone))
        {
            activeClones.Remove(deadClone);
            Debug.Log($"โคลนตาย! เหลือ {activeClones.Count} ตัว");

            if (activeClones.Count == 1)
            {
                // *** เหลือตัวสุดท้าย สั่งเข้าโหมด Enrage! ***
                TriggerEnrageMode(activeClones[0]);
            }
            else if (activeClones.Count == 0)
            {
                // ตายหมดแล้ว! ตัว Container จบหน้าที่ ดรอปของ แล้วทำลายตัวเองทิ้ง
                Debug.Log("Shamakiri Defeated!");
                Destroy(gameObject);
            }
        }
    }

    private void TriggerEnrageMode(BaseEnemyAI lastClone)
    {
        Debug.Log("ร่างสุดท้าย โกรธแล้ว! ปลดล็อคทุกสกิล!");

        // ตรงนี้ให้คุณเช็ค Component Combat ของโคลนตัวสุดท้าย แล้วสั่งให้มันเปลี่ยนโหมด
        if (lastClone.TryGetComponent(out BaseEnemyCombat combat))
        {
            // ตัวอย่าง: ถ้าคุณสร้าง ShamakiriCombat ไว้
            // if (combat is ShamakiriCombat shamakiriCombat) shamakiriCombat.EnableEnrage();
        }
    }

    // --- ท่าผสานกระโดด 3 ตัว ---
    private IEnumerator UltimateComboAttack()
    {
        _isDoingCombo = true;
        Debug.Log("เริ่มท่าผสาน: Shamakiri Triple Strike!");

        // 1. สั่งให้ทุกคนหยุดเดินไล่ผู้เล่น
        foreach (var clone in activeClones)
        {
            if (clone == null) continue;
            clone.TriggerChangeState(BaseEnemyAI.EnemyState.Roaming); // เปลี่ยนเป็น Roaming เพื่อหยุดชั่วคราว
            clone.GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;
        }

        yield return new WaitForSeconds(0.5f); // ยืนโพสท่าแป๊บนึง

        // 2. สั่งทุกคนกระโดดขึ้นฟ้าพร้อมกัน (สมมติว่าสูง 10 หน่วย นาน 1.5 วิ)
        foreach (var clone in activeClones)
        {
            if (clone != null && clone.TryGetComponent(out BaseEnemyMovement movement))
            {
                movement.SkillJump(clone.transform.position, 10f, 1.5f);
            }
        }

        // 3. รอจังหวะที่มันลอยอยู่บนฟ้า (ประมาณ 0.75 วิ)
        yield return new WaitForSeconds(0.75f);

        // 4. สั่งปล่อยพลังเส้นตรงลงมา (TODO: เอา ObjectPoolingManager มาเสก Hitbox เส้นตรงใส่ Player)
        Debug.Log("ปล่อยพลังจากฟ้าฟาดลงมา 3 เส้น!");

        yield return new WaitForSeconds(0.75f); // รอจนมันตกถึงพื้นพอดี

        // 5. ปล่อยให้มันกลับไปไล่ตีผู้เล่นตามปกติ
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