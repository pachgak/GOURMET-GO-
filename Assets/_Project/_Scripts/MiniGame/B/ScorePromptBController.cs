using UnityEngine;
using TMPro; // อย่าลืมใส่บรรทัดนี้เพื่อเรียกใช้ TextMeshPro

public class ScorePromptBController : ScorePromptSpawnerBase
{
    [Header("Hit Prompt")]
    public RectTransform hitSpawnPos; // จุดกึ่งกลางที่จะให้ข้อความเด้ง (ลากกึ่งกลางหลอดต้มมาใส่)
    public float hitRandomRange = 30f; // ระยะสุ่มรอบๆ จุดเกิด
    public Color perfectColor = Color.yellow;
    public Color goodColor = Color.green;

    [Header("Ref")]
    private MiniGameBManager _miniGameBManager;

    private void Awake()
    {
        // หา Manager ของเกมต้มที่แปะอยู่บน GameObject เดียวกัน
        _miniGameBManager = GetComponent<MiniGameBManager>();
    }

    void OnEnable()
    {
        // ไปดักฟัง Event จาก Manager (ถ้า Manager ตะโกนว่า OnHitQualityEvaluated ให้รันฟังก์ชัน ShowHitText)
        if (_miniGameBManager != null)
        {
            _miniGameBManager.OnHitQualityEvaluated += ShowHitText;
        }
        else
        {
            Debug.LogWarning("ScorePromptBController: ไม่พบ MiniGameBManager บน GameObject นี้ครับ");
        }
    }

    void OnDisable()
    {
        // ยกเลิกดักฟังเมื่อปิดฉาก
        if (_miniGameBManager != null)
        {
            _miniGameBManager.OnHitQualityEvaluated -= ShowHitText;
        }
    }

    private void ShowHitText(MiniGameBManager.FireQuality quality, int scoreValue)
    {
        // ถ้าเป็น Miss ให้หยุดการทำงานไปเลย (ไม่ต้องเสก Prompt ตามที่คุณต้องการ)
        if (quality == MiniGameBManager.FireQuality.Miss)
        {
            return;
        }

        // จัดการเครื่องหมาย: ถ้าคะแนนมากกว่า 0 ให้ใส่ "+" นอกนั้นปล่อยว่าง
        string signText = (scoreValue > 0) ? "+" : "";

        string qualityText = "";
        Color promptColor = Color.white;

        switch (quality)
        {
            case MiniGameBManager.FireQuality.Perfect:
                qualityText = "Perfect";
                promptColor = perfectColor;
                break;
            case MiniGameBManager.FireQuality.Good:
                qualityText = "Good";
                promptColor = goodColor;
                break;
        }

        // รวมข้อความเข้าด้วยกัน (ใช้ \n เพื่อขึ้นบรรทัดใหม่)
        string promptText = $"{qualityText}\n{signText}{scoreValue}";

        // --- เรียกใช้ฟังก์ชันจากคลาสแม่ (ScorePromptSpawnerBase) ได้เลย! ---
        SpawnPrompt(promptText, promptColor, hitSpawnPos, hitRandomRange);
    }
}