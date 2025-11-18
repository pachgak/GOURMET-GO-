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

    // เมธอด 1: โหลด Asset ทั้งหมดในโฟลเดอร์ (Recursive)
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

    public static T FindSOByName<T>(string itemName, string[] searchFolders) where T : ScriptableObject
    {
        if (string.IsNullOrWhiteSpace(itemName)) return null;

        // 1. สร้าง Filter สำหรับค้นหา
        // t:T หมายถึง ค้นหา Asset ที่เป็น Type T (เช่น t:EatableItemSO)
        // {itemName} หมายถึง ค้นหาตามชื่อไฟล์
        string filter = $"t:{typeof(T).Name} {itemName}";

        // 2. ค้นหา Asset ทั้งหมดที่ตรงตามเกณฑ์
        string[] guids = AssetDatabase.FindAssets(filter, searchFolders);

        if (guids.Length == 0)
        {
            // ไม่พบ
            Debug.LogWarning($"Asset of type {typeof(T).Name} named '{itemName}' not found in specified folders.");
            return null;
        }

        // 3. โหลด Asset ตัวแรกที่พบ
        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        T assetFind = AssetDatabase.LoadAssetAtPath<T>(assetPath);

        if (guids.Length > 1)
        {
            Debug.LogWarning($"Found multiple assets named '{itemName}' of type {typeof(T).Name}. Using the first one found at {assetPath}.");
        }

        return assetFind;
    }
}