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
            // ค้นหาไอเทมจาก ID ที่ตั้งไว้ (ในตัวอย่างก่อนหน้าที่ให้เพิ่ม string ID ใน ItemSO)
            return allItems.Find(item => item.name == id);
        }

        [ProButton]
        [ContextMenu("TestButton")]
        public void TestButton()
        {
            Debug.Log($"TestButton : {this.name}");
        }
    }
}