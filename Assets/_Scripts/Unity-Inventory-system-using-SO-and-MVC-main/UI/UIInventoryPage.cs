using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.UI
{
    public class UIInventoryPage : MonoBehaviour
    {
        [SerializeField]
        private UIInventoryItem itemPrefab;

        [SerializeField]
        private RectTransform contentPanel;

        //[SerializeField]
        //private UIInventoryDescription itemDescription;
        [SerializeField]
        private ItemDetailPromptController itemDescription;

        [SerializeField]
        private MouseFollower mouseFollower;

        [SerializeField]
        private DropItemZoneUI dropitemZone;

        List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem>();

        private int currentlyDraggedItemIndex = -1;

        public InventoryController Owner { get; set; } // เพิ่มบรรทัดนี้

        // 1. เพิ่ม Static Variable เพื่อจำว่าลากมาจากหน้าไหน
        public static UIInventoryPage DraggedSourcePage { get; private set; }
        public static int DraggedItemIndex { get; private set; } = -1;

        public event Action<int> 
                OnDescriptionRequested,
                OnItemActionRequested,
                OnItemPerformAction,
                OnStartDragging,
                OnPointEnterItem, OnPointExitItem;

        public event Action<int, int> OnSwapItems;
        public event Action<int> OnDropItems;
        public event Action<UIInventoryPage, int, int> OnItemTransferRequested;

        //[SerializeField]
        //private ItemActionPanel actionPanel;

        private void Awake()
        {
            Hide();
            mouseFollower.Toggle(false);
            //itemDescription.ResetDescription();
        }

        public void InitializeInventoryUI(int inventorysize)
        {
            dropitemZone.OnItemDropped += HandleDropItem;

            for (int i = 0; i < inventorysize; i++)
            {
                //UIInventoryItem uiItem = 
                //Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
                //uiItem.transform.SetParent(contentPanel);
                UIInventoryItem uiItem = Instantiate(itemPrefab, contentPanel);
                listOfUIItems.Add(uiItem);
                uiItem.OnItemClicked += HandleItemSelection;
                uiItem.OnItemBeginDrag += HandleBeginDrag;
                uiItem.OnItemDroppedOn += HandleSwap;
                uiItem.OnItemEndDrag += HandleEndDrag;
                uiItem.OnRightMouseBtnClick += HandleItemPerformAction;
                uiItem.OnPointEnterItem += HandlePointEnterItem;
                uiItem.OnPointExitItem += HandlePointExitItem;

            }
        }


        private void HandlePointEnterItem(UIInventoryItem inventoryItemUI)
        {
            if (currentlyDraggedItemIndex > -1) return;

            int index = listOfUIItems.IndexOf(inventoryItemUI);
            //Debug.Log($"index : {index} , currentlyDraggedItemIndex : {currentlyDraggedItemIndex}");
            if (index == -1)
            {
                return;
            }

            OnPointEnterItem?.Invoke(index);
        }

        private void HandlePointEnterItem(UIInventoryItem inventoryItemUI,bool byPass)
        {
            if (!byPass) return;

            int index = listOfUIItems.IndexOf(inventoryItemUI);
            //Debug.Log($"index : {index} , currentlyDraggedItemIndex : {currentlyDraggedItemIndex}");
            if (index == -1)
            {
                return;
            }

            OnPointEnterItem?.Invoke(index);
        }


        private void HandlePointExitItem(UIInventoryItem inventoryItemUI)
        {
            if (currentlyDraggedItemIndex > -1) return;

            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }

            OnPointExitItem?.Invoke(index);
        }

        public void OpenItemDetail()
        {
            itemDescription.Toggle(true);
        }

        public void CheckCloseItemDetail()
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
            Sprite itemImage, int itemQuantity)
        {
            if (listOfUIItems.Count > itemIndex)
            {
                listOfUIItems[itemIndex].SetData(itemImage, itemQuantity);
            }
        }

        private void HandleShowItemActions(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            OnItemActionRequested?.Invoke(index);
        }

        private void HandleItemPerformAction(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            OnItemPerformAction?.Invoke(index);
        }
        private void HandleBeginDrag(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
                return;

            currentlyDraggedItemIndex = index;

            // บันทึกข้อมูลลง Static Variable
            DraggedSourcePage = this;
            DraggedItemIndex = index;

            listOfUIItems[currentlyDraggedItemIndex].ShowCurrentlyDragged();
            HandleItemSelection(inventoryItemUI);
            OnStartDragging?.Invoke(index);
            CheckCloseItemDetail();
        }

        private void HandleEndDrag(UIInventoryItem inventoryItemUI)
        {
            if(currentlyDraggedItemIndex > -1) listOfUIItems[currentlyDraggedItemIndex].DeShowCurrentlyDragged();
            ResetDraggedItem();
        }

        private void HandleSwap(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1) return;

            if (DraggedSourcePage == this)
            {
                OnSwapItems?.Invoke(currentlyDraggedItemIndex, index);
                HandleItemSelection(inventoryItemUI);

                //Debug.Log($"currentlyDraggedItemIndex : {currentlyDraggedItemIndex} || inventoryItemUI Index : {listOfUIItems.IndexOf(inventoryItemUI)}");
                HandlePointEnterItem(inventoryItemUI, true);
                //Debug.Log($"End HandleEndDrad");
            }
            else
            {
                // ส่งคำสั่งไปที่ Controller ของหน้านี้ (Destination) ให้จัดการดึงของมา
                OnItemTransferRequested?.Invoke(DraggedSourcePage, DraggedItemIndex, index);
            }

            //// อย่าลืม Reset การลาก
            //HandleEndDrag(inventoryItemUI); // หรือ ResetDraggedItem() แล้วแต่การ Implement ของคุณ
        }

        private void HandleDropItem()
        {
            OnDropItems?.Invoke(currentlyDraggedItemIndex);
        }

        private void ResetDraggedItem()
        {
            if(currentlyDraggedItemIndex > -1) listOfUIItems[currentlyDraggedItemIndex].DeShowCurrentlyDragged();
            mouseFollower.Toggle(false);
            currentlyDraggedItemIndex = -1;

            // เคลียร์ค่า Static เมื่อจบการลาก
            DraggedSourcePage = null;
            DraggedItemIndex = -1;
        } 

        public void CreateDraggedItem(Sprite sprite, int quantity)
        {
            mouseFollower.Toggle(true);
            mouseFollower.SetData(sprite, quantity);
        }

        private void HandleItemSelection(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
                return;
            OnDescriptionRequested?.Invoke(index);
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
            foreach (UIInventoryItem item in listOfUIItems)
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
            CheckCloseItemDetail();
        }
    }
}