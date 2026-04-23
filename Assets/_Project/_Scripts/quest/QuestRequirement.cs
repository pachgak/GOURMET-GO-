using System;
using UnityEngine;
using Inventory.Model;
using Inventory; // อ้างอิงระบบ Inventory ของคุณ

[Serializable]
public abstract class QuestRequirement
{
    // เช็คว่าผ่านเงื่อนไขหรือยัง
    public abstract bool IsMet(GameObject player);

    // ดึงข้อความแสดงความคืบหน้า เช่น "Sky Egg (0/1)"
    public abstract string GetProgressText(GameObject player);

    // หักของ (ถ้ามี) ตอนส่งเควส
    public virtual void ConsumeRequirement(GameObject player) { }
}

[Serializable]
public class ItemRequirement : QuestRequirement
{
    public ItemSO requiredItem;
    public int amount = 1;

    public override bool IsMet(GameObject player)
    {
        // --- เพิ่มเงื่อนไข player != null ดักเอาไว้ก่อน ---
        if (player != null && player.TryGetComponent(out InventoryController inv))
        {
            return GetItemCount(inv.InventoryData) >= amount;
        }
        return false;
    }

    public override string GetProgressText(GameObject player)
    {
        int current = 0;
        if (player != null && player.TryGetComponent(out InventoryController inv))
        {
            current = GetItemCount(inv.InventoryData);
        }
        // คืนค่าข้อความ เช่น "Sky Egg (0/1)"
        string color = IsMet(player) ? "#2baf2b" : "red";
        return $"<color={color}>{requiredItem.ItemName} ({current}/{amount})</color>";
    }

    public override void ConsumeRequirement(GameObject player)
    {
        if (player.TryGetComponent(out InventoryController inv))
        {
            // เรียกฟังก์ชันหักไอเทมที่คุณมีในกระเป๋า (ต้องไปปรับใช้ตามโค้ดกระเป๋าของคุณ)
            // inv.InventoryData.RemoveItemByID(requiredItem.ID, amount);
        }
    }

    private int GetItemCount(InventorySO inventory)
    {
        int count = 0;
        foreach (var item in inventory.GetCurrentInventoryState().Values)
        {
            if (!item.IsEmpty && item.item.IDg == requiredItem.IDg) count += item.quantity;
        }
        return count;
    }
}

[Serializable]
public class CheckItemRequirement : ItemRequirement
{
    public override void ConsumeRequirement(GameObject player)
    {
        //nope
    }
}

[Serializable]
public class SkillRequirement : QuestRequirement
{
    public PlayerSkillSO requiredSkill;

    public override bool IsMet(GameObject player)
    {
        // เช็คว่าหาผู้เล่นเจอไหม และมีสคริปต์ PlayerSkill แปะอยู่ไหม
        if (player != null && player.TryGetComponent(out PlayerSkill playerSkill))
        {
            // ใช้ Helper Method ที่เราเพิ่งสร้าง!
            return playerSkill.HasSkill(requiredSkill);
        }
        return false;
    }

    public override string GetProgressText(GameObject player)
    {
        bool isMet = IsMet(player);

        // เช็คสี (เขียวถ้าผ่าน แดงถ้ายังไม่ผ่าน)
        string color = isMet ? "#2baf2b" : "red";

        string skillName = requiredSkill != null ? requiredSkill.name : "Unknown Skill";

        return $"<color={color}>ติดตั้งสกิล {skillName} </color>";
    }

    [Serializable]
    public class FinishedMenuIndexRequirement : QuestRequirement
    {
        public override bool IsMet(GameObject player)
        {
            // 1. เช็คความปลอดภัยว่ามี Manager อยู่ในฉากไหม
            if (MenuIndexManager.Instance == null) return false;

            // 2. นับจำนวนเมนูที่ทำสำเร็จแล้ว
            int finishedCount = 0;
            foreach (var recipe in MenuIndexManager.Instance.MenuIndexList)
            {
                if (MenuIndexManager.Instance.IsMenuFinished(recipe))
                {
                    finishedCount++;
                }
            }

            // 3. เทียบว่าจำนวนที่ทำเสร็จ มากกว่าหรือเท่ากับ จำนวนเมนูทั้งหมดหรือไม่
            return finishedCount >= MenuIndexManager.Instance.MenuIndexList.Count;
        }

        public override string GetProgressText(GameObject player)
        {
            int current = 0;
            int total = 0;

            if (MenuIndexManager.Instance != null)
            {
                total = MenuIndexManager.Instance.MenuIndexList.Count;

                foreach (var recipe in MenuIndexManager.Instance.MenuIndexList)
                {
                    if (MenuIndexManager.Instance.IsMenuFinished(recipe))
                    {
                        current++;
                    }
                }
            }

            bool isMet = IsMet(player);

            // เช็คสี (เขียวถ้าผ่าน แดงถ้ายังไม่ผ่าน)
            string color = isMet ? "#2baf2b" : "red";

            return $"<color={color}>({current}/{total})</color>";
        }

        public override void ConsumeRequirement(GameObject player)
        {
            // เงื่อนไขนี้เป็นแค่สถิติ/ความสำเร็จ (Achievement) ไม่ต้องหักไอเทมอะไรออก ปล่อยว่างไว้ได้เลย
        }
    }
}