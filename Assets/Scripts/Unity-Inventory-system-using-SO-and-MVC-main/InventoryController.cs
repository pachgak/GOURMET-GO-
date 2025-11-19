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

        // 3. ฟังก์ชันจัดการการย้ายไอเทมข้าม Inventory
        private void HandleItemTransferRequest(UIInventoryPage sourcePage, int sourceIndex, int targetIndex)
        {
            // ค้นหา InventoryController ที่คุม sourcePage อยู่
            // วิธีที่ง่ายที่สุดคือ GetComponent ใน GameObject ของ UI Page หรือเก็บ Reference ไว้
            // แต่ในที่นี้สมมติว่า InventoryController แปะอยู่คู่กับ UI หรือสามารถหาได้
            InventoryController sourceController = sourcePage.Owner;
            // หมายเหตุ: ถ้า Hierarchy ของคุณซับซ้อน อาจต้องใช้วิธีเก็บ Reference ใน UIInventoryPage ว่าใครเป็น Owner

            if (sourceController == null) return;

            InventorySO sourceInventory = sourceController.InventoryData;
            InventoryItem sourceItem = sourceInventory.GetItemAt(sourceIndex);

            if (sourceItem.IsEmpty) return;

            // Logic การย้ายของ:
            // พยายามเพิ่มของเข้า Inventory ของเรา (This/Target)
            // ฟังก์ชัน AddItem ของคุณคืนค่า int (จำนวนที่เหลือที่ add ไม่เข้า)
            // แต่อันนี้คุณกำลังจะวางลงใน Slot เฉพาะ (targetIndex)

            InventoryItem targetItem = inventoryData.GetItemAt(targetIndex);

            // กรณี 1: ย้ายไปทับช่องว่าง หรือ ไอเทมเดียวกัน (Stack)
            if (targetItem.IsEmpty || (targetItem.item.ID == sourceItem.item.ID && targetItem.item.IsStackable))
            {
                // เพิ่มของเข้าไปในช่องเป้าหมาย (Logic นี้คุณอาจต้องเพิ่มฟังก์ชัน AddAtIndex ใน InventorySO เพื่อความแม่นยำ หรือใช้ AddItem ธรรมดาแต่ระวังเรื่องตำแหน่ง)
                // เพื่อความง่าย ผมจะใช้ AddItem ธรรมดา แต่จริงๆ ควรเขียน Logic สลับของข้าม Inventory

                // **ตัวอย่าง Logic อย่างง่าย (Add ทั้งหมด)**:
                int reminder = inventoryData.AddItem(sourceItem.item, sourceItem.quantity);

                // คำนวณจำนวนที่ย้ายสำเร็จ
                int amountMoved = sourceItem.quantity - reminder;

                // ลบจำนวนที่ย้ายสำเร็จออกจาก Source
                if (amountMoved > 0)
                {
                    sourceInventory.RemoveItem(sourceIndex, amountMoved);
                }
            }
            // กรณี 2: สลับของ (Swap) ข้าม Inventory (ถ้าไอเทมต่างกัน)
            else
            {
                // ตรงนี้ซับซ้อนขึ้น เพราะต้อง Add ของ Source มาที่นี่ และ Add ของ Here ไปที่ Source
                // ต้องเขียน Logic เพิ่มใน InventorySO เพื่อรองรับการ Swap ข้าม Object
                // เบื้องต้น แนะนำให้ "ไม่ทำอะไร" หรือ return ไปก่อนถ้า Slot ไม่ว่างและไม่ใช่ของชนิดเดียวกัน
            }
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
                    canPerformAction = itemAction.PerformAction(gameObject);
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