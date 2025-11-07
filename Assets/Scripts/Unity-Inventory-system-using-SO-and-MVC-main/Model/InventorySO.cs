using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "New InventorySO", menuName = "Inventory/InventorySO")]
    public class InventorySO : ScriptableObject
    {
        [SerializeField]
        private List<InventoryItem> inventoryItems;

        [field: SerializeField]
        public int Size { get; private set; } = 10;

        public event Action<Dictionary<int, InventoryItem>> OnInventoryUpdated;

        public event Action<ItemSO,int> OnAddItem;
        public event Action<ItemSO,int> OnRemoveItem;
        public void Initialize()
        {
            inventoryItems = new List<InventoryItem>();
            for (int i = 0; i < Size; i++)
            {
                inventoryItems.Add(InventoryItem.GetEmptyItem());
            }
        }

        public void AddItem(InventoryItem item)
        {
            AddItem(item.item, item.quantity);
        }

        public int AddItem(ItemSO item, int quantity)
        {
            int initialQuantity = quantity;
            int collectedQuantity = 0;

            if (item.IsStackable == false)
            {
                for (int i = 0; i < inventoryItems.Count; i++)
                {
                    while(quantity > 0 && IsInventoryFull() == false)
                    {
                        quantity -= AddItemToFirstFreeSlot(item, 1);
                    }
                    InformAboutChange();

                    collectedQuantity = initialQuantity - quantity;
                    OnAddItem?.Invoke(item, collectedQuantity);
                    return quantity;
                }
            }
            quantity = AddStackableItem(item, quantity);
            InformAboutChange();

            collectedQuantity = initialQuantity - quantity;
            OnAddItem?.Invoke(item, collectedQuantity);
            return quantity;
        }

        private int AddItemToFirstFreeSlot(ItemSO item, int quantity
        //, List<ItemParameter> itemParameter = null
        )

        {
            InventoryItem newItem = new InventoryItem
            {
                item = item,
                quantity = quantity,
//                itemParameter = 
//                new List<ItemParameter>(itemParameter
// == null ? item.DefaultParametersList : itemParameter
//)
            };

            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty)
                {
                    inventoryItems[i] = newItem;
                    return quantity;
                }
            }
            return 0;
        }

        private bool IsInventoryFull()
            => inventoryItems.Where(item => item.IsEmpty).Any() == false;

        private int AddStackableItem(ItemSO item, int quantity)
        {
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty)
                    continue;
                if(inventoryItems[i].item.ID == item.ID)
                {
                    int amountPossibleToTake =
                        inventoryItems[i].item.MaxStackSize - inventoryItems[i].quantity;

                    if (quantity > amountPossibleToTake)
                    {
                        inventoryItems[i] = inventoryItems[i]
                            .ChangeQuantity(inventoryItems[i].item.MaxStackSize);
                        quantity -= amountPossibleToTake;
                    }
                    else
                    {
                        inventoryItems[i] = inventoryItems[i]
                            .ChangeQuantity(inventoryItems[i].quantity + quantity);
                        InformAboutChange();
                        return 0;
                    }
                }
            }
            while(quantity > 0 && IsInventoryFull() == false)
            {
                int newQuantity = Mathf.Clamp(quantity, 0, item.MaxStackSize);
                quantity -= newQuantity;
                AddItemToFirstFreeSlot(item, newQuantity);
            }
            return quantity;
        }

        public void RemoveItem(int itemIndex, int amount)
        {
            if (inventoryItems.Count > itemIndex)
            {
                OnRemoveItem?.Invoke(inventoryItems[itemIndex].item, amount);

                if (inventoryItems[itemIndex].IsEmpty)
                    return;
                int reminder = inventoryItems[itemIndex].quantity - amount;
                if (reminder <= 0)
                    inventoryItems[itemIndex] = InventoryItem.GetEmptyItem();
                else
                    inventoryItems[itemIndex] = inventoryItems[itemIndex]
                    .ChangeQuantity(reminder);

                //int collectedQuantity = inventoryItems[itemIndex].quantity - Mathf.Max(0, reminder);
                //Debug.Log($"reminder:{reminder} | itemQuantity {inventoryItems[itemIndex].quantity} : | -amount : {amount} | collectedQuantity : {collectedQuantity}");
                
                InformAboutChange();
            }
        }

        public void ResetItem(int itemIndex)
        {
            OnRemoveItem?.Invoke(inventoryItems[itemIndex].item, inventoryItems[itemIndex].quantity);

            if (inventoryItems[itemIndex].IsEmpty)
                return;

            inventoryItems[itemIndex] = InventoryItem.GetEmptyItem();

            InformAboutChange();
        }

        public Dictionary<int, InventoryItem> GetCurrentInventoryState()
        {
            Dictionary<int, InventoryItem> returnValue =
                new Dictionary<int, InventoryItem>();

            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty)
                    continue;
                returnValue[i] = inventoryItems[i];
            }
            return returnValue;
        }

        public InventoryItem GetItemAt(int itemIndex)
        {
            return inventoryItems[itemIndex];
        }

        public void SwapItems(int itemIndex_1, int itemIndex_2)
        {
            InventoryItem item1 = inventoryItems[itemIndex_1];
            inventoryItems[itemIndex_1] = inventoryItems[itemIndex_2];
            inventoryItems[itemIndex_2] = item1;
            InformAboutChange();
        }

        private void InformAboutChange()
        {
            OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
        }
    }

    [Serializable]
    public struct InventoryItem
    {
        public int quantity;
        public ItemSO item;
        //[SerializeField] public List<ItemParameter> itemParameter;
        public bool IsEmpty => item == null;

        public InventoryItem ChangeQuantity(int newQuantity)
        {
            return new InventoryItem
            {
                item = this.item,
                quantity = newQuantity,
                //itemParameter = new List<ItemParameter>(this.itemParameter)
            };
        }

        public static InventoryItem GetEmptyItem()
            => new InventoryItem
            {
                item = null,
                quantity = 0,
                //itemParameter = new List<ItemParameter>()
            };
    }

}
