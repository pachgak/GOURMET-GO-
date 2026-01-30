using System.Collections.Generic;
using System;
using UnityEngine;
using System.Drawing;

public class UILoadoutSkillPage : MonoBehaviour
{
    [SerializeField]
    private UILoadoutSkillItem itemPrefab;
    [SerializeField]
    private RectTransform contentPanel;

    [SerializeField]
    private ItemDetailPromptController itemDescription;

    [SerializeField]
    private MouseFollowerLoadoutSkillUI mouseFollower;

    //[SerializeField]
    //private DropItemZoneUI dropitemZone;

    List<UILoadoutSkillItem> listOfUIItems = new List<UILoadoutSkillItem>();

    [SerializeField] private int currentlyDraggedItemIndex = -1;

    public event Action<int>
            OnItemSelection,
            OnItemActionRequested,
            OnItemPerformAction,
            OnStartDragging,
            OnPointEnterItem, OnPointExitItem;

    public event Action<int, int> OnSwapItems;
    public event Action<int> OnDropItems;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Hide();
        mouseFollower.Toggle(false);
        //itemDescription.ResetDescription();
    }

    public int GetListOfUIItems()
    {
        return listOfUIItems.Count;
    }

    public void InitializeUI(int size)
    {
        //dropitemZone.OnItemDropped += HandleDropItem;

        for (int i = 0; i < size; i++)
        {
            //UISkillLoadoutItem uiItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
            //uiItem.transform.SetParent(contentPanel);
            //UILoadoutSkillItem uiItem = UISkillLoadoutItems[i];
            UILoadoutSkillItem uiItem = Instantiate(itemPrefab, contentPanel);

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


    private void HandlePointEnterItem(UILoadoutSkillItem skillItemUI)
    {
        if (currentlyDraggedItemIndex > -1) return;

        int index = listOfUIItems.IndexOf(skillItemUI);
        Debug.Log($"index : {index} , currentlyDraggedItemIndex : {currentlyDraggedItemIndex}");
        if (index == -1)
        {
            return;
        }

        OnPointEnterItem?.Invoke(index);
    }

    private void HandlePointEnterItem(UILoadoutSkillItem skillItemUI, bool byPass)
    {
        if (!byPass) return;

        int index = listOfUIItems.IndexOf(skillItemUI);
        Debug.Log($"index : {index} , currentlyDraggedItemIndex : {currentlyDraggedItemIndex}");
        if (index == -1)
        {
            return;
        }

        OnPointEnterItem?.Invoke(index);
    }


    private void HandlePointExitItem(UILoadoutSkillItem skillItemUI)
    {
        if (currentlyDraggedItemIndex > -1) return;

        int index = listOfUIItems.IndexOf(skillItemUI);
        if (index == -1)
        {
            return;
        }

        OnPointExitItem?.Invoke(index);
    }

    public void OpenItemDescription()
    {
        itemDescription.Toggle(true);
    }

    public void CloseItemDescription()
    {
        itemDescription.Toggle(false);
    }

    internal void UpdateItemDescription(Sprite itemImage, string name, string description)
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

    internal void UpdateSelect(int itemIndex)
    {
        DeselectAllItems();
        listOfUIItems[itemIndex].Select();
    }

    public void UpdateData(int itemIndex,
        Sprite itemImage, int itemExp)
    {
        if (listOfUIItems.Count > itemIndex)
        {
            listOfUIItems[itemIndex].SetData(itemImage, itemExp);
        }
    }

    internal void UpdateCooldown(int itemIndex, float countdown)
    {
        listOfUIItems[itemIndex].CooldownUpdate(countdown);
    }

    private void HandleShowItemActions(UILoadoutSkillItem skillItemUI)
    {
        int index = listOfUIItems.IndexOf(skillItemUI);
        if (index == -1)
        {
            return;
        }
        OnItemActionRequested?.Invoke(index);
    }

    private void HandleItemPerformAction(UILoadoutSkillItem skillItemUI)
    {
        int index = listOfUIItems.IndexOf(skillItemUI);
        if (index == -1)
        {
            return;
        }
        OnItemPerformAction?.Invoke(index);
    }
    private void HandleBeginDrag(UILoadoutSkillItem skillItemUI)
    {
        int index = listOfUIItems.IndexOf(skillItemUI);
        if (index == -1)
            return;

        currentlyDraggedItemIndex = index;
        listOfUIItems[currentlyDraggedItemIndex].ShowCurrentlyDragged();
        HandleItemSelection(skillItemUI);
        OnStartDragging?.Invoke(index);
        CloseItemDescription();
    }

    private void HandleEndDrag(UILoadoutSkillItem skillItemUI)
    {
        if (currentlyDraggedItemIndex > -1) listOfUIItems[currentlyDraggedItemIndex].DeShowCurrentlyDragged();
        ResetDraggedItem();
    }

    private void HandleSwap(UILoadoutSkillItem skillItemUI)
    {
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

    private void HandleDropItem()
    {
        OnDropItems?.Invoke(currentlyDraggedItemIndex);
    }

    private void ResetDraggedItem()
    {
        if (currentlyDraggedItemIndex > -1) listOfUIItems[currentlyDraggedItemIndex].DeShowCurrentlyDragged();
        mouseFollower.Toggle(false);
        currentlyDraggedItemIndex = -1;
    }

    public void CreateDraggedItem(Sprite skillSprite, int expPoint)
    {
        mouseFollower.Toggle(true);
        mouseFollower.SetData(skillSprite, expPoint);
    }

    private void HandleItemSelection(UILoadoutSkillItem skillItemUI)
    {
        int index = listOfUIItems.IndexOf(skillItemUI);
        if (index == -1)
            return;
        OnItemSelection?.Invoke(index);
    }

    private void HandleItemDescriptionRequested(UILoadoutSkillItem skillItemUI)
    {
        int index = listOfUIItems.IndexOf(skillItemUI);
        if (index == -1)
            return;
        OnItemSelection?.Invoke(index);
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
        foreach (UILoadoutSkillItem item in listOfUIItems)
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
        CloseItemDescription();
    }
}
