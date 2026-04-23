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
    public List<string> acquiredSkillIDs = new List<string>(); // สกิลที่มีทั้งหมดใน Loadout
    public string[] skillBarIDs = new string[4]; // สกิลที่ใส่ไว้ในช่องกด F1-F4

    // --- 4. ความคืบหน้าเกม ---
    public List<string> finishedMenuIDs = new List<string>(); // เมนูที่ปลดล็อคแล้ว
    public List<string> triggeredRewardIDs = new List<string>(); // เควสหรือรางวัลที่เคยรับไปแล้ว
}