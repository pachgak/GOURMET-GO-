using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Inventory.Model;

public class MenuIndexManager : MonoBehaviour
{
    public static MenuIndexManager Instance { get; private set; }

    [Header("Settings")]
    public int itemsPerPage = 16;
    public int slotsItemIngredientsDetel = 4;

    [Header("Data")]
    public List<CookingRecipeSO> MenuIndexList;
    private List<CookingRecipeSO> _finishedMenus = new List<CookingRecipeSO>();

    [Header("UI - Controllers")]
    public newOpenUIController menuListUIController;
    public newOpenUIController menuDetailUIController;

    [Header("UI - MenuList (Page 1)")]
    public Button exitButton;
    public TMP_Text finishedCountText;
    public RectTransform contentMenuPageLeft;
    public RectTransform contentMenuPageRight;
    public MenuListItemUI menuListItemPrefab;

    [Header("UI - MenuDetail (Page 2)")]
    public Button backToMenuIndexButton;
    public Button nextMenuIndexButton;
    public Button backMenuIndexButton;
    public TMP_Text menuNameText;
    public Image menuDetailImage;
    public List<GameObject> detailLockIcons;
    public TMP_Text noIndexMenuText;
    public TMP_Text menuDescriptText;

    public RectTransform contentMenuIngredients;
    public IngredientsMenuItemUI ingredientsMenuPrefab;

    // *** ตัวแปรพระเอกของเรา ที่คอยจำว่าตอนนี้กำลังดูเมนูไหนอยู่ ***
    private int _currentDetailIndex = 0;

    private List<MenuListItemUI> _spawnedListItems = new List<MenuListItemUI>();
    private List<IngredientsMenuItemUI> _spawnedIngredientItems = new List<IngredientsMenuItemUI>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        exitButton.onClick.AddListener(CloseCurrentPanel);
        backToMenuIndexButton.onClick.AddListener(CloseCurrentPanel);
        nextMenuIndexButton.onClick.AddListener(GoToNextDetail);
        backMenuIndexButton.onClick.AddListener(GoToPrevDetail);

        for (int i = 0; i < slotsItemIngredientsDetel; i++)
        {
            IngredientsMenuItemUI item = Instantiate(ingredientsMenuPrefab, contentMenuIngredients);
            item.SetupEmpty();
            _spawnedIngredientItems.Add(item);
        }

        // *** สมัครรับ Event จาก UI Controller ตรงนี้ได้เลย ***
        if (menuListUIController != null) menuListUIController.OnPanelOpened.AddListener(GenerateMenuList);
        if (menuDetailUIController != null) menuDetailUIController.OnPanelOpened.AddListener(SetupDetailUI);

