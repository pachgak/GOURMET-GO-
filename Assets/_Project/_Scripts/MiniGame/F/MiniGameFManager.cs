using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

[RequireComponent(typeof(SlashEffectController))]
public class MiniGameFManager : MonoBehaviour
{
    public static MiniGameFManager Instance;

    [Header("Game Settings")]
    public int currentScore = 0;
    public float hitRadius = 150f;
    public float hitPerfectRadius = 75f;
    public float hitGoodRadius = 125f;
    public float timeToReachTarget = 1.0f;
    public int maxScore = 100;
    public List<Sprite> ingredientSprites = new List<Sprite>();

    // --- เพิ่มการตั้งค่าองศาการโยน ---
    [Header("Throw Rotation Settings")]
    public float minSpawnRotationOffset = 90f;  // หมุนเบี้ยวตอนเกิดน้อยสุด (เช่น 90 องศา)
    public float maxSpawnRotationOffset = 180f; // หมุนเบี้ยวตอนเกิดมากสุด (เช่น 180 องศา)

    [Header("UI References")]
    public RectTransform hitCenter;
    public UI_Ingredient ingredientPrefab;
    public RectTransform spawnPoint;
    public RectTransform spawnParent;


    [Header("Debug")]
    public bool isAutoHit = false;

    private List<UI_Ingredient> activeIngredients = new List<UI_Ingredient>();
    private SlashEffectController _slashEffect;

    public Action<HitQuality, int> OnHitEvaluated;
    public Action<int> OnIngredientDropped;
    public Action<int> OnScoreUpdated;

    public enum HitQuality
    {
        Perfect,
        Good,
        Bad // ขอใช้คำว่า Bad แทน Miss เพื่อไม่ให้สับสนกับตอนที่ปล่อยของตกจอโดยไม่ได้ฟันครับ
    }

    void Awake()
    {
        Instance = this;

        // ดึงคอมโพเนนต์ที่อยู่บน GameObject เดียวกันมาเก็บไว้ใช้งาน
        _slashEffect = GetComponent<SlashEffectController>();

        AddScore(0);
    }

    void Update()
    {
        if (isAutoHit) AutoHit();

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
        GameObject go = Instantiate(ingredientPrefab.gameObject, spawnParent);
        RectTransform ingredientRect = go.GetComponent<RectTransform>();
        ingredientRect.anchoredPosition = spawnPoint.anchoredPosition;

        UI_Ingredient ingredient = go.GetComponent<UI_Ingredient>();

        // ---เพิ่มระบบสุ่มรูปภาพตรงนี้-- -
        if (ingredientSprites.Count > 0)
        {
            // สุ่มตัวเลขตั้งแต่ 0 ถึงตัวสุดท้ายใน List
            int randomIndex = Random.Range(0, ingredientSprites.Count);

            // นำรูปที่สุ่มได้ไปใส่ใน Image ของวัตถุดิบ
            ingredient.IngredientImage.sprite = ingredientSprites[randomIndex];

            // [ทริคเสริม] สั่งให้ Image ปรับขนาดกว้างxยาว ตามภาพต้นฉบับจริงๆ 
            // เผื่อรูปกะหล่ำปลี กับ แครอท มีขนาดไฟล์ไม่เท่ากัน ภาพจะได้ไม่ยืดหรือเบี้ยวครับ
            //ingredient.IngredientImage.SetNativeSize();
        }
        // -----------------------------

        Vector2 startPos = ingredientRect.anchoredPosition;
        Vector2 targetPos = hitCenter.anchoredPosition;

        float heightDifference = targetPos.y - startPos.y;
        if (heightDifference < 0) heightDifference = 0;
        float distanceX = targetPos.x - startPos.x;

        float velocityX = distanceX / timeToReachTarget;
        float velocityY = (2f * heightDifference) / timeToReachTarget;
        float calculatedGravity = velocityY / timeToReachTarget;

        ingredient.SetVelocity(new Vector2(velocityX, velocityY));
        ingredient.SetGravity(calculatedGravity);

        // --- เพิ่มการสุ่มค่าหมุนและส่งไปให้ Ingredient ---
        float randomRotationOffset = Random.Range(minSpawnRotationOffset, maxSpawnRotationOffset);
        ingredient.SetRotation(randomRotationOffset, timeToReachTarget);

        ingredient.OnMissTarget += HeadleIngredientMiss;
        activeIngredients.Add(ingredient);
    }

    //void CheckHit()
    //{
    //    for (int i = activeIngredients.Count - 1; i >= 0; i--)
    //    {
    //        UI_Ingredient ingredient = activeIngredients[i];

    //        // ระยะห่างรวมเพื่อเช็คว่าอยู่ในวงกลมไหม (ใช้ Distance แบบเดิม)
    //        float distance = Vector2.Distance(hitCenter.anchoredPosition, ingredient.Rect.anchoredPosition);

