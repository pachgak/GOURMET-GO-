using UnityEngine;
using TMPro;

public class ScorePromptUI : MonoBehaviour
{
    [Header("References")]
    public TMP_Text textUI; // ลาก TextMeshProUGUI ใน Prefab มาใส่ช่องนี้

    [Header("Animation Settings")]
    public float moveSpeed = 100f; // ความเร็วในการลอยขึ้น
    public float lifetime = 1.0f;  // เวลาทั้งหมดก่อนจะหายไป (วินาที)

    private RectTransform _rectTransform;
    private Color _textColor;
    private float _fadeTimer;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        //if(textUI == null) textUI = gameObject.AddComponent<TMP_Text>();
    }

    // ฟังก์ชันนี้ Controller จะเป็นคนเรียกเพื่อตั้งค่าข้อความและสี
    public void Setup(string text, Color color)
    {
        textUI.text = text;
        textUI.color = color;
        _textColor = color;
        _fadeTimer = lifetime;

        // ตั้งเวลาทำลายตัวเองล่วงหน้าไว้เลย
        Destroy(gameObject, lifetime);
    }

    void Update()
    {

        // 1. ทำให้เลื่อนลอยขึ้นไปข้างบน
        _rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;

        // 2. ทำให้ค่อยๆ จางหายไป (ลดค่า Alpha)
        _fadeTimer -= Time.deltaTime;

        // คำนวณค่าความโปร่งใส (Alpha) จาก 1 -> 0
        float alpha = Mathf.Clamp01(_fadeTimer / lifetime);

        _textColor.a = alpha;
        textUI.color = _textColor;
    }
}