        // สมัครรับ Event เมื่อทำอาหารเสร็จ
        if (MiniGameUIManager.Instance != null)
        {
            MiniGameUIManager.Instance.OnCookingSuccess += UnlockMenu;
        }
    }

    private void OnDestroy()
    {
        if (MiniGameUIManager.Instance != null)
        {
            MiniGameUIManager.Instance.OnCookingSuccess -= UnlockMenu;
        }
    }

    public void UnlockMenu(CookingRecipeSO recipe)
    {
        if (!_finishedMenus.Contains(recipe))
        {
            _finishedMenus.Add(recipe);
            // ถ้าหน้า List เปิดอยู่ ให้รีเฟรชด้วย
            if (menuListUIController.uiPanel.activeSelf) GenerateMenuList();
        }
    }

    public bool IsMenuFinished(CookingRecipeSO recipe) => _finishedMenus.Contains(recipe);

    // ==========================================
    // 1. ระบบจัดการ List (หน้าแรก)
    // ==========================================

    private void GenerateMenuList()
    {
        foreach (var item in _spawnedListItems) Destroy(item.gameObject);
        _spawnedListItems.Clear();

        finishedCountText.text = $"Finished : {_finishedMenus.Count}/{MenuIndexList.Count}";

        int totalSlots = itemsPerPage * 2;
        for (int i = 0; i < totalSlots; i++)
        {
            RectTransform targetParent = (i < itemsPerPage) ? contentMenuPageLeft : contentMenuPageRight;
            MenuListItemUI newItem = Instantiate(menuListItemPrefab, targetParent);

            if (i < MenuIndexList.Count)
            {
                newItem.Setup(MenuIndexList[i], i, IsMenuFinished(MenuIndexList[i]));
            }
            else
            {
                newItem.Setup(null, i, false);
            }

            newItem.OnItemClicked += HandleOnItemClicked; // ผูก Event รอรับการคลิก
            _spawnedListItems.Add(newItem);
        }
    }

    // ==========================================
    // 2. ระบบจัดการ Detail (หน้าที่สอง)
    // ==========================================

    private void HandleOnItemClicked(int clickedIndex)
    {
        // ขั้นตอนที่ 1: จำค่า Index ที่ถูกกดไว้ก่อน!
        _currentDetailIndex = clickedIndex;

        // ขั้นตอนที่ 2: เช็คสถานะหน้าต่าง
        if (!menuDetailUIController.uiPanel.activeSelf && newOpenUIManager.instance != null)
        {
            // ถ้าหน้าต่าง Detail ปิดอยู่ -> ให้ Manager เปิดให้ 
            // (พอมันเปิดเสร็จ Event OnPanelOpened จะเด้งไปเรียก SetupDetailUI ให้อัตโนมัติ!)
            newOpenUIManager.instance._TogglePanel(menuDetailUIController);
        }
        else
        {
            // ถ้าหน้าต่าง Detail เปิดค้างไว้อยู่แล้ว (กรณีนี้อาจเกิดขึ้นได้ถ้าระบบซับซ้อนขึ้น)
            // ก็สั่งอัปเดต UI ทันที ไม่ต้องรอ Event
            SetupDetailUI();
        }
    }

    // ฟังก์ชันนี้ไม่มี Parameter แล้ว! ทำให้ใส่ใน Event OnPanelOpened ได้อย่างสมบูรณ์
    private void SetupDetailUI()
    {
        // ไปดึงค่า _currentDetailIndex ที่จำไว้มาใช้งาน
        CookingRecipeSO recipe = MenuIndexList[_currentDetailIndex];
        bool isFinished = IsMenuFinished(recipe);

        menuDetailImage.sprite = recipe.resultItem.ItemImage;
        foreach (var lockObj in detailLockIcons) lockObj.SetActive(!isFinished);
        //menuDetailImage.color = isFinished ? Color.white : Color.black;

        noIndexMenuText.text = $"No. {_currentDetailIndex + 1}/{MenuIndexList.Count}";

        if (isFinished)
        {
            menuNameText.text = recipe.resultItem.ItemName;
            menuDescriptText.text = recipe.resultItem.GetDescription();
        }
        else
        {
            menuNameText.text = recipe.resultItem.ItemName;
            menuDescriptText.text = recipe.resultItem.GetDescription();
        }

        for (int i = 0; i < slotsItemIngredientsDetel; i++)
        {
            if (i < recipe.ingredients.Count) _spawnedIngredientItems[i].Setup(recipe.ingredients[i].item.ItemImage);
            else _spawnedIngredientItems[i].SetupEmpty();
        }

        // *** เปลี่ยนตรงนี้: ให้ปุ่มกดได้เสมอ เพราะมันจะเปิดวนลูปทะลุได้แล้ว! ***
        backMenuIndexButton.interactable = true;
        nextMenuIndexButton.interactable = true;
    }

    // *** แก้ไขปุ่ม Next: ถ้าเกินช่องสุดท้าย ให้เด้งกลับไปหน้า 0 ***
    private void GoToNextDetail()
    {
        if (MenuIndexList.Count == 0) return; // กันบั๊กกรณีเผลอใส่ List ว่าง

        _currentDetailIndex++; // เลื่อนหน้าไป 1

        // ถ้าหน้าปัจจุบัน ทะลุเกินจำนวนเมนูทั้งหมด ให้กลับไปหน้าแรกสุด (Index 0)
        if (_currentDetailIndex >= MenuIndexList.Count)
        {
            _currentDetailIndex = 0;
        }

        SetupDetailUI();
    }

    // *** แก้ไขปุ่ม Prev: ถ้าถอยหลังทะลุหน้า 0 ให้เด้งไปหน้าสุดท้าย ***
    private void GoToPrevDetail()
    {
        if (MenuIndexList.Count == 0) return; // กันบั๊ก

        _currentDetailIndex--; // ถอยหลัง 1 หน้า

        // ถ้าถอยจนทะลุหน้าแรก (ติดลบ) ให้ไปหน้าสุดท้าย (Count - 1)
        if (_currentDetailIndex < 0)
        {
            _currentDetailIndex = MenuIndexList.Count - 1;
        }

        SetupDetailUI();
    }

    private void CloseCurrentPanel()
    {
        if (newOpenUIManager.instance != null)
        {
            newOpenUIManager.instance._CloseTopPanel();
        }
    }

    // ==========================================
    // สำหรับโหมด Debug (กดคลิกขวาที่ชื่อ Script ใน Inspector)
    // ==========================================
    [ContextMenu("Unlock All Menus (Debug)")]
    public void UnlockAllMenus()
    {
        if (MenuIndexList == null || MenuIndexList.Count == 0)
        {
            Debug.LogWarning("[MenuIndexManager] ไม่มีเมนูใน List ให้ปลดล็อค!");
            return;
        }

        int unlockCount = 0;
        foreach (var recipe in MenuIndexList)
        {
            if (!_finishedMenus.Contains(recipe))
            {
                _finishedMenus.Add(recipe);
                unlockCount++;
            }
        }

        Debug.Log($"<color=green>[MenuIndexManager] ปลดล็อคเมนูสำเร็จ {unlockCount} เมนู! (รวมทั้งหมด {MenuIndexList.Count} เมนู)</color>");

        // ป้องกัน Error หากเผลอไปกดตอนที่ไม่ได้รันเกม (Edit Mode)
        // ถ้ากดตอนรันเกมอยู่และหน้าจอเมนูเปิดอยู่ ให้มันอัปเดตภาพทันที
        if (Application.isPlaying && menuListUIController != null && menuListUIController.uiPanel != null && menuListUIController.uiPanel.activeSelf)
        {
            GenerateMenuList();
        }
    }
}