    //        if (distance <= hitRadius)
    //        {
    //            score += 1;

    //            // --- เพิ่มการคำนวณระยะห่างเฉพาะแกน X ---
    //            float distanceX = ingredient.Rect.anchoredPosition.x - hitCenter.anchoredPosition.x;

    //            // แปลงค่าระยะห่างให้อยู่ในช่วง -1 ถึง 1 (Clamp ไว้กันเหนียวเผื่อบั๊กทะลุขอบ)
    //            float normalizedDistX = Mathf.Clamp(distanceX / hitRadius, -1f, 1f);

    //            // สูตรคำนวณ: ค่าเริ่มต้น 0.5 + (ค่าที่แปลงแล้ว * 0.5)
    //            // ถ้า distanceX = 0 -> 0.5 + 0 = 0.5
    //            // ถ้า distanceX ติดลบ (ฟันเร็วไป/อยู่ฝั่งซ้าย) ซีกซ้ายจะน้อยกว่า 0.5
    //            // ถ้า distanceX เป็นบวก (ฟันช้าไป/อยู่ฝั่งขวา) ซีกซ้ายจะมากกว่า 0.5
    //            float leftFill = 0.5f + (normalizedDistX * 0.5f);

    //            activeIngredients.RemoveAt(i);

    //            // ส่งค่าสัดส่วนซีกซ้ายไปให้ฟังก์ชัน
    //            ingredient.HitDestroySelf(leftFill);

    //            return;
    //        }
    //    }


    //}

    void CheckHit()
    {
        for (int i = activeIngredients.Count - 1; i >= 0; i--)
        {
            UI_Ingredient ingredient = activeIngredients[i];

            // ระยะห่างรวมเพื่อเช็คว่าอยู่ในวงกลมไหม (ใช้ Distance แบบเดิม)
            float distance = Vector2.Distance(hitCenter.anchoredPosition, ingredient.Rect.anchoredPosition);

            if (distance <= hitRadius)
            {
                // ประเมินผลการฟัน
                if (distance < hitPerfectRadius)
                {
                    int perfectScore = 5;
                    AddScore(perfectScore);
                    OnHitEvaluated?.Invoke(HitQuality.Perfect, perfectScore);
                }
                else if (distance < hitGoodRadius)
                {
                    int goodScore = 2; // (แอบเปลี่ยนคะแนน Good ให้ต่างจาก Perfect นิดนึง)
                    AddScore(goodScore);
                    OnHitEvaluated?.Invoke(HitQuality.Good, goodScore);
                }
                else
                {
                    int badScore = 1; // ฟันโดน แต่โดนขอบๆ เลยติดลบ
                    AddScore(badScore);
                    OnHitEvaluated?.Invoke(HitQuality.Bad, badScore);
                }

                // --- เพิ่มการคำนวณระยะห่างเฉพาะแกน X ---
                float distanceX = ingredient.Rect.anchoredPosition.x - hitCenter.anchoredPosition.x;

                // แปลงค่าระยะห่างให้อยู่ในช่วง -1 ถึง 1 (Clamp ไว้กันเหนียวเผื่อบั๊กทะลุขอบ)
                float normalizedDistX = Mathf.Clamp(distanceX / hitRadius, -1f, 1f);

                // สูตรคำนวณ: ค่าเริ่มต้น 0.5 + (ค่าที่แปลงแล้ว * 0.5)
                // ถ้า distanceX = 0 -> 0.5 + 0 = 0.5
                // ถ้า distanceX ติดลบ (ฟันเร็วไป/อยู่ฝั่งซ้าย) ซีกซ้ายจะน้อยกว่า 0.5
                // ถ้า distanceX เป็นบวก (ฟันช้าไป/อยู่ฝั่งขวา) ซีกซ้ายจะมากกว่า 0.5
                float leftFill = 0.5f + (normalizedDistX * 0.5f);

                activeIngredients.RemoveAt(i);

                // ส่งค่าสัดส่วนซีกซ้ายไปให้ฟังก์ชัน
                ingredient.HitDestroySelf(leftFill);

                return;
            }
        }
    }

    // ฟังก์ชันให้วัตถุดิบเรียกใช้ เวลาตัวมันตกออกนอกจอ
    public void HeadleIngredientMiss(UI_Ingredient ingredient)
    {
        if (activeIngredients.Contains(ingredient))
        {
            activeIngredients.Remove(ingredient);

            // หักคะแนนตอนตกจอ (ถ้าต้องการ)
            int missScore = -1; // ฟันโดน แต่โดนขอบๆ เลยติดลบ

            AddScore(missScore);

            // ตะโกนบอกให้ UI รู้ว่ามีของตกจอแล้ว!
            OnIngredientDropped?.Invoke(missScore);
        }
    }

