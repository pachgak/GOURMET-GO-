using Inventory.UI;
using System.Collections.Generic;
using System;
using UnityEngine;

public class UISkilPage : MonoBehaviour
{
    //public UISkillBarItem itemPrefab;

    //[SerializeField]
    //private RectTransform contentPanel;

    //[SerializeField]
    //private UIInventoryDescription itemDescription;
    [SerializeField]
    private ItemDetailPromptController itemDescription;

    [SerializeField]
    private MouseFollowerSkillUI mouseFollower;

    [SerializeField]
    private DropItemZoneUI dropitemZone;

    List<UISkillBarItem> listOfUIItems = new List<UISkillBarItem>();

    [SerializeField]private int currentlyDraggedItemIndex = -1;

    private bool _canActiveSlotLoadSkill = false;

    public event Action<int>
            OnDescriptionRequested,
            OnItemActionRequested,
            OnItemPerformAction,
            OnStartDragging,
            OnPointEnterItem, OnPointExitItem;

    public event Action<int, int> OnSwapItems;
    public event Action<int> OnDropItems;

    [Header("_Manager References")]
    private OpenUiManager _uiManager;
    private bool _isUiOpening;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Hide();
        mouseFollower.Toggle(false);
        //itemDescription.ResetDescription();

        _uiManager = OpenUiManager.instance;
    }

    private void OnEnable()
    {
        _uiManager.OnUiOpeningStateChange += HandleUiOpeningStateChange;
    }
    private void OnDisable()
    {
        _uiManager.OnUiOpeningStateChange -= HandleUiOpeningStateChange;
    }

    internal void HandleUiOpeningStateChange(bool isUiOpeningState)
    {
        _isUiOpening = isUiOpeningState;
        _canActiveSlotLoadSkill = isUiOpeningState;

        ResetDraggedItem();
        CloseItemDetail();
    }

    public void InitializeInventoryUI(UISkillBarItem[] UISkillBarItems)
    {
        dropitemZone.OnItemDropped += HandleDropItem;

        for (int i = 0; i < UISkillBarItems.Length; i++)
        {
            //UISkillBarItem uiItem = 
            //Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
            //uiItem.transform.SetParent(contentPanel);
            //UISkillBarItem uiItem = Instantiate(itemPrefab, contentPanel);
            UISkillBarItem uiItem = UISkillBarItems[i];

            listOfUIItems.Add(uiItem);
            uiItem.OnItemClicked += HandleItemSelection;
            uiItem.OnItemBeginDrag += HandleBeginDrag;
            uiItem.OnItemDroppedOn += HandleSwap;
            uiItem.OnItemEndDrag += HandleEndDrag;
            //uiItem.OnRightMouseBtnClick += HandleItemPerformAction;
            uiItem.OnPointEnterItem += HandlePointEnterItem;
            uiItem.OnPointExitItem += HandlePointExitItem;
        }
    }



    public void OpenItemDetail()
    {
        itemDescription.Toggle(true);
    }

    public void CloseItemDetail()
    {
        itemDescription.Toggle(false);
    }

    internal void UpdateItemDetail(Sprite itemImage, string name, string description)
    {
        itemDescription.SetDescription(itemImage, name, description);
    }

    internal void ResetAllItems()
    {
        foreach (var item in listOfUIItems)
        {
            item.ResetData();
            item.Deselect();
        }
    }

    internal void UpdateDescription(int itemIndex, Sprite itemImage, string name, string description)
    {
        //itemDescription.SetDescription(itemImage, name, description);
        DeselectAllItems();
        listOfUIItems[itemIndex].Select();
    }

    public void UpdateData(int itemIndex,
        Sprite itemImage, int itemQuantity,Sprite typeSprite,float countdown,float maxCooldown)
    {
        if (listOfUIItems.Count > itemIndex)
        {
            listOfUIItems[itemIndex].SetData(itemImage, itemQuantity, typeSprite, countdown, maxCooldown);
        }
    }

    internal void UpdateCooldown(int itemIndex, float countdown)
    {
        listOfUIItems[itemIndex].CooldownUpdate(countdown);
    }

    //--------------------

    private void HandlePointEnterItem(UISkillBarItem skillItemUI)
    {
        if (!_canActiveSlotLoadSkill) return;
        if (currentlyDraggedItemIndex > -1) return;

        int index = listOfUIItems.IndexOf(skillItemUI);
        Debug.Log($"index : {index} , currentlyDraggedItemIndex : {currentlyDraggedItemIndex}");
        if (index == -1)
        {
            return;
        }

        OnPointEnterItem?.Invoke(index);
    }

    private void HandlePointEnterItem(UISkillBarItem skillItemUI, bool byPass)
    {
        if (!_canActiveSlotLoadSkill) return;
        if (!byPass) return;

        int index = listOfUIItems.IndexOf(skillItemUI);
        Debug.Log($"index : {index} , currentlyDraggedItemIndex : {currentlyDraggedItemIndex}");
        if (index == -1)
        {
            return;
        }

        OnPointEnterItem?.Invoke(index);
    }


    private void HandlePointExitItem(UISkillBarItem skillItemUI)
    {
        if (currentlyDraggedItemIndex > -1) return;

        int index = listOfUIItems.IndexOf(skillItemUI);
        if (index == -1)
        {
            return;
        }

        OnPointExitItem?.Invoke(index);
    }

    private void HandleShowItemActions(UISkillBarItem skillItemUI)
    {
        int index = listOfUIItems.IndexOf(skillItemUI);
        if (index == -1)
        {
            return;
        }
        OnItemActionRequested?.Invoke(index);
    }

    private void HandleItemPerformAction(UISkillBarItem skillItemUI)
    {
        int index = listOfUIItems.IndexOf(skillItemUI);
        if (index == -1)
        {
            return;
        }
        OnItemPerformAction?.Invoke(index);
    }
    private void HandleBeginDrag(UISkillBarItem skillItemUI)
    {
        if (!_canActiveSlotLoadSkill) return;

        int index = listOfUIItems.IndexOf(skillItemUI);
        if (index == -1)
            return;

        currentlyDraggedItemIndex = index;
        listOfUIItems[currentlyDraggedItemIndex].ShowCurrentlyDragged();
        HandleItemSelection(skillItemUI);
        OnStartDragging?.Invoke(index);
        CloseItemDetail();
    }

    private void HandleEndDrag(UISkillBarItem skillItemUI)
    {
        if (currentlyDraggedItemIndex > -1) listOfUIItems[currentlyDraggedItemIndex].DeShowCurrentlyDragged();
        ResetDraggedItem();
    }

    private void HandleSwap(UISkillBarItem skillItemUI)
    {
        if (!_canActiveSlotLoadSkill) return;

        int index = listOfUIItems.IndexOf(skillItemUI);
        if (index == -1)
        {
            return;
        }
        OnSwapItems?.Invoke(currentlyDraggedItemIndex, index);
        HandleItemSelection(skillItemUI);

        Debug.Log($"currentlyDraggedItemIndex : {currentlyDraggedItemIndex} || skillItemUI Index : {listOfUIItems.IndexOf(skillItemUI)}");
        HandlePointEnterItem(skillItemUI, true);
        Debug.Log($"End HandleEndDrad");
    }
    private void HandleItemSelection(UISkillBarItem skillItemUI)
    {
        if (!_canActiveSlotLoadSkill) return;

        int index = listOfUIItems.IndexOf(skillItemUI);
        if (index == -1)
            return;
        OnDescriptionRequested?.Invoke(index);
    }

    private void HandleDropItem()
    {
        OnDropItems?.Invoke(currentlyDraggedItemIndex);
    }

    private void ResetDraggedItem()
    {
        if (currentlyDraggedItemIndex <= -1) return; 
            
        listOfUIItems[currentlyDraggedItemIndex].DeShowCurrentlyDragged();
        
        mouseFollower.Toggle(false);
        currentlyDraggedItemIndex = -1;
    }

    public void CreateDraggedItem(Sprite skillSprite, int uesdCount, Sprite typeSprite, float countdown, float maxCooldown)
    {
        mouseFollower.Toggle(true);
        mouseFollower.SetData(skillSprite, uesdCount, typeSprite, countdown, maxCooldown);
    }


    public void Show()
    {
        //gameObject.SetActive(true);
        ResetSelection();
    }

    public void ResetSelection()
    {
        //itemDescription.ResetDescription();
        DeselectAllItems();
    }

    public void AddAction(string actionName, Action performAction)
    {
        //actionPanel.AddButon(actionName, performAction);
    }

    public void ShowItemAction(int itemIndex)
    {
        //actionPanel.Toggle(true);
        //actionPanel.transform.position = listOfUIItems[itemIndex].transform.position;
    }

    private void DeselectAllItems()
    {
        foreach (UISkillBarItem item in listOfUIItems)
        {
            item.Deselect();
        }
        //actionPanel.Toggle(false);
    }

    public void Hide()
    {
        //actionPanel.Toggle(false);
        //gameObject.SetActive(false);
        ResetDraggedItem();
        CloseItemDetail();
    }
}
