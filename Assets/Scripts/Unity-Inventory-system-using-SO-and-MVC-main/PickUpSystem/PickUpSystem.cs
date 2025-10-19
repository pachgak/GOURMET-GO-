using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpSystem : MonoBehaviour
{
    [SerializeField]
    private InventorySO inventoryData;
    public Action<Item, int> OnPickUpItem;

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    Item item = collision.GetComponent<Item>();
    //    if (item != null)
    //    {
    //        int reminder = inventoryData.AddItem(item.InventoryItem, item.Quantity);
    //        if (reminder == 0)
    //            item.DestroyItem();
    //        else
    //            item.Quantity = reminder;
    //    }
    //}

    public void PickItUp()
    {
        if (TryGetComponent(out Item item))
        {
            // เก็บจำนวนที่ต้องการเก็บ
            int initialQuantity = item.Quantity;
            int reminder = inventoryData.AddItem(item.itemSO, item.Quantity);
            int collectedQuantity = initialQuantity - reminder;

            if (reminder == 0)
                item.DestroyItem();
            else
                item.Quantity = reminder;
        }
    }
}
