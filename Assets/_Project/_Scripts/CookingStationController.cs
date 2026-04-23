using Inventory.Model;
using Inventory.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Inventory
{
    public class CookingStationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventorySO cookingPotInventory; // Inventory ของหม้อ
        [SerializeField] private InventorySO playerInventory;     // Inventory ของตัวผู้เล่น (เพื่อรับอาหาร)

        [Header("Drop System")]
        [SerializeField] private GameObject itemDropPrefab; // Prefab ตัว ItemDrop ที่เก็บได้
        [SerializeField] private Transform playerTransform; // ตำแหน่งที่จะดรอปของ (ปกติคือตัว Player)

        [Header("Recipe Database")]
        [SerializeField] private List<CookingRecipeSO> allRecipes; // ลากสูตรอาหารทั้งหมดมาใส่ที่นี่

        [Header("UI Preview")]
        [SerializeField] private Image resultImage;        // รูปอาหารที่จะได้
        [SerializeField] private TMP_Text resultNameText;        // รูปอาหารที่จะได้
        private UIItemDetailTrigger resultTooltipTrigger;
        [SerializeField] private Button cookButton;        // ปุ่ม Cook
        //[SerializeField] private GameObject previewPanel;  // Panel ที่รวมรูปและปุ่ม (ไว้ซ่อนถ้าสูตรไม่ตรง)

        [SerializeField] private Sprite nullResultImage;
        private CookingRecipeSO currentValidRecipe = null;

        private void Awake()
        {
            resultTooltipTrigger = resultImage.GetComponent<UIItemDetailTrigger>();
        }

        private void Start()
        {
            // ฟัง Event เมื่อของในหม้อมีการเปลี่ยนแปลง
            cookingPotInventory.OnInventoryUpdated += HandlePotUpdated;

            // ผูกปุ่ม Cook
            cookButton.onClick.AddListener(PerformCook);

            // เริ่มต้นซ่อน Preview ไปก่อน
            UpdatePreviewUI(null);
        }

        private void OnDestroy()
        {
            cookingPotInventory.OnInventoryUpdated -= HandlePotUpdated;
        }

        // ฟังก์ชันนี้จะถูกเรียกทุกครั้งที่เราลากของใส่หม้อ หรือเอาของออก
        private void HandlePotUpdated(Dictionary<int, InventoryItem> potState)
        {
            CookingRecipeSO foundRecipe = null;

            // วนลูปเช็คทุกสูตรที่มี
            foreach (var recipe in allRecipes)
            {
                if (recipe.CanCook(potState))
                {
                    foundRecipe = recipe;
                    break; // เจอสูตรที่ตรงแล้ว หยุดหา
                }
            }

            currentValidRecipe = foundRecipe;
            UpdatePreviewUI(currentValidRecipe);
        }

        private void UpdatePreviewUI(CookingRecipeSO recipe)
        {
            if (recipe != null)
            {
                //previewPanel.SetActive(true);
                resultImage.sprite = recipe.resultItem.ItemImage;
                resultNameText.text = recipe.resultItem.ItemName;
                cookButton.interactable = true;

                // ส่งข้อมูล ItemSO ไปให้ Trigger เก็บไว้
                resultTooltipTrigger.SetItemData(recipe.resultItem);
            }
            else
            {
                //previewPanel.SetActive(false); // หรือจะแค่ทำให้ปุ่มเป็นสีเทาก็ได้
                resultImage.sprite = nullResultImage;
                resultNameText.text = "";
                cookButton.interactable = false;

                // ล้างข้อมูล (เพื่อความชัวร์)
                resultTooltipTrigger.SetItemData(null);
            }
        }

        public void PerformCook()
        {
            if (currentValidRecipe == null) return;

            // 1. ลบวัตถุดิบออกจากหม้อ
            RemoveIngredientsFromPot(currentValidRecipe);

            // 2. เพิ่มอาหารใส่กระเป๋าผู้เล่น
            playerInventory.AddItem(currentValidRecipe.resultItem, 1);

            // 3. (Optional) เล่นเสียงปรุงอาหาร
            Debug.Log($"Cooked {currentValidRecipe.resultItem.name}!");
        }

        private void RemoveIngredientsFromPot(CookingRecipeSO recipe)
        {
            Dictionary<int, InventoryItem> potState = cookingPotInventory.GetCurrentInventoryState();

            foreach (var ingredient in recipe.ingredients)
            {
                int amountToRemove = ingredient.quantity;

                // ต้องวนลูปหา Index เพราะ InventorySO ของคุณลบด้วย Index
                // เราต้อง Loop จนกว่าจะลบครบจำนวน (กรณีของแยกกองกัน)
                for (int i = 0; i < cookingPotInventory.Size; i++)
                {
                    InventoryItem itemInSlot = cookingPotInventory.GetItemAt(i);

                    if (!itemInSlot.IsEmpty && itemInSlot.item.IDg == ingredient.item.IDg)
                    {
                        if (itemInSlot.quantity >= amountToRemove)
                        {
                            // กองนี้มีพอ ลบแล้วจบเลยสำหรับวัตถุดิบชนิดนี้
                            cookingPotInventory.RemoveItem(i, amountToRemove);
                            amountToRemove = 0;
                            break;
                        }
                        else
                        {
                            // กองนี้มีไม่พอ ลบให้หมดกอง แล้วไปหากองถัดไป
                            int amountInThisSlot = itemInSlot.quantity;
                            cookingPotInventory.RemoveItem(i, amountInThisSlot);
                            amountToRemove -= amountInThisSlot;
                        }
                    }
                }
            }
        }

        public void ReturnItemsToPlayer()
        {
            // 1. ดึงของทั้งหมดในหม้อมา
            Dictionary<int, InventoryItem> itemsInPot = cookingPotInventory.GetCurrentInventoryState();

            if (itemsInPot.Count == 0) return; // หม้อว่าง ก็ไม่ต้องทำอะไร

            // 2. วนลูปคืนของ
            foreach (var kvp in itemsInPot)
            {
                int slotIndex = kvp.Key;
                InventoryItem item = kvp.Value;

                if (item.IsEmpty) continue;

                // 2.1 พยายามยัดเข้ากระเป๋า Player
                int remainingQuantity = playerInventory.AddItem(item.item, item.quantity);

                // 2.2 ถ้ามีของเหลือ (กระเป๋าเต็ม) -> ดรอปลงพื้น
                if (remainingQuantity > 0)
                {
                    SpawnItemDrop(item.item, remainingQuantity);
                }

                // 2.3 ลบของออกจากหม้อ (สำคัญ! ต้องลบหลังจากย้ายเสร็จ)
                // แต่เนื่องจากเราวนลูปอยู่ การลบทีละอันในนี้อาจซ้ำซ้อน 
                // วิธีที่ดีที่สุดคือ ล้างหม้อทีเดียวตอนจบ Loop หรือ Reset ทีละช่อง
                cookingPotInventory.RemoveItem(slotIndex, item.quantity);
            }

            // (Optional) เพื่อความสะอาดเกลี้ยงเกลา ล้างหม้อทั้งหมดอีกรอบถ้าต้องการ
            // cookingPotInventory.Initialize(); 
        }

        private void SpawnItemDrop(ItemSO itemSO, int quantity)
        {
            // ใช้ Object Pooling Manager (ถ้ามี) หรือ Instantiate
            // ในที่นี้สมมติใช้ Instantiate ปกติไปก่อน หรือปรับใช้ Manager ของคุณ

            Vector3 dropPosition = playerTransform.position + (Vector3.up * 1f) + (Vector3.forward * 0.5f); // ดรอปสูงกว่าพื้นนิดหน่อย

            // ใช้ ObjectPoolingManager ที่คุณมี (แนะนำ)
            GameObject dropObj = ObjectPoolingManager.Instance.Spawn(itemDropPrefab, dropPosition);

            // เข้าถึง Component Item (Script ที่คุมการเก็บของ) แล้ว Setup ข้อมูล
            // สมมติชื่อ script คือ 'Item'
            if (dropObj.TryGetComponent(out Item itemScript))
            {
                itemScript.Setup(itemSO, quantity);
            }
        }
    }
}