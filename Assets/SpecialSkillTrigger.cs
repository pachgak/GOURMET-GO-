using UnityEngine;

public class SpecialSkillTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("ลากตัวผู้เล่นมาใส่ตรงนี้ (ถ้าไม่ใส่ ระบบจะหาแท็ก Player ให้อัตโนมัติ)")]
    public GameObject playerObj;

    [Header("Special Skill Settings")]
    [Tooltip("ลาก PlayerSkillSO หรือ AttacksSkill ที่เป็นสกิลเคลียร์มอนวงกว้างมาใส่ตรงนี้")]
    public PlayerSkillSO ultimateClearSkill;

    [Tooltip("ตัวคูณความแรงของสกิล (ค่าเริ่มต้นคือ 1)")]
    public float damageMultiplier = 1f;

    private void Start()
    {
        // 1. ถ้าลืมใส่ Player ไว้ ให้ระบบพยายามหาจาก Tag "Player" อัตโนมัติ
        if (playerObj == null)
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player");
            if (foundPlayer != null)
            {
                playerObj = foundPlayer;
            }
            else
            {
                Debug.LogWarning(" [SpecialSkillTrigger] หาตัวผู้เล่นไม่เจอ! ตรวจสอบว่ามี Object ที่เซ็ต Tag เป็น 'Player' หรือยัง");
            }
        }
    }

    private void Update()
    {
        // ตรวจสอบว่ามีการกดปุ่ม F3 หรือไม่
        if (Input.GetKeyDown(KeyCode.F3))
        {
            TriggerUltimateSkill();
        }

        // ตรวจสอบว่ามีการกดปุ่ม F4 หรือไม่
        if (Input.GetKeyDown(KeyCode.F4))
        {
            if (MenuIndexManager.Instance != null)
            {
                MenuIndexManager.Instance.UnlockAllMenus();
            }
        }
    }

    [ContextMenu("Trigger Ultimate Skill")]
    private void TriggerUltimateSkill()
    {
        if (ultimateClearSkill == null)
        {
            Debug.LogWarning(" [SpecialSkillTrigger] ยังไม่ได้ใส่สกิลในช่อง Ultimate Clear Skill!");
            return;
        }

        // 2. เช็คกันเหนียวว่าหาผู้เล่นเจอไหม
        if (playerObj == null)
        {
            Debug.LogWarning(" [SpecialSkillTrigger] ไม่มีตัวอ้างอิง PlayerObj เลยร่ายสกิลไม่ได้!");
            return;
        }

        // 3. *** เปลี่ยนมาใช้ตำแหน่งและทิศทางของ playerObj แทน ***
        Vector3 targetPosition = playerObj.transform.position + playerObj.transform.forward;

        // 4. ส่ง playerObj เข้าไปเป็นคนร่ายสกิล
        ultimateClearSkill.Use(playerObj, targetPosition, damageMultiplier);

        Debug.Log(" [SpecialSkillTrigger] กด F3: ใช้งานสกิลเคลียร์มอนสเตอร์วงกว้างแล้ว!");
    }
}