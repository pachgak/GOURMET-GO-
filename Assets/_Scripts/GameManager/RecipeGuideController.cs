using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;
using System.Linq;

namespace Inventory.UI
{
    public class RecipeGuideController : MonoBehaviour
    {
        [Header("Data References")]
        [SerializeField] private InventorySO cookingPotInventory;

        // *** เพิ่ม: อ้างอิงกระเป๋าผู้เล่นเพื่อเอาไปเช็คจำนวน ***
        [SerializeField] private InventorySO playerInventory;

        [SerializeField] private List<CookingRecipeSO> allRecipes;

        [Header("UI References")]
        [SerializeField] private Transform contentPanel;
        [SerializeField] private RecipeEntryUI recipeEntryPrefab;
        [SerializeField] private ItemDetailPromptController detailPrompt;

        private List<RecipeEntryUI> _spawnedEntries = new List<RecipeEntryUI>();
        private Dictionary<RecipeEntryUI, CookingRecipeSO> _entryMap = new Dictionary<RecipeEntryUI, CookingRecipeSO>();

        private void Start()
        {
            InitializeGuide();

            // ฟัง Event ทั้งหม้อ และ กระเป๋าผู้เล่น
            // (ต้องอัปเดตถ้ากระเป๋าผู้เล่นเปลี่ยนด้วย เพราะตัวเลขเขียว/แดงอาจเปลี่ยน)
            cookingPotInventory.OnInventoryUpdated += UpdateGuideDisplay;
            playerInventory.OnInventoryUpdated += UpdateGuideDisplay; // *** เพิ่มการฟัง Event นี้

            UpdateGuideDisplay(cookingPotInventory.GetCurrentInventoryState());
        }

        private void OnDestroy()
        {
            cookingPotInventory.OnInventoryUpdated -= UpdateGuideDisplay;
            playerInventory.OnInventoryUpdated -= UpdateGuideDisplay; // *** อย่าลืมเอาออก
        }

        private void InitializeGuide()
        {
            foreach (Transform child in contentPanel) Destroy(child.gameObject);
            _spawnedEntries.Clear();
            _entryMap.Clear();

            foreach (var recipe in allRecipes)
            {
                RecipeEntryUI uiItem = Instantiate(recipeEntryPrefab, contentPanel);

                // *** แก้ไข: ส่ง cookingPotInventory เข้าไปด้วย ***
                uiItem.Setup(recipe, detailPrompt, playerInventory, cookingPotInventory);

                _spawnedEntries.Add(uiItem);
                _entryMap.Add(uiItem, recipe);
            }
        }

        private void UpdateGuideDisplay(Dictionary<int, InventoryItem> potState)
        {
            HashSet<int> itemsInPotIDs = new HashSet<int>();
            foreach (var item in cookingPotInventory.GetCurrentInventoryState().Values)
            {
                if (!item.IsEmpty) itemsInPotIDs.Add(item.item.ID);
            }

            foreach (var entry in _spawnedEntries)
            {
                CookingRecipeSO recipe = _entryMap[entry];
                if (itemsInPotIDs.Count == 0)
                {
                    entry.SetVisibility(true);

                    // *** แก้ไข: ส่ง cookingPotInventory เข้าไปด้วย ***
                    entry.Setup(recipe, detailPrompt, playerInventory, cookingPotInventory);
                    continue;
                }

                bool isMatch = IsPotSubsetOfRecipe(itemsInPotIDs, recipe);
                entry.SetVisibility(isMatch);

                if (isMatch)
                {
                    // *** แก้ไข: ส่ง cookingPotInventory เข้าไปด้วย ***
                    entry.Setup(recipe, detailPrompt, playerInventory, cookingPotInventory);
                }
            }
        }

        // ... IsPotSubsetOfRecipe เหมือนเดิม ...
        private bool IsPotSubsetOfRecipe(HashSet<int> potItemIDs, CookingRecipeSO recipe)
        {
            HashSet<int> recipeIngredientIDs = new HashSet<int>();
            foreach (var ing in recipe.ingredients) recipeIngredientIDs.Add(ing.item.ID);
            foreach (int potID in potItemIDs) if (!recipeIngredientIDs.Contains(potID)) return false;
            return true;
        }
    }
}