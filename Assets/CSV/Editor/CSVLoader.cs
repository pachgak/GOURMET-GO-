using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using Inventory.Model;
using System.Collections.Generic; // ตรวจสอบว่า namespace ถูกต้อง

/// <summary>
/// Editor script สำหรับการโหลดข้อมูล Item จาก CSV เข้าสู่ ScriptableObject
/// ไฟล์นี้ต้องอยู่ในโฟลเดอร์ชื่อ "Editor"
/// </summary>
public class CSVLoader : EditorWindow
{
    private static string csvFoodItemPath = "Assets/CSV/Gourmet Go! CSV Stat - Food Item.csv"; // เปลี่ยนเป็น Path ของไฟล์ CSV ของคุณ
    private static string FoodItemOSPath = "Assets/_DataSO/Inventory/Items/Food Item/"; // Path ที่จะเก็บ ScriptableObject

    private static string csvMonsterItemPath = "Assets/CSV/Gourmet Go! CSV Stat - Monster Item.csv"; // เปลี่ยนเป็น Path ของไฟล์ CSV ของคุณ
    private static string MonsterItemOSPath = "Assets/_DataSO/Inventory/Items/Monster Item/"; // Path ที่จะเก็บ ScriptableObject

    private static string csvNaturalItemPath = "Assets/CSV/Gourmet Go! CSV Stat - Natural Item.csv"; // เปลี่ยนเป็น Path ของไฟล์ CSV ของคุณ
    private static string NaturalItemOSPath = "Assets/_DataSO/Inventory/Items/Natural Item/"; // Path ที่จะเก็บ ScriptableObject

    private static string csvGourmetCreaturePath = "Assets/CSV/Gourmet Go! CSV Stat - Gourmet Creature.csv"; // เปลี่ยนเป็น Path ของไฟล์ CSV ของคุณ
    private static string GourmetCreatureOSPath = "Assets/_DataSO/Enity/Gourmet Creature/"; // Path ที่จะเก็บ ScriptableObject

    private static string csvGourmetNaturalPath = "Assets/CSV/Gourmet Go! CSV Stat - Gourmet Natural.csv"; // เปลี่ยนเป็น Path ของไฟล์ CSV ของคุณ
    private static string GourmetNaturalOSPath = "Assets/_DataSO/Enity/Gourmet Natural/"; // Path ที่จะเก็บ ScriptableObject

    private static string csvPlayerPassivePath = "Assets/CSV/Gourmet Go! CSV Stat - Passive List.csv"; // เปลี่ยนเป็น Path ของไฟล์ CSV ของคุณ
    private static string PlayerPassiveOSPath = "Assets/_DataSO/Enity/Gourmet Natural/"; // Path ที่จะเก็บ ScriptableObject

    private static string csvPlayerSkillPath = "Assets/CSV/Gourmet Go! CSV Stat - Skill List.csv"; // เปลี่ยนเป็น Path ของไฟล์ CSV ของคุณ
    private static string PlayerSkillOSPath = "Assets/_DataSO/Player Skill/"; // Path ที่จะเก็บ ScriptableObject

    private static ItemDropRage.ItemDropFormat GetNewItemDropFormat(ItemSO itemDrop,string[] dropRandomCountTexts)
    {
        ItemDropRage.ItemDropFormat itemDropFormat = new ItemDropRage.ItemDropFormat();
        itemDropFormat.item = itemDrop;

        if (dropRandomCountTexts.Length == 1)
        {
            itemDropFormat.countMin = int.TryParse(dropRandomCountTexts[0].Trim(), out int resultA) ? resultA : 0;
            itemDropFormat.isRandom = false;
        }
        else
        {
            itemDropFormat.countMin = int.TryParse(dropRandomCountTexts[0].Trim(), out int resultB) ? resultB : 0;
            itemDropFormat.isRandom = true;
            itemDropFormat.countMin = int.TryParse(dropRandomCountTexts[1].Trim(), out int resultC) ? resultC : 0;
        }

        return itemDropFormat;
    }

