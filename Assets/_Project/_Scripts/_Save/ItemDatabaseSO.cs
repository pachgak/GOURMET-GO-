using UnityEngine;
using System.Collections.Generic;
using com.cyborgAssets.inspectorButtonPro;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/ItemDatabase")]
    public class ItemDatabaseSO : ScriptableObject
    {
        public List<ItemSO> allItems = new List<ItemSO>();

        public ItemSO GetItemByID(string id)
        {
            // เปลี่ยนจาก item.name เป็น item.ID ครับ
            return allItems.Find(item => item != null && item.ID == id);
        }

        [ProButton]
        [ContextMenu("TestButton")]
        public void TestButton()
        {
            Debug.Log($"TestButton : {this.name}");
        }
    }
}