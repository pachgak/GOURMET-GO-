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
        if (player.TryGetComponent(out InventoryController inv))
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
        return $"{requiredItem.ItemName} ({current}/{amount})";
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
            if (!item.IsEmpty && item.item.ID == requiredItem.ID) count += item.quantity;
        }
        return count;
    }
}

[Serializable]
public class CheckItemRequirement : QuestRequirement
{
    public ItemSO requiredItem;
    public int amount = 1;

    public override bool IsMet(GameObject player)
    {
        if (player.TryGetComponent(out InventoryController inv))
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
        return $"{requiredItem.ItemName} ({current}/{amount})";
    }

    private int GetItemCount(InventorySO inventory)
    {
        int count = 0;
        foreach (var item in inventory.GetCurrentInventoryState().Values)
        {
            if (!item.IsEmpty && item.item.ID == requiredItem.ID) count += item.quantity;
        }
        return count;
    }
}

[Serializable]
public class SkillRequirement : QuestRequirement
{
    public PlayerSkillSO requiredSkill;

    public override bool IsMet(GameObject player)
    {
        // เช็คว่าผู้เล่นใส่สกิลนี้อยู่หรือไม่ (ปรับให้เข้ากับสคริปต์ของคุณ)
        // if (player.TryGetComponent(out PlayerSkill loadout)) return loadout.HasSkill(requiredSkill);
        return true;
    }

    public override string GetProgressText(GameObject player)
    {
        string color = IsMet(player) ? "green" : "red";
        return $"<color={color}>ติดตั้งสกิล {requiredSkill.skillName}</color>";
    }
}