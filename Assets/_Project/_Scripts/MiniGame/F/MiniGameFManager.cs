using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using Inventory.Model;

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
    private int _rewardCount; // <--- 1. เพิ่มตัวแปรจำจำนวน
    [Header("Game Loop Settings")]
    public bool isPlaying = false; // สถานะว่าเกมรันอยู่ไหม
    public float delayBetweenPatterns = 2.0f; // เวลาพักก่อนเริ่มสุ่ม Pattern ถัดไป

    [Header("Rhythm System")]
    public float responseDelay = 2.0f; // เวลาพักก่อนเริ่มเทิร์นของผู้เล่น
    public List<RhythmPattern> gamePatterns = new List<RhythmPattern>(); // จัด Pattern ผ่าน Inspector

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

    [Header("System")]
    private List<UI_Ingredient> activeIngredients = new List<UI_Ingredient>();
    private Sprite _rewardSprite;

    [Header("Action Event")]
    public Action<HitQuality, int> OnHitEvaluated;
    public Action<int> OnIngredientDropped;
    public Action<int> OnScoreUpdated;
    public Action OnPlaySoundRhythm;
    public Action OnPlaySoundHit;
    public Action OnPlaySoundSlat;
    public Action<Sprite, int> OnGameFinished;
    public Action OnSlashTriggered; // 3. เพิ่ม Action สำหรับบอกว่ามีการกดฟัน (หรือ AutoHit ฟัน)

    [Header("Coroutine")]
    private Coroutine currentPatternCoroutine;
    private Coroutine currentAudioSequenceCoroutine;
    private Coroutine currentSpawnSequenceCoroutine;
    private Coroutine gameLoopCoroutine; // เก็บตัวลูปเกมไว้ เพื่อสั่งหยุดได้

    private int _currentPatternIndex;
    public enum HitQuality
    {
        Perfect,
        Good,
        Bad // ขอใช้คำว่า Bad แทน Miss เพื่อไม่ให้สับสนกับตอนที่ปล่อยของตกจอโดยไม่ได้ฟันครับ
    }

    [System.Serializable]
    public class RhythmPattern
    {
        [Tooltip("เวลาต่อ 1 ตัวอักษร (เช่น 0.5 วินาที)")]
        public float stepDuration = 0.5f;

        [Tooltip("แพทเทิร์นจังหวะ เช่น 101011 (1=ฟัน, 0=เว้น)")]
        public string notes;

        // เพิ่ม Constructor เข้าไป
        public RhythmPattern(float s, string n)
        {
            stepDuration = s;
            notes = n;
        }
    }

    void Awake()
    {
        Instance = this;

        AddScore(0);
    }

    void Update()
    {
        // กดปุ่ม P เพื่อเริ่มเล่น Pattern ที่ 0
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayPattern(_currentPatternIndex);
        }

        if (isAutoHit) AutoHit();

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (!isPlaying) return;
            // สั่งให้สคริปต์เอฟเฟคทำงาน
            // 5. เปลี่ยนจากการเรียกสคริปต์ตรงๆ เป็นการตะโกนเรียก Event
            OnSlashTriggered?.Invoke();

            CheckHit();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            PlayPattern();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            StartGame();
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
        OnPlaySoundSlat?.Invoke();

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

                OnPlaySoundHit?.Invoke();

                return;
            }
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
                CheckHit();

                return;
            }
        }
    }

    private void AddScore(int score)
    {
        currentScore += score;
        if (currentScore < 0) currentScore = 0;

        OnScoreUpdated?.Invoke(currentScore);

        //// เช็คว่าถ้าคะแนนถึงเป้า และเกมยังรันอยู่ ให้สั่งจบเกม
        //if (isPlaying && currentScore >= maxScore)
        //{
        //    EndGame();
        //}
    }

    public void SetupFromRecipe(CookingRecipeSO recipe, int targetMaxScore, int cookCount)
    {
        if (recipe == null) return;

        maxScore = targetMaxScore;
        _rewardCount = cookCount; // จำค่าไว้ใช้ตอนจบ

        ingredientSprites.Clear();

        foreach (var ingredientData in recipe.ingredients)
        {
            if (ingredientData.item != null && ingredientData.item.ItemImage != null)
            {
                ingredientSprites.Add(ingredientData.item.ItemImage);
            }
        }

        if (recipe.resultItem != null)
        {
            _rewardSprite = recipe.resultItem.ItemImage;
        }

        AddScore(0);
    }

    // --- ฟังก์ชันสำหรับเริ่มเล่น Pattern ---
    public void PlayPattern(int patternIndex)
    {
        if (patternIndex >= 0 && patternIndex < gamePatterns.Count)
        {
            // ถ้ามี Pattern อื่นเล่นอยู่ให้หยุดก่อน
            if (currentPatternCoroutine != null) StopCoroutine(currentPatternCoroutine);
            if (currentAudioSequenceCoroutine != null) StopCoroutine(currentAudioSequenceCoroutine);
            if (currentSpawnSequenceCoroutine != null) StopCoroutine(currentSpawnSequenceCoroutine);

            // เริ่มเล่น Pattern ใหม่
            currentPatternCoroutine = StartCoroutine(PlayPatternRoutine(gamePatterns[patternIndex]));

            _currentPatternIndex = (_currentPatternIndex + 1) % gamePatterns.Count;
        }
    }

    public void PlayPattern()
    {
        // ถ้ามี Pattern อื่นเล่นอยู่ให้หยุดก่อน
        if (currentPatternCoroutine != null) StopCoroutine(currentPatternCoroutine);
        if (currentAudioSequenceCoroutine != null) StopCoroutine(currentAudioSequenceCoroutine);
        if (currentSpawnSequenceCoroutine != null) StopCoroutine(currentSpawnSequenceCoroutine);

        RhythmPattern pattern = new RhythmPattern(0.2f, "1");

        // เริ่มเล่น Pattern ใหม่
        currentPatternCoroutine = StartCoroutine(PlayPatternRoutine(pattern));
    }


    // --- ฟังก์ชันหลักที่กดแล้วเรียกทำงาน ---
    private System.Collections.IEnumerator PlayPatternRoutine(RhythmPattern pattern)
    {
        // 1. คำนวณความยาวของเสียงทั้งหมด (เวลาฟัง)
        float listenDuration = pattern.notes.Length * pattern.stepDuration;

        // 2. คำนวณเวลาเป้าหมายที่ชิ้นแรกต้องถึงกลางจอ
        float firstHitTime = listenDuration + responseDelay;

        // 3. คำนวณเวลาที่ต้องเริ่ม "เสก" ชิ้นแรก
        float firstSpawnTime = firstHitTime - timeToReachTarget;

        // เช็คเงื่อนไขตามที่คุณบอกเป๊ะๆ
        if (firstSpawnTime < 0)
        {
            // กรณีเวลาติดลบ: แปลว่าต้องเสกก่อนที่เสียงจะเริ่ม
            currentAudioSequenceCoroutine = StartCoroutine(AudioSequence(pattern)); // ค่อยเริ่มเล่นเสียง
            currentSpawnSequenceCoroutine = StartCoroutine(SpawnSequence(pattern)); // เริ่มเสกเลย
            //yield return new WaitForSeconds(Mathf.Abs(firstSpawnTime)); // รอให้เวลาติดลบผ่านไป
        }
        else
        {
            // กรณีปกติ: เสียงเริ่มก่อน
            currentAudioSequenceCoroutine = StartCoroutine(AudioSequence(pattern)); // เริ่มเสียงเลย
            yield return new WaitForSeconds(firstSpawnTime); // รอจังหวะ Timing
            currentAudioSequenceCoroutine = StartCoroutine(SpawnSequence(pattern)); // ค่อยเริ่มเสก
        }
    }

    // --- Coroutine สำหรับเล่นเสียงอย่างเดียว ---
    private System.Collections.IEnumerator AudioSequence(RhythmPattern pattern)
    {
        for (int i = 0; i < pattern.notes.Length; i++)
        {
            if (pattern.notes[i] == '1')
            {
                // ตะโกนบอกให้ SoundController เล่นเสียง!
                OnPlaySoundRhythm?.Invoke();
            }

            // รอจนกว่าจะถึงคิวตัวอักษรต่อไป
            yield return new WaitForSeconds(pattern.stepDuration);
        }
    }

    // --- Coroutine สำหรับโยนของอย่างเดียว ---
    private System.Collections.IEnumerator SpawnSequence(RhythmPattern pattern)
    {
        for (int i = 0; i < pattern.notes.Length; i++)
        {
            if (pattern.notes[i] == '1')
            {
                AddIngredient(); // โยนวัตถุดิบ
            }

            // รอจนกว่าจะถึงคิวตัวอักษรต่อไป (ระยะห่างเท่ากับเสียงเป๊ะๆ)
            yield return new WaitForSeconds(pattern.stepDuration);
        }
    }

    // --- ระบบ Game Loop ---

    public void StartGame()
    {
        if (isPlaying) return; // ถ้าเล่นอยู่แล้วไม่ต้องกดซ้ำ

        isPlaying = true;
        currentScore = 0;
        AddScore(0); // รีเซ็ตคะแนนและบอก UI

        // เริ่มลูปเกม
        gameLoopCoroutine = StartCoroutine(GameLoopRoutine());
    }

    public void EndGame()
    {
        isPlaying = false;

        if (gameLoopCoroutine != null) StopCoroutine(gameLoopCoroutine);
        if (currentPatternCoroutine != null) StopCoroutine(currentPatternCoroutine);
        if (currentAudioSequenceCoroutine != null) StopCoroutine(currentAudioSequenceCoroutine);
        if (currentSpawnSequenceCoroutine != null) StopCoroutine(currentSpawnSequenceCoroutine);

        // 4. ส่งค่า Count ออกไปพร้อมรูปภาพ!
        OnGameFinished?.Invoke(_rewardSprite, _rewardCount);
        Debug.Log(" Game Finished! Max Score Reached!");
    }

    private System.Collections.IEnumerator GameLoopRoutine()
    {
        // ลูปจะทำงานไปเรื่อยๆ ตราบใดที่ isPlaying เป็น true และคะแนนยังไม่ถึง
        while (isPlaying && currentScore < maxScore)
        {
            if (gamePatterns.Count == 0)
            {
                Debug.LogWarning("ไม่มี Pattern ให้เล่น! กรุณาเพิ่มใน Inspector");
                break;
            }

            // 1. สุ่ม Pattern จาก List
            int randomIndex = Random.Range(0, gamePatterns.Count);
            RhythmPattern currentPattern = gamePatterns[randomIndex];

            // 2. สั่งเล่น Pattern
            PlayPattern(randomIndex);

            // 3. คำนวณว่า Pattern นี้ต้องใช้เวลาเล่น "ทั้งหมด" นานเท่าไหร่
            // เวลาฟัง (Listen) + เวลาพัก (Response) + เวลาที่ใช้ฟันทั้งหมด (Action)
            float listenDuration = currentPattern.notes.Length * currentPattern.stepDuration;
            float actionDuration = currentPattern.notes.Length * currentPattern.stepDuration;

            // เผื่อเวลาให้วัตถุดิบชิ้นสุดท้ายลอยมาถึง และตกลงไปพ้นจอ (กันบั๊กเวลาเหลื่อม)
            float bufferTime = timeToReachTarget + 0.5f;

            float totalPatternTime = listenDuration + responseDelay + actionDuration ;

            // 4. รอให้ Pattern นี้เล่นจบสมบูรณ์
            yield return new WaitForSeconds(totalPatternTime);

            // เช็คว่าถ้าคะแนนถึงเป้า และเกมยังรันอยู่ ให้สั่งจบเกม
            if (isPlaying && currentScore >= maxScore)
            {
                //yield return new WaitForSeconds(1f);
                EndGame();
            }

            // 5. รอจังหวะพัก (Delay) ก่อนเริ่มสุ่ม Pattern ต่อไป
            if (isPlaying)
            {
                yield return new WaitForSeconds(delayBetweenPatterns);
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