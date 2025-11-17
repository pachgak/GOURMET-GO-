using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Inventory.Model; // ตรวจสอบว่า namespace ถูกต้อง

public static class SOAssetLoader
{
    /// <summary>
    /// โหลด ItemSO ทั้งหมดที่อยู่ในโฟลเดอร์ที่กำหนด
    /// </summary>
    /// <typeparam name="T">ประเภทของ ScriptableObject ที่สืบทอดจาก ItemSO</typeparam>
    /// <param name="folderPath">Path ของโฟลเดอร์เริ่มต้น (เช่น "Assets/_DataSO/Inventory/Items/")</param>
    /// <returns>Array ของ ItemSO ที่พบ</returns>
    public static T[] LoadAllSOsInFolder<T>(string folderPath) where T : ScriptableObject
    {
        // 1. ตรวจสอบให้ Path สิ้นสุดด้วย '/' เพื่อความปลอดภัย
        if (!folderPath.EndsWith("/"))
        {
            folderPath += "/";
        }

        // 2. ใช้ AssetDatabase.FindAssets ในการค้นหา
        // "t:T" หมายถึงการค้นหา Assets ที่เป็นประเภท T
        // 
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folderPath });

        // 3. โหลด Asset ทั้งหมด
        List<T> items = new List<T>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            T item = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            if (item != null)
            {
                items.Add(item);
            }
        }

        return items.ToArray();
    }
}