    void AutoHit()
    {
        for (int i = activeIngredients.Count - 1; i >= 0; i--)
        {
            UI_Ingredient ingredient = activeIngredients[i];

            // ระยะห่างรวมเพื่อเช็คว่าอยู่ในวงกลมไหม (ใช้ Distance แบบเดิม)
            float distance = Vector2.Distance(hitCenter.anchoredPosition, ingredient.Rect.anchoredPosition);

            if (distance <= hitRadius)
            {
                currentScore += 1;

                // --- เพิ่มการคำนวณระยะห่างเฉพาะแกน X ---
                float distanceX = ingredient.Rect.anchoredPosition.x - hitCenter.anchoredPosition.x;

                // แปลงค่าระยะห่างให้อยู่ในช่วง -1 ถึง 1 (Clamp ไว้กันเหนียวเผื่อบั๊กทะลุขอบ)
                float normalizedDistX = Mathf.Clamp(distanceX / hitRadius, -1f, 1f);

                // สูตรคำนวณ: ค่าเริ่มต้น 0.5 + (ค่าที่แปลงแล้ว * 0.5)
                // ถ้า distanceX = 0 -> 0.5 + 0 = 0.5
                // ถ้า distanceX ติดลบ (ฟันเร็วไป/อยู่ฝั่งซ้าย) ซีกซ้ายจะน้อยกว่า 0.5
                // ถ้า distanceX เป็นบวก (ฟันช้าไป/อยู่ฝั่งขวา) ซีกซ้ายจะมากกว่า 0.5
                float rightFill = 0.5f + (normalizedDistX * 0.5f);

                activeIngredients.RemoveAt(i);

                // ส่งค่าสัดส่วนซีกซ้ายไปให้ฟังก์ชัน
                ingredient.HitDestroySelf(rightFill);

                if (_slashEffect != null) _slashEffect.PlaySlashEffect();

                return;
            }
        }
    }

    private void AddScore(int score)
    {
        currentScore += score;
        OnScoreUpdated?.Invoke(currentScore);
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

        // 2. วาดจุดเกิด (Spawn Point) และเส้นวิถีโยนแบบเต็มเส้น
        if (spawnPoint != null)
        {
            Gizmos.color = Color.red;
            float worldSpawnRadius = 20f * spawnPoint.lossyScale.x;
            Gizmos.DrawWireSphere(spawnPoint.position, worldSpawnRadius);

            if (hitCenter != null)
            {
                // ตั้งสีเส้นเป็นสีเหลือง
                Gizmos.color = Color.yellow;

                int segmentsToCenter = 10; // จำนวนเส้นย่อยจากจุดเกิดไปถึงกลางจอ
                int totalSegments = 21;    // จำนวนเส้นทั้งหมด (วาดเผื่อให้ s = 2.2 เพื่อให้เห็นตอนตกหลุดจอชัดๆ)

                Vector3 previousPoint = spawnPoint.position;

                float startY = spawnPoint.position.y;
                float targetY = hitCenter.position.y;
                float heightDiff = Mathf.Max(0, targetY - startY);

                // หาระยะทางแกน X จากจุดเกิดไปหาเป้าหมาย
                float distX = hitCenter.position.x - spawnPoint.position.x;
                float distZ = hitCenter.position.z - spawnPoint.position.z;

                for (int i = 1; i <= totalSegments; i++)
                {
                    // ถ้า i = 10 ค่า s จะเท่ากับ 1.0 (ถึงกลางจอพอดี)
                    // ถ้า i > 10 ค่า s จะเกิน 1.0 (เริ่มโค้งตกลงมาอีกฝั่ง)
                    float s = (float)i / segmentsToCenter;

                    // แกน X เดินหน้าต่อไปเรื่อยๆ
                    float x = spawnPoint.position.x + (distX * s);

                    // แกน Y ใช้สมการพาราโบลาเดิม (พอมันคูณค่า s ที่เกิน 1.0 กราฟมันจะดิ่งลงเองตามธรรมชาติ)
                    float y = startY + (heightDiff * s * (2f - s));

                    float z = spawnPoint.position.z + (distZ * s);

                    Vector3 nextPoint = new Vector3(x, y, z);

                    // วาดเส้นเชื่อมจุด
                    Gizmos.DrawLine(previousPoint, nextPoint);
                    previousPoint = nextPoint;
                }

                // วาดเส้นวงกลม Good (สีเหลือง)
                Gizmos.color = Color.yellow;
                float worldGoodRadius = hitGoodRadius * hitCenter.lossyScale.x;
                Gizmos.DrawWireSphere(hitCenter.position, worldGoodRadius);

                // วาดเส้นวงกลม Perfect (สีฟ้า)
                Gizmos.color = Color.cyan;
                float worldPerfectRadius = hitPerfectRadius * hitCenter.lossyScale.x;
                Gizmos.DrawWireSphere(hitCenter.position, worldPerfectRadius);
            }
        }
    }

}