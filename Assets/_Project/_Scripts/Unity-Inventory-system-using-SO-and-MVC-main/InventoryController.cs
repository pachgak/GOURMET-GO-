using Inventory.Model;
using Inventory.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

namespace Inventory
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField]
        private UIInventoryPage inventoryUI;

        [SerializeField]
        private InventorySO inventoryData;

        public List<InventoryItem> initialItems = new List<InventoryItem>();

        public bool isNotDeletItem = true;
        public Item itemDropPrefab;
        [SerializeField]
        private AudioClip dropClip;

        [SerializeField]
        private AudioSource audioSource;

        private InventoryManager _inventoryManager;

        // 1. เปิด Property ให้ Controller อื่นเข้าถึง Data ได้
        public InventorySO InventoryData => inventoryData;

        private void Awake()
        {
            _inventoryManager = InventoryManager.instance;
        }

        private void OnEnable()
        {
            _inventoryManager.OnOpenInventoryStateChange += HandleOpenInventoryStateChange;
        }


        private void OnDisable()
        {
            _inventoryManager.OnOpenInventoryStateChange -= HandleOpenInventoryStateChange;

            inventoryData.OnInventoryUpdated -= UpdateInventoryUI;
        }

        internal void HandleOpenInventoryStateChange(bool obj)
        {
            if (obj)
            {
                inventoryUI.Show();
                foreach (var item in inventoryData.GetCurrentInventoryState())
                {
                    inventoryUI.UpdateData(item.Key,
                        item.Value.item.ItemImage,
                        item.Value.quantity);
                }
            }
            else
            {
                inventoryUI.Hide();
            }
        }

        private void Start()
        {
            PrepareUI();
            PrepareInventoryData();
        }

        private void PrepareInventoryData()
        {
            inventoryData.Initialize();
            inventoryData.OnInventoryUpdated += UpdateInventoryUI;

            foreach (InventoryItem item in initialItems)
            {
                if (item.IsEmpty)
                    continue;
                inventoryData.AddItem(item);
            }
        }

        private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
        {

            inventoryUI.ResetAllItems();
            foreach (var item in inventoryState)
            {

                inventoryUI.UpdateData(item.Key, item.Value.item.ItemImage,
                item.Value.quantity);
            }
        }

        private void PrepareUI()
        {
            inventoryUI.Owner = this; // Assign ตัวเองว่าเป็นเจ้าของ

            inventoryUI.InitializeInventoryUI(inventoryData.Size);
            inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
            inventoryUI.OnSwapItems += HandleSwapItems;
            inventoryUI.OnStartDragging += HandleDragging;
            inventoryUI.OnItemActionRequested += HandleItemActionRequest;
            inventoryUI.OnItemPerformAction += HandleItemPerformAction;
            inventoryUI.OnPointEnterItem += HandlePointEnterItem;
            inventoryUI.OnPointExitItem += HandlePointExitItem;

            inventoryUI.OnDropItems += HandleDropItem;

            // 2. Subscribe Event ใหม่
            inventoryUI.OnItemTransferRequested += HandleItemTransferRequest;
        }
        private void HandleItemTransferRequest(UIInventoryPage sourcePage, int sourceIndex, int targetIndex)
        {
            // 1. หา Controller เจ้าของ Source Page
            InventoryController sourceController = sourcePage.Owner; // ตรวจสอบว่าคุณเพิ่ม Owner ใน UIInventoryPage ตามคำแนะนำรอบก่อนหรือยัง
            if (sourceController == null) return;

            InventorySO sourceInventory = sourceController.InventoryData;
            InventorySO targetInventory = this.inventoryData; // กระเป๋าของ Controller นี้คือ Target

            // 2. เรียกใช้ฟังก์ชันย้ายข้ามกระเป๋าที่เราเพิ่งเขียน
            // สั่งให้ Source เป็นคนเริ่มย้ายของตัวเอง ไปยัง Target
            sourceInventory.MoveItemTo(targetInventory, sourceIndex, targetIndex);
        }

        private void HandlePointEnterItem(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
            {
                return;
            }
            ItemSO item = inventoryItem.item;
            inventoryUI.OpenItemDetail();
            inventoryUI.UpdateItemDetail(item.ItemImage, item.name, item.Description);
        }

        private void HandlePointExitItem(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
            {
                return;
            }

            inventoryUI.CheckCloseItemDetail();
        }

        private void DropItem(int itemIndex, int quantity)
        {
            inventoryData.RemoveItem(itemIndex, quantity);
            inventoryUI.ResetSelection();
            audioSource.PlayOneShot(dropClip);
        }

        private void HandleItemPerformAction(int itemIndex)
        {
            PerformAction(itemIndex);

            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty) inventoryUI.CheckCloseItemDetail();
        }

        public void PerformAction(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
                return;

            bool canPerformAction = false;
            IItemAction itemAction = inventoryItem.item as IItemAction;

            if (itemAction != null)
            {
                canPerformAction = itemAction.PerformAction(gameObject);
            }

            if (canPerformAction)
            {
                if (itemAction != null)
                {
                    //canPerformAction = itemAction.PerformAction(gameObject);
                    if (itemAction.actionSFX != null) audioSource.PlayOneShot(itemAction.actionSFX);
                    if (inventoryData.GetItemAt(itemIndex).IsEmpty)
                        inventoryUI.ResetSelection();
                }

                IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
                if (destroyableItem != null)
                {
                    inventoryData.RemoveItem(itemIndex, 1);
                }
            }

        }

        private void HandleDragging(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
                return;
            inventoryUI.CreateDraggedItem(inventoryItem.item.ItemImage, inventoryItem.quantity);
        }

        private void HandleSwapItems(int itemIndex_1, int itemIndex_2)
        {
            if (itemIndex_1 <= -1) return;
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex_1);
            if (inventoryItem.IsEmpty)
                return;
            inventoryData.SwapItems(itemIndex_1, itemIndex_2);
        }

        private void HandleDropItem(int idex)
        {
            if (idex <= -1) return;
            InventoryItem inventoryItem = inventoryData.GetItemAt(idex);
            if (inventoryItem.IsEmpty)
                return;

            if (isNotDeletItem)
            {
                GameObject itemDrop_clone = ObjectPoolingManager.Instance.Spawn(itemDropPrefab.gameObject, transform.position + Vector3.up);
                if (itemDrop_clone.TryGetComponent(out Item item))
                {
                    item.Setup(inventoryItem.item, inventoryItem.quantity);
                }
            }

            inventoryData.ResetItem(idex);
        }

        private void HandleDescriptionRequest(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
            {
                inventoryUI.ResetSelection();
                return;
            }
            ItemSO item = inventoryItem.item;
            string description = PrepareDescription(inventoryItem);
            inventoryUI.UpdateDescription(itemIndex, item.ItemImage,
                item.name, description);
        }

        private string PrepareDescription(InventoryItem inventoryItem)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(inventoryItem.item.Description);
            sb.AppendLine();
            //for (int i = 0; i < inventoryItem.itemParameter.Count; i++)
            //{
            //    sb.Append($"{inventoryItem.itemParameter[i].itemParameterSO.ParameterName} " +
            //        $": {inventoryItem.itemParameter[i].value} / " +
            //        $"{inventoryItem.item.DefaultParametersList[i].value}");
            //    sb.AppendLine();
            //}
            return sb.ToString();
        }

        //public void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.I))
        //    {
        //        if (inventoryUI.isActiveAndEnabled == false)
        //        {
        //            inventoryUI.Show();
        //            foreach (var item in inventoryData.GetCurrentInventoryState())
        //            {
        //                inventoryUI.UpdateData(item.Key,
        //                    item.Value.item.ItemImage,
        //                    item.Value.quantity);
        //            }
        //        }
        //        else
        //        {
        //            inventoryUI.Hide();
        //        }

        //    }
        //}

        private void HandleItemActionRequest(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
                return;

            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null)
            {

                inventoryUI.ShowItemAction(itemIndex);
                inventoryUI.AddAction(itemAction.ActionName, () => PerformAction(itemIndex));
            }

            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventoryUI.AddAction("Drop", () => DropItem(itemIndex, inventoryItem.quantity));
            }

        }

    }
}