using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    // --- 1. ข้อมูลผู้เล่น ---
    public float currentHealth;
    public float maxHealth;
    public float[] playerPosition = new float[3]; // เก็บ [x, y, z]

    // --- 2. ข้อมูลไอเทม (Inventory & Storage) ---
    [System.Serializable]
    public struct SavedItem
    {
        public int slotIndex;
        public string itemID;
        public int amount;
    }
    public List<SavedItem> inventoryItems = new List<SavedItem>();
    public List<SavedItem> storageItems = new List<SavedItem>();

    // --- 3. ข้อมูลสกิล ---
    [System.Serializable]
    public struct SavedLoadoutSkill
    {
        public string skillID;
        public int exp;
    }
    public List<SavedLoadoutSkill> acquiredSkills = new List<SavedLoadoutSkill>(); // เปลี่ยนมาใช้ List นี้แทน

    // *** เพิ่มโครงสร้างสำหรับ Skill Bar เข้ามาแทน string[] แบบเก่า ***
    [System.Serializable]
    public struct SavedSkillBarItem
    {
        public int slotIndex;
        public string skillID;
        public int usedCount;
    }
    public List<SavedSkillBarItem> skillBarItems = new List<SavedSkillBarItem>();

    // --- 4. บัฟในตัว Player ---
    [System.Serializable]
    public struct SavedBuff
    {
        public string buffID;
        public float timeRemaining; // เวลาบัฟที่เหลืออยู่
        public int stacks;          // จำนวน Stack ของบัฟนั้น
    }
    public List<SavedBuff> activeBuffs = new List<SavedBuff>();


    // --- 5. ความคืบหน้าเกม ---
    public List<string> finishedMenuIDs = new List<string>(); // เมนูที่ปลดล็อคแล้ว

    // --- 6. ความคืบหน้าเควส ---
    [System.Serializable]
    public struct SavedNPCQuest
    {
        public string npcID;
        public int questIndex;
        public int stepIndex;
    }
    public List<SavedNPCQuest> npcQuestProgress = new List<SavedNPCQuest>();

    //อันนี้มีอยู่แล้ว ใช้สำหรับเก็บ RewardID ที่เคยถูกเปิดไปแล้ว (5.2 พื้นที่/สิ่งของ)
    public List<string> triggeredRewardIDs = new List<string>();
}