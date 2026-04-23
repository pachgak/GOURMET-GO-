using UnityEngine;
using UnityEngine.UI;
using Inventory.Model;
using TMPro;

public class RecipeEntryUI : MonoBehaviour
{
    [Header("UI Images")]
    [SerializeField] private Image resultImage;
    [SerializeField] private Image[] ingredientSlots;

    [Header("UI Texts")]
    [SerializeField] private TMP_Text[] ingredientCountTexts;

    [Header("UI Triggers")]
    [SerializeField] private UIItemDetailTrigger resultTrigger;
    [SerializeField] private UIItemDetailTrigger[] ingredientTriggers;

    [Header("Settings")]
    [SerializeField] private Color sufficientColor = Color.green;
    [SerializeField] private Color insufficientColor = Color.red;

    [Header("Action")]
    [SerializeField] private Button cookButton; // **เพิ่มปุ่ม Cook เข้ามาตรงนี้**

    private CookingRecipeSO currentRecipe;
    private InventorySO currentPlayerInventory;

    public void Setup(CookingRecipeSO recipe, ItemDetailPromptController promptController, InventorySO playerInventory)
    {
        currentRecipe = recipe;
        currentPlayerInventory = playerInventory;

        // 1. จัดการภาพผลลัพธ์
        resultImage.sprite = recipe.resultItem.ItemImage;
        if (resultTrigger != null)
        {
            resultTrigger.SetItemData(recipe.resultItem);
            resultTrigger.SetPromptController(promptController);
        }

        // 2. จัดการวัตถุดิบ
        foreach (var slot in ingredientSlots) slot.gameObject.SetActive(false);
        foreach (var txt in ingredientCountTexts) txt.gameObject.SetActive(false);

        bool canCookThis = true; // ตัวแปรสำหรับเช็คว่าของครบไหม

        for (int i = 0; i < recipe.ingredients.Count; i++)
        {
            if (i < ingredientSlots.Length)
            {
                ingredientSlots[i].gameObject.SetActive(true);
                ingredientSlots[i].sprite = recipe.ingredients[i].item.ItemImage;

                if (i < ingredientTriggers.Length && ingredientTriggers[i] != null)
                {
                    ingredientTriggers[i].SetItemData(recipe.ingredients[i].item);
                    ingredientTriggers[i].SetPromptController(promptController);
                }

                if (i < ingredientCountTexts.Length && ingredientCountTexts[i] != null)
                {
                    ingredientCountTexts[i].gameObject.SetActive(true);

                    ItemSO itemNeeded = recipe.ingredients[i].item;
                    int needQuantity = recipe.ingredients[i].quantity;

                    // นับของเฉพาะในกระเป๋าคนเล่น
                    int countInBag = GetItemCountInInventory(playerInventory, itemNeeded.IDg);
                    ingredientCountTexts[i].text = $"{countInBag}/{needQuantity}";

                    if (countInBag >= needQuantity)
                    {
                        ingredientCountTexts[i].color = sufficientColor;
                    }
                    else
                    {
                        ingredientCountTexts[i].color = insufficientColor;
                        canCookThis = false; // ถ้าของชิ้นนี้ไม่พอ ให้จำไว้ว่าคราฟไม่ได้
                    }
                }
            }
        }

        // 3. จัดการปุ่ม Cook
        if (cookButton != null)
        {
            cookButton.interactable = canCookThis; // ถ้าของครบ ปุ่มจะกดได้ (ไม่เป็นสีเทา)
            cookButton.onClick.RemoveAllListeners();
            cookButton.onClick.AddListener(PerformCook); // ผูกฟังก์ชันคราฟต์
        }
    }

