using UnityEngine;
using UnityEditor;
using Inventory.Model;

[CustomEditor(typeof(ItemDatabaseSO))]
public class ItemDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // วาดหน้าต่าง Inspector ตามปกติ
        DrawDefaultInspector();

        ItemDatabaseSO database = (ItemDatabaseSO)target;

        GUILayout.Space(10);

        // สร้างปุ่มใน Inspector
        if (GUILayout.Button("Auto-Scan All Items", GUILayout.Height(30)))
        {
            ScanAndLoadAllItems(database);
        }
    }

    private void ScanAndLoadAllItems(ItemDatabaseSO database)
    {
        database.allItems.Clear();

        // คำสั่งนี้จะค้นหาไฟล์ประเภท ItemSO ทั่วทั้งโปรเจกต์ (ไม่ต้องสน Path เลย!)
        string[] guids = AssetDatabase.FindAssets("t:ItemSO");

        foreach (string guid in guids)
        {
            // แปลง GUID กลับเป็น Path
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // โหลดไฟล์ขึ้นมา
            ItemSO item = AssetDatabase.LoadAssetAtPath<ItemSO>(path);

            if (item != null)
            {
                database.allItems.Add(item);
            }
        }

        // แจ้งเตือน Unity ว่าข้อมูลมีการเปลี่ยนแปลง และให้ Save ทับลงไป
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();

        Debug.Log($"<color=green>Successfully scanned and loaded {database.allItems.Count} items into the database!</color>");
    }
}