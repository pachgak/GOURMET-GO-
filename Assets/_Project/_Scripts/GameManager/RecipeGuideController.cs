using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;

namespace Inventory.UI
{
    public class RecipeGuideController : MonoBehaviour
    {
        [Header("Data References")]
        [SerializeField] private InventorySO playerInventory; // ใช้แค่กระเป๋าคนเล่นอย่างเดียวแล้ว
        [SerializeField] private List<CookingRecipeSO> allRecipes;

        [Header("UI References")]
        [SerializeField] private Transform contentPanel;
        [SerializeField] private RecipeEntryUI recipeEntryPrefab;
        [SerializeField] private ItemDetailPromptController detailPrompt;

        private List<RecipeEntryUI> _spawnedEntries = new List<RecipeEntryUI>();

        private void Start()
        {
            InitializeGuide();

            // ดักฟังแค่กระเป๋าคนเล่น
            playerInventory.OnInventoryUpdated += UpdateGuideDisplay;

            // อัปเดตครั้งแรกตอนเริ่ม
            UpdateGuideDisplay(playerInventory.GetCurrentInventoryState());
        }

        private void OnDestroy()
        {
            playerInventory.OnInventoryUpdated -= UpdateGuideDisplay;
        }

        private void InitializeGuide()
        {
            // ล้างของเก่า
            foreach (Transform child in contentPanel) Destroy(child.gameObject);
            _spawnedEntries.Clear();

            // สร้าง UI ของทุกสูตรเตรียมไว้
            foreach (var recipe in allRecipes)
            {
                RecipeEntryUI uiItem = Instantiate(recipeEntryPrefab, contentPanel);
                // ส่งแค่กระเป๋าผู้เล่นไปให้ UI
                uiItem.Setup(recipe, detailPrompt, playerInventory);
                _spawnedEntries.Add(uiItem);
            }
        }

        private void UpdateGuideDisplay(Dictionary<int, InventoryItem> state)
        {
            // โชว์สูตรทั้งหมด และสั่งให้แต่ละสูตรคำนวณของใหม่ (เผื่อผู้เล่นเพิ่งได้ของมา)
            for (int i = 0; i < allRecipes.Count; i++)
            {
                _spawnedEntries[i].Setup(allRecipes[i], detailPrompt, playerInventory);
                _spawnedEntries[i].SetVisibility(true); // โชว์ทั้งหมด ไม่ต้องซ่อนแล้ว
            }
        }
    }
}