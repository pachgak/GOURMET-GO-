using UnityEngine;
using UnityEngine.UI;
using Inventory.Model;
using TMPro;

public class RecipeEntryUI : MonoBehaviour
{
    // ... (References ส่วน Header เหมือนเดิม) ...
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

    // *** แก้ไข: เพิ่ม Parameter potInventory เข้ามารับ ***
    public void Setup(CookingRecipeSO recipe, ItemDetailPromptController promptController, InventorySO playerInventory, InventorySO potInventory)
    {
        // 1. จัดการ Result Image (เหมือนเดิม)
        resultImage.sprite = recipe.resultItem.ItemImage;
        if (resultTrigger != null)
        {
            resultTrigger.SetItemData(recipe.resultItem);
            resultTrigger.SetPromptController(promptController);
        }

        // 2. จัดการ Ingredient Slots
        foreach (var slot in ingredientSlots) slot.gameObject.SetActive(false);
        foreach (var txt in ingredientCountTexts) txt.gameObject.SetActive(false);

        for (int i = 0; i < recipe.ingredients.Count; i++)
        {
            if (i < ingredientSlots.Length)
            {
                // ... (ส่วน Image & Trigger เหมือนเดิม) ...
                ingredientSlots[i].gameObject.SetActive(true);
                ingredientSlots[i].sprite = recipe.ingredients[i].item.ItemImage;

                if (i < ingredientTriggers.Length && ingredientTriggers[i] != null)
                {
                    ingredientTriggers[i].SetItemData(recipe.ingredients[i].item);
                    ingredientTriggers[i].SetPromptController(promptController);
                }

                // --- ส่วนจัดการ Text จำนวน (ที่แก้ไข) ---
                if (i < ingredientCountTexts.Length && ingredientCountTexts[i] != null)
                {
                    ingredientCountTexts[i].gameObject.SetActive(true);

                    ItemSO itemNeeded = recipe.ingredients[i].item;
                    int needQuantity = recipe.ingredients[i].quantity;

                    // 1. นับของในกระเป๋าตัว
                    int countInBag = GetItemCountInInventory(playerInventory, itemNeeded.ID);
                    // 2. นับของในหม้อ
                    int countInPot = GetItemCountInInventory(potInventory, itemNeeded.ID);

                    // 3. รวมกัน
                    int totalHas = countInBag + countInPot;

                    // แสดงผล
                    ingredientCountTexts[i].text = $"{totalHas}/{needQuantity}";

                    // เช็คเงื่อนไข (ใช้ยอดรวมในการเช็ค)
                    if (totalHas >= needQuantity)
                    {
                        ingredientCountTexts[i].color = sufficientColor;
                    }
                    else
                    {
                        ingredientCountTexts[i].color = insufficientColor;
                    }
                }
            }
        }
    }

    public void SetVisibility(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    // Helper นับของ
    private int GetItemCountInInventory(InventorySO inventory, int itemID)
    {
        int count = 0;
        foreach (var item in inventory.GetCurrentInventoryState().Values)
        {
            if (!item.IsEmpty && item.item.ID == itemID)
            {
                count += item.quantity;
            }
        }
        return count;
    }
}