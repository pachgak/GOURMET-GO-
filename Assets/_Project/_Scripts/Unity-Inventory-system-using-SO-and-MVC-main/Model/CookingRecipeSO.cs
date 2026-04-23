using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;
using System;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "New Cooking Recipe", menuName = "Inventory/Cooking/Recipe")]
    public class CookingRecipeSO : ScriptableObject
    {
        [field: SerializeField]
        [Tooltip("ไอดีสำหรับเซฟเกม ห้ามซ้ำกัน (เช่น menu_fried_egg)")]
        public string ID { get; private set; }

        [Header("Ingredients (วัตถุดิบ)")]
        public List<ItemQuantity> ingredients;

        [Header("Result (อาหารที่ได้)")]
        public ItemSO resultItem;

        // เช็คว่าในกระเป๋า (หม้อ) มีของครบตามสูตรนี้ไหม
        public bool CanCook(Dictionary<int, InventoryItem> potInventoryItems)
        {
            // 1. สร้าง List จำลองของที่มีในหม้อ
            List<InventoryItem> tempPotContents = new List<InventoryItem>();

            foreach (var itemInPot in potInventoryItems.Values)
            {
                if (itemInPot.IsEmpty) continue;
                tempPotContents.Add(itemInPot);
            }

            // [Optinal] เช็คจำนวนก่อนเลย ถ้าจำนวน Slot ที่ใช้ไม่เท่ากับสูตร ก็ผิดตั้งแต่ประตูบ้าน
            // จะช่วยประหยัด Performance ไม่ต้องไปวนลูปข้างล่าง
            if (tempPotContents.Count != ingredients.Count)
            {
                return false;
            }

            // 2. วนลูปเช็ควัตถุดิบตามสูตร และ "หักลบ" ออกจาก List จำลอง
            foreach (var ingredient in ingredients)
            {
                int amountNeeded = ingredient.quantity;
                ItemSO itemNeeded = ingredient.item;

                // --- จุดที่แก้ไข ---
                // แทนที่จะวน foreach เราใช้ FindIndex หาตำแหน่งของไอเทมที่ตรงเงื่อนไขแทน
                // เงื่อนไข: ID ตรงกัน และ จำนวน >= ที่ต้องการ
                int indexFound = tempPotContents.FindIndex(x =>
                    x.item.IDg == itemNeeded.IDg &&
                    x.quantity >= amountNeeded
                );

                if (indexFound != -1)
                {
                    // เจอคู่! ลบออกจาก List จำลองทันที (เสมือนว่าถูกจับคู่ไปแล้ว)
                    tempPotContents.RemoveAt(indexFound);
                }
                else
                {
                    // หาไม่เจอ หรือมีแต่จำนวนไม่พอ -> สูตรผิดทันที
                    return false;
                }
                // ------------------
            }

            // 3. จุดตัดสินใจสำคัญ! (The Sandwich Protection)
            // หลังจากจับคู่ครบทุกวัตถุดิบในสูตรแล้ว... "ในหม้อต้องไม่เหลืออะไรเลย"

            if (tempPotContents.Count > 0)
            {
                // ยังเหลือของค้างในหม้อ แสดงว่ามีของแปลกปลอมเกินมา
                return false;
            }

            // ถ้าไม่เหลืออะไรเลย แปลว่า วัตถุดิบ เป๊ะ! (Exact Match)
            return true;
        }
    }

    [Serializable]
    public struct ItemQuantity
    {
        public ItemSO item;
        public int quantity;
    }
}