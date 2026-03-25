using System.Collections;
using UnityEngine;

public class ShamakiriContainerCombat : BaseEnemyCombat
{
    [Header("Shamakiri Phase Settings")]
    public bool isEnraged = false; // สถานะโกรธ
    public float enragedAttackSpeed = 1.5f; // ตัวคูณความเร็วท่าโจมตีตอนโกรธ (ยิ่งมากยิ่งร่ายไว)

    // ==========================================
    // ฟังก์ชันสำหรับเปิดโหมดโกรธ (ถูกเรียกจาก Controller ตอนลูกน้องตายหมด)
    // ==========================================
    public void SetEnragePhase(bool state)
    {
        isEnraged = state;

        if (isEnraged)
        {
            _currentSkillIndex = 1; // บังคับให้เริ่มใช้สกิลโจมตี (ข้ามสกิล 0)

            // (Option เสริม) ถ้าอยากให้มันวิ่งเร็วขึ้นตอนโกรธ ดึง Movement มาแก้ได้ตรงนี้ครับ
            if (TryGetComponent(out BaseEnemyMovement move))
            {
                move.chaseSpeed += 2f;
            }

            Debug.Log("<color=red>[Shamakiri] ร่างแม่เข้าสู่สภาวะคลุ้มคลั่ง!!</color>");
        }
    }

    // ==========================================
    // เขียนทับ (Override) ลำดับการตัดสินใจใช้สกิล
    // ==========================================
    protected override IEnumerator AttackLogic()
    {
        if (enemySkills == null || enemySkills.Length == 0) yield break;

        if (!isEnraged)
        {
            // --- Phase 1: ร่างปกติ ---
            // บังคับใช้แค่สกิล 0 (SplitSelfAction) ด้วยความเร็วปกติ
            if (enemySkills[0] != null)
            {
                yield return UseSkill(0, 1.0f);
            }
        }
        else
        {
            // --- Phase 2: ร่างโกรธ ---
            // ดักจับเผื่อ Index ไปตกอยู่ที่ 0 (ห้ามใช้สกิลแยกร่างแล้ว)
            if (_currentSkillIndex == 0) _currentSkillIndex = 1;

            // ตรวจสอบว่ามีสกิลในช่องนั้นๆ จริง ค่อยร่าย
            if (_currentSkillIndex < enemySkills.Length && enemySkills[_currentSkillIndex] != null)
            {
                // ใช้สกิลตาม Index พร้อมกับตัวคูณความเร็วร่างโกรธ!
                yield return UseSkill(_currentSkillIndex, enragedAttackSpeed);
            }

            // เลื่อนคิวสกิลให้วนลูปเฉพาะ [1], [2], [3]
            _currentSkillIndex++;
            if (_currentSkillIndex >= enemySkills.Length)
            {
                _currentSkillIndex = 1; // วนกลับไปเริ่มที่ 1 ใหม่
            }
        }
    }
}