    private static ItemSO FindItemSO(string itemName, string ItemSOPath)
    {
        ItemSO itemFind = null;

        if (string.IsNullOrWhiteSpace(itemName))
        {
            string soFileNameitem = itemName.Replace(" ", "_") + ".asset";
            string fullSOPathitem = ItemSOPath + soFileNameitem;
            itemFind = AssetDatabase.LoadAssetAtPath<ItemSO>(fullSOPathitem);

            if(itemFind = null) Debug.Log($"dont have {itemName} in {ItemSOPath}");
        }
        return itemFind;
    }

    [MenuItem("Tools/Load from CSV/Monster Item")]
    public static void LoadMonsterItemFromCSV()
    {
        string csvThisPath = csvMonsterItemPath;
        string AssetOSPath = MonsterItemOSPath;


        // 1. ตรวจสอบและสร้างโฟลเดอร์สำหรับเก็บ SO ถ้ายังไม่มี
        if (!Directory.Exists(AssetOSPath))
        {
            Directory.CreateDirectory(AssetOSPath);
            AssetDatabase.Refresh();
        }

        // 2. อ่านไฟล์ CSV
        if (!File.Exists(csvThisPath))
        {
            Debug.LogError($"CSV file not found at: {csvThisPath}");
            return;
        }

        // อ่านทุกบรรทัดใน CSV
        string[] lines = File.ReadAllLines(csvThisPath);

        // เก็บจำนวนหัวข้อ
        int expectedColumnCount = 0;
        if (lines.Length > 0 && !string.IsNullOrWhiteSpace(lines[0]))
        {
            // นับจำนวนคอลัมน์จากบรรทัดแรก (Header)
            expectedColumnCount = lines[0].Split(',').Length;
        }

        // ข้ามบรรทัดแรกที่เป็น Header
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] fields = line.Split(',');

            // ตรวจสอบจำนวนคอลัมน์ให้เท่ากับจำนวนหัวข้อ
            if (fields.Length < expectedColumnCount)
            {
                Debug.LogWarning($"Skipping line {i + 1} due to insufficient fields: {line}");
                continue;
            }

            //========= ดึงข้อมูลจาก CSV ================================\
            
            string itemName = fields[0].Trim();
            string description = fields[1].Trim();
            bool canStack = fields[2].ToBool(); // ใช้ Extension Method ที่จะสร้างในขั้นตอนถัดไป
            int maxStack = int.TryParse(fields[3].Trim(), out int result) ? result : 1;
            // "ได้รับ" (fields[4]) ยังไม่มีใน SO

            //======================================================================\

            // กำหนดชื่อไฟล์ SO
            string soFileName = itemName.Replace(" ", "_") + ".asset";
            string fullSOPath = AssetOSPath + soFileName;

            // 3. ตรวจสอบว่ามี SO อยู่แล้วหรือไม่
            EatableItemSO itemSO = AssetDatabase.LoadAssetAtPath<EatableItemSO>(fullSOPath);
            bool isNewitemSO = false;
            if (itemSO == null)
            {
                itemSO = ScriptableObject.CreateInstance<EatableItemSO>();
                isNewitemSO = true;
            }

            //========= ตั้งค่าเริ่มต้น ================================\

            itemSO.ItemName = itemName;
            itemSO.Description = description;
            itemSO.IsStackable = canStack;
            itemSO.MaxStackSize = maxStack;

            //=================================================================\
        

