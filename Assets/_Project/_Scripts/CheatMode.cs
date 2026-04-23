using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Inventory.Model; // สำหรับ ItemSO และ InventorySO

public class CheatMode : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("ลาก Panel ของหน้าต่าง Cheat Mode มาใส่ตรงนี้")]
    public GameObject cheatPanel;

    [Tooltip("ปุ่มสำหรับใช้สกิล Ultimate")]
    public Button btnTriggerSkill;
    [Tooltip("ปุ่มสำหรับเสกไอเทม")]
    public Button btnSpawnItem;
    [Tooltip("ปุ่มสำหรับปลดล็อคเมนูอาหาร")]
    public Button btnUnlockMenu;
    [Tooltip("ปุ่มสำหรับให้สกิลใน Loadout")]
    public Button btnGiveSkill;

    [Header("1. Skill Trigger Settings")]
    public GameObject playerObj;
    public PlayerSkillSO ultimateClearSkill;
    public float damageMultiplier = 1f;

    [Header("2. Spawn Item Settings")]
    public InventorySO playerInventory;
    [System.Serializable]
    public struct CheatItem
    {
        public ItemSO item;
        public int amount;
    }
    [Tooltip("ใส่รายชื่อไอเทมและจำนวนที่ต้องการเสกเมื่อกดปุ่ม")]
    public List<CheatItem> itemsToSpawn = new List<CheatItem>();

    [Header("4. Give Loadout Skill Settings")]
    public PlayerLoadoutSkill playerLoadout;
    [System.Serializable]
    public struct CheatSkill
    {
        public PlayerSkillSO skill;
        public int exp; // จำนวน Exp หรือ Level สกิลที่ต้องการให้
    }
    [Tooltip("ใส่รายชื่อสกิลที่ต้องการให้เข้ากระเป๋า Loadout")]
    public List<CheatSkill> skillsToGive = new List<CheatSkill>();

    private void Start()
    {
        // --- 1. หา Player อัตโนมัติถ้าลืมใส่ ---
        if (playerObj == null)
        {
            playerObj = GameObject.FindWithTag("Player");
        }

        // --- 2. ซ่อนหน้าต่าง Cheat ไว้ก่อนตอนเริ่มเกม ---
        if (cheatPanel != null)
        {
            cheatPanel.SetActive(false);
        }

        // --- 3. ผูก Event ให้ปุ่มต่างๆ ---
        if (btnTriggerSkill != null) btnTriggerSkill.onClick.AddListener(TriggerUltimateSkill);
        if (btnSpawnItem != null) btnSpawnItem.onClick.AddListener(SpawnItems);
        if (btnUnlockMenu != null) btnUnlockMenu.onClick.AddListener(UnlockAllMenus);
        if (btnGiveSkill != null) btnGiveSkill.onClick.AddListener(GiveLoadoutSkills);
    }

    private void Update()
    {
        // กด F3 เพื่อเปิด/ปิด หน้าต่าง Cheat Mode
        if (Input.GetKeyDown(KeyCode.F3))
        {
            if (cheatPanel != null)
            {
                bool isActive = cheatPanel.activeSelf;
                cheatPanel.SetActive(!isActive);

                // ปลดล็อคเมาส์ให้กดปุ่มได้ (ถ้าเกมคุณล็อคเมาส์ตอนเดิน)
                if (!isActive)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }
    }

    // ==========================================
    // ฟังก์ชันการทำงานของปุ่มต่างๆ
    // ==========================================

    // 1. ใช้สกิลเคลียร์มอนสเตอร์
    private void TriggerUltimateSkill()
    {
        if (ultimateClearSkill == null || playerObj == null)
        {
            Debug.LogWarning("[CheatMode] ร่ายสกิลไม่ได้! กรุณาเช็ค ultimateClearSkill หรือ playerObj");
            return;
        }

        Vector3 targetPosition = playerObj.transform.position + playerObj.transform.forward;
        ultimateClearSkill.Use(playerObj, targetPosition, damageMultiplier);

        Debug.Log("[CheatMode] ใช้งานสกิลเคลียร์มอนสเตอร์แล้ว!");
    }

    // 2. เสกไอเทมเข้ากระเป๋า
    private void SpawnItems()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("[CheatMode] กรุณาลาก InventorySO ของ Player มาใส่ด้วย!");
            return;
        }

        int count = 0;
        foreach (var cheatItem in itemsToSpawn)
        {
            if (cheatItem.item != null)
            {
                // เรียกใช้ฟังก์ชัน AddItem จาก InventorySO
                playerInventory.AddItem(cheatItem.item, cheatItem.amount);
                count++;
            }
        }
        Debug.Log($"[CheatMode] เสกไอเทมสำเร็จ {count} รายการ!");
    }

    // 3. ปลดล็อคเมนูอาหารทั้งหมด
    private void UnlockAllMenus()
    {
        if (MenuIndexManager.Instance != null)
        {
            // เรียกฟังก์ชันที่คุณทำไว้แล้ว
            MenuIndexManager.Instance.UnlockAllMenus();
        }
        else
        {
            Debug.LogWarning("[CheatMode] หา MenuIndexManager ไม่เจอในฉาก!");
        }
    }

    // 4. ให้สกิลลงใน Loadout
    private void GiveLoadoutSkills()
    {
        if (playerLoadout == null)
        {
            Debug.LogWarning("[CheatMode] กรุณาลากสคริปต์ PlayerLoadoutSkill มาใส่ด้วย!");
            return;
        }

        int count = 0;
        foreach (var cheatSkill in skillsToGive)
        {
            if (cheatSkill.skill != null)
            {
                // เรียกใช้ฟังก์ชัน AddItem ใน LoadoutData
                playerLoadout.loadoutData.AddItem(cheatSkill.skill, cheatSkill.exp);
                count++;
            }
        }
        Debug.Log($"[CheatMode] เพิ่มสกิลเข้า Loadout สำเร็จ {count} สกิล!");
    }
}