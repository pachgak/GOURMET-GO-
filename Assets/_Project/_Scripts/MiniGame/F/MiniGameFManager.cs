using UnityEngine;
using System.Collections.Generic;

// บังคับว่าถ้าแปะ Manager ต้องมี SlashEffectController แปะอยู่ด้วย ป้องกันลืม
[RequireComponent(typeof(SlashEffectController))]
public class MiniGameFManager : MonoBehaviour
{
    public static MiniGameFManager Instance;

    [Header("Game Settings")]
    public int score = 0;
    public float hitRadius = 150f;
    public float timeToReachTarget = 1.0f;

    [Header("UI References")]
    public RectTransform hitCenter;
    public UI_Ingredient ingredientPrefab;
    public RectTransform spawnPoint;
    public RectTransform spawnParent;

    private List<UI_Ingredient> activeIngredients = new List<UI_Ingredient>();

    // ตัวแปรเก็บสคริปต์เอฟเฟค
    private SlashEffectController _slashEffect;

    void Awake()
    {
        Instance = this;

        // ดึงคอมโพเนนต์ที่อยู่บน GameObject เดียวกันมาเก็บไว้ใช้งาน
        _slashEffect = GetComponent<SlashEffectController>();
    }

    void Update()
    {
        AutoHit();

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            // สั่งให้สคริปต์เอฟเฟคทำงาน
            if (_slashEffect != null)
            {
                _slashEffect.PlaySlashEffect();
            }

            CheckHit();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            AddIngredient();
        }
    }

    public void AddIngredient()
    {
        // 1. สร้าง Prefab และจัดตำแหน่งเริ่มต้นให้อยู่ที่ spawnPoint
        GameObject go = Instantiate(ingredientPrefab.gameObject, spawnParent);
        RectTransform ingredientRect = go.GetComponent<RectTransform>();
        ingredientRect.anchoredPosition = spawnPoint.anchoredPosition;

        UI_Ingredient ingredient = go.GetComponent<UI_Ingredient>();

        // 2. ดึงค่าจากจุดเริ่มต้น และ จุดเป้าหมาย
        Vector2 startPos = ingredientRect.anchoredPosition;
        Vector2 targetPos = hitCenter.anchoredPosition;

        ////float gravity = gravity; // ดึงค่าแรงโน้มถ่วงจาก Prefab
        //// 3. คำนวณความเร็วแกน Y (ให้ลอยไปถึงความสูงของเป้าหมายพอดี)
        //float heightDifference = targetPos.y - startPos.y;
        //if (heightDifference < 0) heightDifference = 0; // ป้องกันรูทติดลบกรณีจุดเกิดอยู่สูงกว่าเป้า

        //float velocityY = Mathf.Sqrt(2f * gravity * heightDifference);

        //// 4. คำนวณเวลาที่ใช้เดินทางไปถึงจุดสูงสุด (เป้าหมาย)
        //float timeToPeak = velocityY / gravity;

        //// 5. คำนวณความเร็วแกน X (ให้เดินทางไปถึงกึ่งกลางเป้าหมายในเวลาที่กำหนดพอดี)
        //float distanceX = targetPos.x - startPos.x;
        //float velocityX = distanceX / timeToPeak;

        float heightDifference = targetPos.y - startPos.y;
        if (heightDifference < 0) heightDifference = 0;
        float distanceX = targetPos.x - startPos.x;

        // --- สูตรคำนวณใหม่ โดยยึดเวลา (timeToReachTarget) เป็นหลัก ---

        // 1. ความเร็วแกน X = ระยะทาง / เวลา (วิ่งไปถึงแนวนอนพอดี)
        float velocityX = distanceX / timeToReachTarget;

        // 2. ความเร็วแกน Y = (2 * ระยะความสูง) / เวลา (พุ่งไปถึงความสูงพอดีเป๊ะ)
        float velocityY = (2f * heightDifference) / timeToReachTarget;

        // 3. คำนวณแรงโน้มถ่วงที่ต้องใช้ (g = ความเร็ว Y / เวลา)
        float calculatedGravity = velocityY / timeToReachTarget;

        // 6. ส่งค่าแรงโยนที่คำนวณได้เป๊ะๆ ไปให้วัตถุดิบ
        ingredient.SetVelocity(new Vector2(velocityX, velocityY));
        ingredient.SetGravity(calculatedGravity);
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
                ingredient.HitDestroySelf(); // สั่งให้วัตถุดิบทำลายตัวเอง

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

    void AutoHit()
    {
        // ใช้ for ลูปแบบถอยหลัง เพราะเราอาจจะมีการลบข้อมูลออกจาก List (ป้องกัน Error)
        for (int i = activeIngredients.Count - 1; i >= 0; i--)
        {
            UI_Ingredient ingredient = activeIngredients[i];

            // หาระยะห่างระหว่างตำแหน่งของ "เป้าฟัน" กับ "วัตถุดิบ"
            float distance = Vector2.Distance(hitCenter.anchoredPosition, ingredient.Rect.anchoredPosition);

            if (distance <= 10f)
            {
                // ฟันโดน!
                score += 1;

                activeIngredients.RemoveAt(i); // เอาออกจาก List
                ingredient.HitDestroySelf(); // สั่งให้วัตถุดิบทำลายตัวเอง

                // สั่งให้สคริปต์เอฟเฟคทำงาน
                if (_slashEffect != null)
                {
                    _slashEffect.PlaySlashEffect();
                }

                return; // ฟันโดน 1 ชิ้นแล้วหยุดทำงานเลย (ถ้าอยากฟันทีเดียวโดนหลายชิ้น ให้ลบบรรทัดนี้ออก)
            }
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