            if (isNewitemSO)
            {
                // สร้าง Asset
                AssetDatabase.CreateAsset(itemSO, fullSOPath);
                Debug.Log($"Created new ItemSO: {itemName}");
            }
            else
            {
                // แจ้ง Unity ว่ามีการเปลี่ยนแปลง
                EditorUtility.SetDirty(itemSO);
                Debug.Log($"Updated existing ItemSO: {itemName}");
            }
        }

        // 4. บันทึกการเปลี่ยนแปลงทั้งหมด
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CSV loading and ScriptableObject creation/update complete!");

    }

    [MenuItem("Tools/Load from CSV/Natural Item")]
    public static void LoadNaturalItemFromCSV()
    {
        string csvThisPath = csvNaturalItemPath;
        string AssetOSPath = NaturalItemOSPath;


        // 1. ตรวจสอบและสร้างโฟลเดอร์สำหรับเก็บ SO ถ้ายังไม่มี
        if (!Directory.Exists(AssetOSPath))
        {
            Directory.CreateDirectory(AssetOSPath);
            AssetDatabase.Refresh();
        }

        // 2. อ่านไฟล์ CSV
        if (!File.Exists(csvThisPath))
        {
            Debug.LogError($"CSV file not found at: {csvThisPath}");
            return;
        }

        // อ่านทุกบรรทัดใน CSV
        string[] lines = File.ReadAllLines(csvThisPath);

        // เก็บจำนวนหัวข้อ
        int expectedColumnCount = 0;
        if (lines.Length > 0 && !string.IsNullOrWhiteSpace(lines[0]))
        {
            // นับจำนวนคอลัมน์จากบรรทัดแรก (Header)
            expectedColumnCount = lines[0].Split(',').Length;
        }

        // ข้ามบรรทัดแรกที่เป็น Header
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] fields = line.Split(',');

            // ตรวจสอบจำนวนคอลัมน์ให้เท่ากับจำนวนหัวข้อ
            if (fields.Length < expectedColumnCount)
            {
                Debug.LogWarning($"Skipping line {i + 1} due to insufficient fields: {line}");
                continue;
            }

            //========= ดึงข้อมูลจาก CSV ================================\

            string itemName = fields[0].Trim();
            string description = fields[1].Trim();
            bool canStack = fields[2].ToBool(); // ใช้ Extension Method ที่จะสร้างในขั้นตอนถัดไป
            int maxStack = int.TryParse(fields[3].Trim(), out int result) ? result : 1;
            // "ได้รับ" (fields[4]) ยังไม่มีใน SO

            //======================================================================\

            // กำหนดชื่อไฟล์ SO
            string soFileName = itemName.Replace(" ", "_") + ".asset";
            string fullSOPath = AssetOSPath + soFileName;

            // 3. ตรวจสอบว่ามี SO อยู่แล้วหรือไม่
            EatableItemSO itemSO = AssetDatabase.LoadAssetAtPath<EatableItemSO>(fullSOPath);
            bool isNewitemSO = false;
            if (itemSO == null)
            {
                itemSO = ScriptableObject.CreateInstance<EatableItemSO>();
                isNewitemSO = true;
            }

            //========= ตั้งค่าเริ่มต้น ================================\

            itemSO.ItemName = itemName;
            itemSO.Description = description;
            itemSO.IsStackable = canStack;
            itemSO.MaxStackSize = maxStack;

            //=================================================================\


            if (isNewitemSO)
            {
                // สร้าง Asset
                AssetDatabase.CreateAsset(itemSO, fullSOPath);
                Debug.Log($"Created new ItemSO: {itemName}");
            }
            else
            {
                // แจ้ง Unity ว่ามีการเปลี่ยนแปลง
                EditorUtility.SetDirty(itemSO);
                Debug.Log($"Updated existing ItemSO: {itemName}");
            }
        }

        // 4. บันทึกการเปลี่ยนแปลงทั้งหมด
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CSV loading and ScriptableObject creation/update complete!");

    }

    [MenuItem("Tools/Load from CSV/Gourmet Creature")]
    public static void LoadGourmetCreatureFromCSV()
    {
        string csvThisPath = csvGourmetCreaturePath;
        string AssetOSPath = GourmetCreatureOSPath;


        // 1. ตรวจสอบและสร้างโฟลเดอร์สำหรับเก็บ SO ถ้ายังไม่มี
        if (!Directory.Exists(AssetOSPath))
        {
            Directory.CreateDirectory(AssetOSPath);
            AssetDatabase.Refresh();
        }

        // 2. อ่านไฟล์ CSV
        if (!File.Exists(csvThisPath))
        {
            Debug.LogError($"CSV file not found at: {csvThisPath}");
            return;
        }

        // อ่านทุกบรรทัดใน CSV
        string[] lines = File.ReadAllLines(csvThisPath);

        // เก็บจำนวนหัวข้อ
        int expectedColumnCount = 0;
        if (lines.Length > 0 && !string.IsNullOrWhiteSpace(lines[0]))
        {
            // นับจำนวนคอลัมน์จากบรรทัดแรก (Header)
            expectedColumnCount = lines[0].Split(',').Length;
        }

        // ข้ามบรรทัดแรกที่เป็น Header
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] fields = line.Split(',');

            // ตรวจสอบจำนวนคอลัมน์ให้เท่ากับจำนวนหัวข้อ
            if (fields.Length < expectedColumnCount)
            {
                Debug.LogWarning($"Skipping line {i + 1} due to insufficient fields: {line}");
                continue;
            }

            //========= ดึงข้อมูลจาก CSV ================================\

            string enemyName = fields[0].Trim();
            string description = fields[1].Trim();
            int hp = int.TryParse(fields[2].Trim(), out int result) ? result : 1;

            ItemSO itemDrop1 = FindItemSO(fields[3].Trim(), MonsterItemOSPath);
            string[] dropRandomCount1Texts = fields[4].Trim().Split('-');

            ItemSO itemDrop2 = FindItemSO(fields[5].Trim(), MonsterItemOSPath);
            string[] dropRandomCount2Texts = fields[6].Trim().Split('-');
            //bool canStack = fields[2].ToBool(); // ใช้ Extension Method ที่จะสร้างในขั้นตอนถัดไป
            // "ได้รับ" (fields[4]) ยังไม่มีใน SO

            //======================================================================\

            // กำหนดชื่อไฟล์ SO
            string soFileName = enemyName.Replace(" ", "_") + ".asset";
            string fullSOPath = AssetOSPath + soFileName;

            // 3. ตรวจสอบว่ามี SO อยู่แล้วหรือไม่
            EnemySO enemySO = AssetDatabase.LoadAssetAtPath<EnemySO>(fullSOPath);
            bool isNewitemSO = false;
            if (enemySO == null)
            {
                enemySO = ScriptableObject.CreateInstance<EnemySO>();
                isNewitemSO = true;
            }

            //========= ตั้งค่าเริ่มต้น ================================\

            enemySO.enemyName = enemyName;
            enemySO.Description = description;
            enemySO.hp = hp;
            List<ItemDropRage.ItemDropFormat> drop = new List<ItemDropRage.ItemDropFormat>();

            if (itemDrop1 != null)
            {
                ItemDropRage.ItemDropFormat itemDropFormat = GetNewItemDropFormat(itemDrop1 , dropRandomCount1Texts);
                drop.Add(itemDropFormat);
            }

            if (itemDrop2 != null)
            {
                ItemDropRage.ItemDropFormat itemDropFormat = GetNewItemDropFormat(itemDrop2, dropRandomCount2Texts);
                drop.Add(itemDropFormat);
            }
            //enemySO.drop.Add();


            //=================================================================\


            if (isNewitemSO)
            {
                // สร้าง Asset
                AssetDatabase.CreateAsset(enemySO, fullSOPath);
                Debug.Log($"Created new ItemSO: {enemyName}");
            }
            else
            {
                // แจ้ง Unity ว่ามีการเปลี่ยนแปลง
                EditorUtility.SetDirty(enemySO);
                Debug.Log($"Updated existing ItemSO: {enemyName}");
            }
        }

        // 4. บันทึกการเปลี่ยนแปลงทั้งหมด
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CSV loading and ScriptableObject creation/update complete!");

    }
}