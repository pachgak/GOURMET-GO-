using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Inventory.Model;

public class RecipeMiniGameUIManager : MonoBehaviour
{
    public static RecipeMiniGameUIManager Instance;

    [Header("MiniGame UI Reference")]
    [Tooltip("ลากหน้าต่าง MiniGame (newOpenUIController) มาใส่ได้เลย ไม่ต้องพิมพ์ชื่อแล้ว!")]
    public newOpenUIController miniGamePanel;

    [Header("UI - Left Panel")]
    public Image toolIconImage;
    public Transform recipesContentPanel;
    public RecipesItemUI recipesItemPrefab;

    [Header("UI - Right Panel")]
    public Image detailRecipeIcon;
    public TMP_Text detailNameText;
    public TMP_Text detailDescText;
    public Transform requirementsContentPanel;
    public RequirementsItemUI requirementPrefab;
    public TMP_Text progressCountText;

    [Header("UI - Controls")]
    public TMP_Text cookCountText;
    public Button addCountBtn, removeCountBtn, cookButton;

    // --- State ---
    private CookingStation _currentStation;
    private CookingRecipeSO _selectedRecipe;
    private int _cookCount = 1;
    private List<RecipesItemUI> _spawnedItems = new List<RecipesItemUI>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

        private void Start()
    {
        addCountBtn.onClick.AddListener(() => ChangeCount(1));
        removeCountBtn.onClick.AddListener(() => ChangeCount(-1));
        cookButton.onClick.AddListener(OnCookBtnPressed);
    }

    // ฟังก์ชันหลักที่ Station จะเรียกใช้
    public void Open(CookingStation station)
    {
        _currentStation = station;
        _cookCount = 1;

        // 1. Update Tool Icon
        toolIconImage.sprite = station.toolIcon;

        // 2. สร้างรายการอาหารเฉพาะของ Station นี้
        RefreshRecipeList();

        // 3. เลือกอันแรกให้เป็น Default
        if (_spawnedItems.Count > 0) HandleRecipeSelected(_spawnedItems[0]);
    }

    private void RefreshRecipeList()
    {
        foreach (var item in _spawnedItems) Destroy(item.gameObject);
        _spawnedItems.Clear();

        foreach (var recipe in _currentStation.recipesForThisStation)
        {
            RecipesItemUI uiItem = Instantiate(recipesItemPrefab, recipesContentPanel);
            uiItem.Setup(recipe);

            // *** เพิ่มส่วนการเช็ควัตถุดิบสำหรับ 1 ที่ตรงนี้ ***
            bool canCookAtLeastOne = CheckIngredientForOneServing(recipe);
            uiItem.SetAvailability(canCookAtLeastOne);

            uiItem.OnItemClicked += HandleRecipeSelected;
            _spawnedItems.Add(uiItem);
        }
    }

    // Helper ใหม่สำหรับเช็คว่าทำได้อย่างน้อย 1 จานหรือไม่
    private bool CheckIngredientForOneServing(CookingRecipeSO recipe)
    {
        foreach (var req in recipe.ingredients)
        {
            // เช็คจำนวนรวมในทุก Inventory ที่เชื่อมต่ออยู่
            if (GetTotalItemCount(req.item.ID) < req.quantity)
            {
                return false; // ถ้ามีแค่อย่างเดียวที่ไม่พอ ก็คือทำไม่ได้
            }
        }
        return true; // วัตถุดิบครบทุกอย่างสำหรับ 1 ที่
    }

    private void HandleRecipeSelected(RecipesItemUI clicked)
    {
        foreach (var item in _spawnedItems) item.Deselect();
        clicked.Select();

        _selectedRecipe = clicked.RecipeData;
        _cookCount = 1;
        UpdateDetails();
    }

    private void ChangeCount(int amt)
    {
        _cookCount = Mathf.Clamp(_cookCount + amt, 1, 99);
        UpdateDetails();
    }

    private void UpdateDetails()
    {
        if (_selectedRecipe == null) return;

        detailRecipeIcon.sprite = _selectedRecipe.resultItem.ItemImage;
        detailNameText.text = _selectedRecipe.resultItem.ItemName;
        detailDescText.text = _selectedRecipe.resultItem.GetDescription();
        cookCountText.text = _cookCount.ToString();

        // เคลียร์และสร้าง Requirements
        foreach (Transform child in requirementsContentPanel) Destroy(child.gameObject);

        bool canCook = true;
        foreach (var req in _selectedRecipe.ingredients)
        {
            int needed = req.quantity * _cookCount;
            // เช็ครวมทุก Inventory (กระเป๋า + ตู้เย็น)
            int totalOwned = GetTotalItemCount(req.item.ID);

            RequirementsItemUI reqUI = Instantiate(requirementPrefab, requirementsContentPanel);
            reqUI.Setup(req.item.ItemImage, totalOwned, needed);

            if (totalOwned < needed) canCook = false;
        }

        // สูตร Progress 100% + (10% * จำนวนที่เพิ่ม)
        float targetProgress = 100f * (1f + (0.1f * (_cookCount - 1)));
        progressCountText.text = $"{Mathf.RoundToInt(targetProgress)}";

        cookButton.interactable = canCook;
    }

    private int GetTotalItemCount(int itemID)
    {
        int total = 0;
        foreach (var inv in _currentStation.inventoriesToCheck)
        {
            foreach (var slot in inv.GetCurrentInventoryState().Values)
            {
                if (!slot.IsEmpty && slot.item.ID == itemID) total += slot.quantity;
            }
        }
        return total;
    }

    private void OnCookBtnPressed()
    {
        // 1. ส่งข้อมูลให้ MiniGame
        float targetScore = 100f * (1f + (0.1f * (_cookCount - 1)));
        _currentStation.miniGameType.SetupFromRecipe(_selectedRecipe, Mathf.RoundToInt(targetScore), _cookCount);

        // 2. ปิดหน้าจอนี้ แล้วใช้ Wrapper Method ของคุณเปิดหน้ามินิเกม!
        if (newOpenUIManager.instance != null)
        {
            newOpenUIManager.instance._CloseTopPanel();

            if (miniGamePanel != null)
            {
                newOpenUIManager.instance._TogglePanel(miniGamePanel); // โคตรสะดวก!
            }
        }
    }
}