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

    public static T FindExactSOByName<T>(string itemName, string[] searchFolders) where T : ScriptableObject
    {
        if (string.IsNullOrWhiteSpace(itemName)) return null;

        // 1. สร้างชื่อไฟล์ SO ที่คาดหวัง (ตามหลักการสร้างไฟล์ใน CSVLoader)
        // เช่น "Apple Salted Caramel Skewer" -> "Apple_Salted_Caramel_Skewer.asset"
        string expectedFileName = itemName.Trim().Replace(" ", "_") + ".asset";

        // 2. วนลูปค้นหาในแต่ละ Path ที่กำหนด
        foreach (string folderPath in searchFolders)
        {
            // ตรวจสอบและทำให้ Path ถูกต้อง (มี / ต่อท้าย)
            string cleanedFolderPath = folderPath;
            if (!cleanedFolderPath.EndsWith("/") && !cleanedFolderPath.EndsWith("\\"))
            {
                cleanedFolderPath += "/";
            }

            // สร้าง Full Path ที่สมบูรณ์แบบ
            string fullSOPath = cleanedFolderPath + expectedFileName;

            // 3. ใช้ LoadAssetAtPath เพื่อค้นหาไฟล์นั้นโดยตรง (แม่นยำที่สุด)
            T itemFind = AssetDatabase.LoadAssetAtPath<T>(fullSOPath);

            if (itemFind != null)
            {
                // พบ Asset แล้ว คืนค่าทันที
                return itemFind;
            }
        }

        // Debug.LogWarning($"Asset of type {typeof(T).Name} named '{itemName}' (Expected file: {expectedFileName}) not found in specified folders.");
        return null; // ไม่พบในทุกโฟลเดอร์
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

        // 1. ตรวจสอบและ Log เมื่อพบ Asset ซ้ำ
        if (guids.Length > 1)
        {
            // แจ้งเตือนรวมว่าพบกี่รายการ และรายการแรกที่ถูกเลือกใช้คืออะไร
            Debug.LogWarning($"!!! Found {guids.Length} multiple assets named '{itemName}' of type {typeof(T).Name}. " +
                             $"Using the first one found at {AssetDatabase.GUIDToAssetPath(guids[0])}.");
            Debug.LogWarning("--- List of all duplicates found: ---");

            // 2. วนลูป Log ชื่อและ Path ของ Asset ที่ซ้ำกันทั้งหมด
            int index = 0;
            foreach (string guid in guids)
            {
                // แปลง GUID เป็น Full Asset Path
                string duplicatePath = AssetDatabase.GUIDToAssetPath(guid);

                // เนื่องจากเราทราบชื่อ ItemName อยู่แล้ว เราจะ Log GUID และ Path
                Debug.LogWarning($"\t [{index}] GUID: {guid} | Path: {duplicatePath}");
                index++;
            }
            Debug.LogWarning("------------------------------------");
        }

        // 3. โหลด Asset ตัวแรกที่พบ
        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        T assetFind = AssetDatabase.LoadAssetAtPath<T>(assetPath);

        return assetFind;
    }
}