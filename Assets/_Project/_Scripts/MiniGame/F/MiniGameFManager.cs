using UnityEngine;
using System.Collections.Generic;

public class MiniGameFManager : MonoBehaviour
{
    // ทำให้เป็น Singleton เพื่อให้ Script อื่นเรียกใช้งานได้ง่าย
    public static MiniGameFManager Instance;

    [Header("Game Settings")]
    public int score = 0;
    public float hitRadius = 150f; // รัศมีวงกลมระยะฟันโดน (ปรับใน Inspector)

    [Header("UI References")]
    public RectTransform hitCenter; // ตำแหน่งเป้าตรงกลางจอที่จะให้ฟัน
    public UI_Ingredient ingredientPrefab; // Prefab ของวัตถุดิบ (UI Image)
    public RectTransform spawnPoint; // จุดเกิดตั้งต้น (เช่น ขอบจอด้านล่าง)

    // ลิสต์เก็บวัตถุดิบที่อยู่บนหน้าจอ
    private List<UI_Ingredient> activeIngredients = new List<UI_Ingredient>();

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // 1. เช็คการกดฟัน (เช่น กด Spacebar หรือ คลิกเมาส์ซ้าย)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            CheckHit();
        }

        // ตัวอย่าง: กดปุ่ม 'S' เพื่อทดสอบเสกวัตถุดิบ
        if (Input.GetKeyDown(KeyCode.S))
        {
            AddIngredient();
        }
    }

    public void AddIngredient()
    {
        // 1. สร้าง Prefab และจัดตำแหน่งเริ่มต้นให้อยู่ที่ spawnPoint
        GameObject go = Instantiate(ingredientPrefab.gameObject, spawnPoint.parent);
        RectTransform ingredientRect = go.GetComponent<RectTransform>();
        ingredientRect.anchoredPosition = spawnPoint.anchoredPosition;

        UI_Ingredient ingredient = go.GetComponent<UI_Ingredient>();

        // 2. ดึงค่าจากจุดเริ่มต้น และ จุดเป้าหมาย
        Vector2 startPos = ingredientRect.anchoredPosition;
        Vector2 targetPos = hitCenter.anchoredPosition;
        float gravity = ingredient.gravity; // ดึงค่าแรงโน้มถ่วงจาก Prefab

        // 3. คำนวณความเร็วแกน Y (ให้ลอยไปถึงความสูงของเป้าหมายพอดี)
        float heightDifference = targetPos.y - startPos.y;
        if (heightDifference < 0) heightDifference = 0; // ป้องกันรูทติดลบกรณีจุดเกิดอยู่สูงกว่าเป้า

        float velocityY = Mathf.Sqrt(2f * gravity * heightDifference);

        // 4. คำนวณเวลาที่ใช้เดินทางไปถึงจุดสูงสุด (เป้าหมาย)
        float timeToPeak = velocityY / gravity;

        // 5. คำนวณความเร็วแกน X (ให้เดินทางไปถึงกึ่งกลางเป้าหมายในเวลาที่กำหนดพอดี)
        float distanceX = targetPos.x - startPos.x;
        float velocityX = distanceX / timeToPeak;

        // 6. ส่งค่าแรงโยนที่คำนวณได้เป๊ะๆ ไปให้วัตถุดิบ
        ingredient.SetVelocity(new Vector2(velocityX, velocityY));

        // สมัครรับข่าวสาร (Subscribe) เมื่อวัตถุดิบชิ้นนี้ตกจอ ให้มาเรียกฟังก์ชัน RemoveIngredientFromList
        ingredient.OnMissTarget += RemoveIngredientFromList;

        // เก็บเข้า List
        activeIngredients.Add(ingredient);
    }

    void CheckHit()
    {
        // ใช้ for ลูปแบบถอยหลัง เพราะเราอาจจะมีการลบข้อมูลออกจาก List (ป้องกัน Error)
        for (int i = activeIngredients.Count - 1; i >= 0; i--)
        {
            UI_Ingredient ingredient = activeIngredients[i];

            // หาระยะห่างระหว่างตำแหน่งของ "เป้าฟัน" กับ "วัตถุดิบ"
            float distance = Vector2.Distance(hitCenter.anchoredPosition, ingredient.Rect.anchoredPosition);

            if (distance <= hitRadius)
            {
                // ฟันโดน!
                score += 1;
                Debug.Log("Hit! Score: " + score);

                activeIngredients.RemoveAt(i); // เอาออกจาก List
                ingredient.DestroySelf(); // สั่งให้วัตถุดิบทำลายตัวเอง

                return; // ฟันโดน 1 ชิ้นแล้วหยุดทำงานเลย (ถ้าอยากฟันทีเดียวโดนหลายชิ้น ให้ลบบรรทัดนี้ออก)
            }
        }

        // ถ้าลูปจบแล้วไม่เข้า if ด้านบนเลย แสดงว่าฟันวืด
        Debug.Log("Miss! (ฟันลม)");
    }

    // ฟังก์ชันให้วัตถุดิบเรียกใช้ เวลาตัวมันตกออกนอกจอ
    public void RemoveIngredientFromList(UI_Ingredient ingredient)
    {
        if (activeIngredients.Contains(ingredient))
        {
            activeIngredients.Remove(ingredient);
        }
    }

    private void OnDrawGizmos()
    {
        // 1. วาดวงกลมแสดงระยะฟัน (Hit Radius) ที่ตรงกลางเป้าหมาย
        if (hitCenter != null)
        {
            // ตั้งสีเป็นสีเขียว
            Gizmos.color = Color.green;

            // แปลงระยะรัศมีจากหน่วย UI ให้เป็นหน่วย World Space เพื่อให้วาดได้ขนาดเป๊ะๆ
            float worldHitRadius = hitRadius * hitCenter.lossyScale.x;

            // วาดเส้นวงกลม
            Gizmos.DrawWireSphere(hitCenter.position, worldHitRadius);
        }

        // 2. วาดจุดเกิด (Spawn Point) และเส้นวิถีโยนคร่าวๆ
        if (spawnPoint != null)
        {
            // ตั้งสีจุดเกิดเป็นสีแดง
            Gizmos.color = Color.red;

            // วาดจุดเล็กๆ เพื่อให้รู้ว่า Spawn Point อยู่ตรงไหน (ขนาด 20 พิกเซล)
            float worldSpawnRadius = 20f * spawnPoint.lossyScale.x;
            Gizmos.DrawWireSphere(spawnPoint.position, worldSpawnRadius);

            // วาดเส้นลากจากจุดเกิดไปหาเป้าหมาย (เพื่อให้เห็นทิศทาง)
            if (hitCenter != null)
            {
                // ตั้งสีเส้นเป็นสีเหลือง
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(spawnPoint.position, hitCenter.position);
            }
        }
    }

}