    public void SetVisibility(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    private void PerformCook()
    {
        if (currentRecipe == null || currentPlayerInventory == null) return;

        // *** เช็คพื้นที่ว่างก่อนเริ่มกระบวนการคราฟต์ ***
        if (!HasSpaceForCrafting(currentPlayerInventory, currentRecipe))
        {
            Debug.LogWarning("กระเป๋าเต็ม! ไม่สามารถคราฟได้ ป้องกันการเสียวัตถุดิบฟรี");
            // แจ้งเตือนผู้เล่น: ตรงนี้คุณสามารถเปลี่ยนเป็น Popup ของเกมคุณได้เลย
            return; // หยุดการทำงานทันที ไม่หักของ
        }

        // 1. วนลูปหักของออกจากกระเป๋าผู้เล่น (ทำเมื่อมั่นใจแล้วว่ามีที่เก็บ)
        foreach (var ingredient in currentRecipe.ingredients)
        {
            RemoveItemFromInventory(currentPlayerInventory, ingredient.item.IDg, ingredient.quantity);
        }

        // 2. เพิ่มอาหารสำเร็จรูปเข้ากระเป๋า
        currentPlayerInventory.AddItem(currentRecipe.resultItem, 1);

        Debug.Log($"Cooked {currentRecipe.resultItem.ItemName} successfully!");
    }

    // --- Helper Methods ---

    // ฟังก์ชันใหม่: เช็คว่ากระเป๋ามีที่ว่างรับผลลัพธ์หรือไม่
    private bool HasSpaceForCrafting(InventorySO inventory, CookingRecipeSO recipe)
    {
        // 1. ตรวจสอบว่ามี "ช่องว่างเปล่าๆ" เหลืออยู่บ้างไหม (ง่ายที่สุด)
        for (int i = 0; i < inventory.Size; i++)
        {
            if (inventory.GetItemAt(i).IsEmpty) return true;
        }

        // 2. กรณีที่กระเป๋าเต็มเปี๊ยะ (ไม่มีช่องว่างเลย)
        // ต้องตรวจสอบว่าการหักวัตถุดิบครั้งนี้ จะทำให้ช่องไหนสักช่อง "ว่าง" ลงหรือไม่
        foreach (var ingredient in recipe.ingredients)
        {
            int itemID = ingredient.item.IDg;
            int amountNeeded = ingredient.quantity;

            for (int i = 0; i < inventory.Size; i++)
            {
                InventoryItem itemInSlot = inventory.GetItemAt(i);
                if (!itemInSlot.IsEmpty && itemInSlot.item.IDg == itemID)
                {
                    // ถ้าของในช่องนี้ มีจำนวนน้อยกว่าหรือเท่ากับที่จะถูกใช้
                    // แปลว่าคราฟต์เสร็จ ช่องนี้จะกลายเป็นช่องว่าง! (มีที่ว่างให้อาหารใหม่ไปแทนที่)
                    if (itemInSlot.quantity <= amountNeeded)
                    {
                        return true;
                    }

                    amountNeeded -= itemInSlot.quantity;
                    if (amountNeeded <= 0) break; // หักครบตามจำนวนแล้ว ไม่ต้องเช็คช่องอื่นต่อ
                }
            }
        }

        // 3. (เสริม) ถ้าระบบของคุณอาหารสามารถดรอปซ้อน (Stack) กับที่มีอยู่แล้วได้
        // คุณต้องเช็คด้วยว่าในกระเป๋ามีอาหารชนิดนี้อยู่ และยังไม่เต็ม Stack หรือไม่ 
        // *ถ้าของเกมคุณอาหารซ้อนกันไม่ได้ (1 ช่อง 1 จาน) ก็ปล่อยผ่านให้คืนค่า false ได้เลยครับ

        return false;
    }

    // --- Helper Methods ---

    private int GetItemCountInInventory(InventorySO inventory, int itemID)
    {
        int count = 0;
        foreach (var item in inventory.GetCurrentInventoryState().Values)
        {
            if (!item.IsEmpty && item.item.IDg == itemID)
            {
                count += item.quantity;
            }
        }
        return count;
    }

    // ฟังก์ชันหักของ (ยกมาจากหม้อต้มของเก่าเลย)
    private void RemoveItemFromInventory(InventorySO inventory, int itemID, int amountToRemove)
    {
        for (int i = 0; i < inventory.Size; i++)
        {
            InventoryItem itemInSlot = inventory.GetItemAt(i);
            if (!itemInSlot.IsEmpty && itemInSlot.item.IDg == itemID)
            {
                if (itemInSlot.quantity >= amountToRemove)
                {
                    inventory.RemoveItem(i, amountToRemove);
                    return; // ลบครบแล้ว จบการทำงาน
                }
                else
                {
                    int amountInThisSlot = itemInSlot.quantity;
                    inventory.RemoveItem(i, amountInThisSlot);
                    amountToRemove -= amountInThisSlot; // ลบไม่พอ ไปหักกองถัดไป
                }
            }
        }
    }
}