using UnityEngine;
using TMPro; // อย่าลืมใส่บรรทัดนี้เพื่อเรียกใช้ TextMeshPro

public class ScorePromptFController : ScorePromptSpawnerBase
{
    //public ScorePromptUI prefabScorePrompt; // Prefab ข้อความที่จะเสก
    [Header("Hit Prompt")]
    public RectTransform hitSpawnPos; // จุดกึ่งกลางที่จะให้ข้อความเด้ง
    public float hitRandomRange = 50f; // ระยะสุ่มรอบๆ จุดเกิด (ขออนุญาตแก้ตัวสะกดจาก randonRang นะครับ)
    public Color perfectColor = Color.yellow;
    public Color goodColor = Color.green;
    public Color badColor = Color.gray;

    [Header("Miss Prompt")]
    public RectTransform missSpawnPos; // จุดกึ่งกลางที่จะให้ข้อความเด้ง
    public float missRandomRange = 50f; // ระยะสุ่มรอบๆ จุดเกิด (ขออนุญาตแก้ตัวสะกดจาก randonRang นะครับ)
    public Color missColor = Color.red;

    [Header("Ref")]
    private MiniGameFManager _miniGameFManager;
    private void Awake()
    {
        _miniGameFManager = GetComponent<MiniGameFManager>();
    }

    void OnEnable()
    {
        // ไปดักฟัง Event จาก Manager (ถ้า Manager ตะโกนว่า OnHitEvaluated ให้รันฟังก์ชัน ShowHitText)
        if (_miniGameFManager != null)
        {
            _miniGameFManager.OnHitEvaluated += ShowHitText;
            _miniGameFManager.OnIngredientDropped += ShowMissText;
        }
        else Debug.Log("MiniGameFManager.Instance == null");
    }

    void OnDisable()
    {
        // ยกเลิกดักฟังเมื่อปิดฉาก
        if (_miniGameFManager != null)
        {
            _miniGameFManager.OnHitEvaluated -= ShowHitText;
            _miniGameFManager.OnIngredientDropped -= ShowMissText;
        }
    }

    private void ShowHitText(MiniGameFManager.HitQuality quality, int scoreValue)
    {
        // จัดการเครื่องหมาย: ถ้าคะแนนมากกว่า 0 ให้ใส่ "+" นอกนั้นปล่อยว่าง (เพราะตัวเลขติดลบมันมี "-" ของมันอยู่แล้ว)
        string signText = (scoreValue > 0) ? "+" : "";

        string qualityText = "";
        Color promptColor = Color.white;

        switch (quality)
        {
            case MiniGameFManager.HitQuality.Perfect:
                qualityText = "Perfect";
                promptColor = perfectColor;
                break;
            case MiniGameFManager.HitQuality.Good:
                qualityText = "Good";
                promptColor = goodColor;
                break;
            case MiniGameFManager.HitQuality.Bad:
                qualityText = "Bad";
                promptColor = badColor;
                break;
        }

        // รวมข้อความเข้าด้วยกัน (ใช้ \n เพื่อขึ้นบรรทัดใหม่)
        string promptText = $"{qualityText}\n{signText}{scoreValue}";

        // --- เรียกใช้ฟังก์ชันจากคลาสแม่ได้เลย! ---
        SpawnPrompt(promptText, promptColor, hitSpawnPos, hitRandomRange);

        //// --- 1. สร้าง Prefab ให้อยู่ภายใต้ spawnPos ---
        //GameObject promptObj = Instantiate(prefabScorePrompt.gameObject, hitSpawnPos.transform);

        //// --- 2. สุ่มตำแหน่งรอบๆ จุดกึ่งกลาง ---
        //RectTransform promptRect = promptObj.GetComponent<RectTransform>();
        //promptRect.anchoredPosition = new Vector2(
        //    Random.Range(-hitRandomRange, hitRandomRange),
        //    Random.Range(-hitRandomRange, hitRandomRange)
        //);

        //// --- 3. ส่งข้อมูลไปให้สคริปต์ HitQualityPrompt ที่อยู่บน Prefab จัดการต่อ ---
        //ScorePromptUI promptScript = promptObj.GetComponent<ScorePromptUI>();
        //if (promptScript != null)
        //{
        //    promptScript.Setup(promptText, promptColor);
        //}
    }

    private void ShowMissText(int scoreValue)
    {
        // จัดการเครื่องหมาย: ถ้าคะแนนมากกว่า 0 ให้ใส่ "+" นอกนั้นปล่อยว่าง (เพราะตัวเลขติดลบมันมี "-" ของมันอยู่แล้ว)
        string signText = (scoreValue > 0) ? "+" : "";
        // กำหนดข้อความ (สมมติว่าตกจอแล้วโดนหัก 1 คะแนน)
        string promptText = $"Miss\n{signText}{scoreValue}";
        //string promptText = "Miss!\n-1";

        // --- สร้างและจัดตำแหน่ง Prefab ---
        SpawnPrompt(promptText, missColor, missSpawnPos, missRandomRange);
        //GameObject promptObj = Instantiate(prefabScorePrompt.gameObject, missSpawnPos.transform);
        //RectTransform promptRect = promptObj.GetComponent<RectTransform>();

        //promptRect.anchoredPosition = new Vector2(
        //    Random.Range(-missRandomRange, missRandomRange),
        //    Random.Range(-missRandomRange, missRandomRange)
        //);

        //// --- โยนข้อความและสีไปให้กิริยา (HitQualityPrompt) จัดการต่อ ---
        //ScorePromptUI promptScript = promptObj.GetComponent<ScorePromptUI>();
        //if (promptScript != null)
        //{
        //    promptScript.Setup(promptText, missColor);
        //}
    }
}