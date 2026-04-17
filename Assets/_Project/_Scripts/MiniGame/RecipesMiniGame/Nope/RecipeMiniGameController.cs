using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Inventory.Model;

public class RecipeMiniGameController : MonoBehaviour
{
    [Header("Data References")]
    [SerializeField] private InventorySO playerInventory;
    [SerializeField] private List<CookingRecipeSO> availableRecipes; // สูตรที่มีในสถานีนี้ (เช่น เฉพาะของต้ม)
    [SerializeField] private MiniGameBase targetMiniGame; // เกมที่จะให้เล่น (เช่น เกมต้ม, เกมทอด)
    [SerializeField] private Sprite toolIconSprite; //

    [Header("UI - Left Panel (Selection)")]
    [SerializeField] private Image toolIconImage;
    [SerializeField] private Transform recipesContentPanel;
    [SerializeField] private RecipesItemUI recipesItemPrefab;

    [Header("UI - Right Panel (Details)")]
    [SerializeField] private Image detailRecipeIcon;
    [SerializeField] private TMP_Text detailNameText;
    [SerializeField] private TMP_Text detailDescText;
    [SerializeField] private Transform requirementsContentPanel;
    [SerializeField] private RequirementsItemUI requirementPrefab;
    [SerializeField] private TMP_Text progressCountText; // โชว์เป้าหมายคะแนนมินิเกม

    [Header("UI - Cook Controls")]
    [SerializeField] private TMP_Text cookCountText;
    [SerializeField] private Button addCountBtn;
    [SerializeField] private Button removeCountBtn;
    [SerializeField] private Button cookButton;

    // --- State Variables ---
    private List<RecipesItemUI> _spawnedRecipeItems = new List<RecipesItemUI>();
    private CookingRecipeSO _currentSelectedRecipe;
    private int _currentCookCount = 1;

    // ตั้งค่า Base Progress ขั้นต่ำ สมมติว่าแต่ละเมนูอาจจะมีค่านี้ใน SO แต่ถ้าไม่มีตั้งตรงนี้ได้เลย
    private float _baseProgressNeeded = 100f;

    private void Start()
    {
        InitializeRecipeList();

        // สมัคร Event ปุ่มเพิ่มลดจำนวน
        addCountBtn.onClick.AddListener(() => ChangeCookCount(1));
        removeCountBtn.onClick.AddListener(() => ChangeCookCount(-1));

        // ปุ่ม Cook! 
        cookButton.onClick.AddListener(StartCookingMiniGame);
    }

    private void OnEnable()
    {
        // อัปเดตรายละเอียดใหม่เผื่อเพิ่งไปเก็บของมา
        if (_currentSelectedRecipe != null) UpdateDetailPanel(_currentSelectedRecipe);
    }

    private void InitializeRecipeList()
    {
        toolIconImage.sprite = toolIconSprite;

        // ล้างของเก่า
        foreach (Transform child in recipesContentPanel) Destroy(child.gameObject);
        _spawnedRecipeItems.Clear();

        // สร้าง Item ตามสูตรที่มี
        foreach (var recipe in availableRecipes)
        {
            RecipesItemUI uiItem = Instantiate(recipesItemPrefab, recipesContentPanel);

            bool isFinished = MenuIndexManager.Instance.IsMenuFinished(recipe);

            // *** ส่งผลลัพธ์ไปให้ UI วาดรูป ***
            uiItem.Setup(recipe, isFinished);

            uiItem.OnItemClicked += HandleRecipeSelected;
            _spawnedRecipeItems.Add(uiItem);
        }

        // เลือกอันแรกเป็นค่าเริ่มต้น (ถ้ามีสูตร)
        if (_spawnedRecipeItems.Count > 0)
            HandleRecipeSelected(_spawnedRecipeItems[0]);
    }

    private void HandleRecipeSelected(RecipesItemUI clickedItem)
    {
        // รีเซ็ตกรอบ UI ของอันเก่า
        foreach (var item in _spawnedRecipeItems) item.Deselect();
        clickedItem.Select(); // ใส่กรอบไฮไลต์อันที่เพิ่งกด

        _currentSelectedRecipe = clickedItem.RecipeData;
        _currentCookCount = 1; // เปลี่ยนเมนูใหม่ รีเซ็ตจำนวนกลับเป็น 1

        UpdateDetailPanel(_currentSelectedRecipe);
    }

    private void ChangeCookCount(int amount)
    {
        if (_currentSelectedRecipe == null) return;

        _currentCookCount += amount;
        if (_currentCookCount < 1) _currentCookCount = 1;
        if (_currentCookCount > 99) _currentCookCount = 99; // กันผู้เล่นกดเกิน

        UpdateDetailPanel(_currentSelectedRecipe);
    }

    private void UpdateDetailPanel(CookingRecipeSO recipe)
    {
        // 1. อัปเดตข้อมูลพื้นฐาน
        detailRecipeIcon.sprite = recipe.resultItem.ItemImage;
        detailNameText.text = recipe.resultItem.ItemName;
        detailDescText.text = recipe.resultItem.Description;
        cookCountText.text = _currentCookCount.ToString();

        // 2. อัปเดตวัตถุดิบ และเช็คว่าของพอไหม
        foreach (Transform child in requirementsContentPanel) Destroy(child.gameObject);

        bool canCook = true;

        foreach (var ingredient in recipe.ingredients)
        {
            int totalNeed = ingredient.quantity * _currentCookCount; // คูณจำนวนชิ้น
            int countInBag = GetItemCountInInventory(playerInventory, ingredient.item.ID);

            RequirementsItemUI reqUI = Instantiate(requirementPrefab, requirementsContentPanel);
            reqUI.Setup(ingredient.item.ItemImage, countInBag, totalNeed);

            if (countInBag < totalNeed) canCook = false; // ของไม่พอชิ้นใดชิ้นหนึ่ง ปุ่ม Cook จะโดนล็อค
        }

        // 3. คำนวณ Progress (Base + 10% ต่อชิ้นที่เกินมา)
        // สมมติทำ 1 ชิ้น = 100, 2 ชิ้น = 110, 3 ชิ้น = 120
        float targetProgress = _baseProgressNeeded * (1f + (0.1f * (_currentCookCount - 1)));
        progressCountText.text = $"Target Score: {Mathf.RoundToInt(targetProgress)}";

        // 4. เปิด/ปิด ปุ่ม Cook
        cookButton.interactable = canCook;
    }

    private void StartCookingMiniGame()
    {
        if (_currentSelectedRecipe == null || targetMiniGame == null) return;

        // คำนวณ Score ที่ต้องทำได้
        int finalTargetScore = Mathf.RoundToInt(_baseProgressNeeded * (1f + (0.1f * (_currentCookCount - 1))));

        // ส่งข้อมูลให้ MiniGameBase
        targetMiniGame.SetupFromRecipe(_currentSelectedRecipe, finalTargetScore, _currentCookCount);

        // ให้ Manager เปิดหน้าต่างเกมขึ้นมาทับหน้านี้
        if (MiniGameUIManager.Instance != null)
        {
            MiniGameUIManager.Instance.OpenMiniGame(targetMiniGame);

            // *ถ้าต้องการปิดหน้าต่างเลือกเมนูนี้ไปเลย ให้เรียก newOpenUIManager._CloseTopPanel() ตรงนี้ได้ครับ
            if (newOpenUIManager.instance != null) newOpenUIManager.instance._CloseTopPanel();
        }
    }

    // Helper เช็คของในกระเป๋า
    private int GetItemCountInInventory(InventorySO inventory, int itemID)
    {
        int count = 0;
        foreach (var item in inventory.GetCurrentInventoryState().Values)
        {
            if (!item.IsEmpty && item.item.ID == itemID) count += item.quantity;
        }
        return count;
    }
}