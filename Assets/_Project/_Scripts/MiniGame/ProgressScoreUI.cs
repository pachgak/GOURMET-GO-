using UnityEngine;
using TMPro; // จำเป็นสำหรับการใช้ UI ของ TextMeshPro

public class ProgressScoreUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text progressText;
    public TMP_Text percentText; // ขออนุญาตแก้ตัวสะกดเป็น percentText นะครับ

    [Header("Ref")]
    private MiniGameBase _miniGameBase;

    private void Awake()
    {
        _miniGameBase = GetComponent<MiniGameBase>();
    }

    private void OnEnable()
    {
        // สมมติว่าใน Manager คุณมี Action<int> OnScoreUpdated อยู่แล้ว
        if (_miniGameBase != null)
        {
            _miniGameBase.OnScoreUpdated += HandleCurrentScoreChange;
        }
    }

    private void OnDisable()
    {
        // อย่าลืม Unsubscribe เสมอ
        if (_miniGameBase != null)
        {
            _miniGameBase.OnScoreUpdated -= HandleCurrentScoreChange;
        }
    }

    // ฟังก์ชันนี้จะถูกเรียกอัตโนมัติเมื่อคะแนนเปลี่ยน
    private void HandleCurrentScoreChange(int currentScore)
    {
        int maxScore = _miniGameBase.maxScore;

        // 1. อัปเดต Progress (เช่น Progress : 5/10)
        if (progressText != null)
        {
            progressText.text = $"Progress : {currentScore}/{maxScore}";
        }

        // 2. คำนวณและอัปเดต Percent (%)
        if (percentText != null)
        {
            // ป้องกัน Error หารด้วย 0 กรณีที่เผลอตั้ง maxScore เป็น 0 ใน Inspector
            if (maxScore > 0)
            {
                // แปลงเป็น float ก่อนหาร เพื่อให้ได้ทศนิยม แล้วคูณ 100
                float percent = ((float)currentScore / maxScore) * 100f;

                // .ToString("F0") หมายถึง แปลงเป็นตัวหนังสือโดยเอาทศนิยม 0 ตำแหน่ง (เช่น 50%)
                // ถ้าอยากได้ทศนิยม 1 ตำแหน่งให้ใช้ "F1" (เช่น 50.5%)
                percentText.text = $"{percent.ToString("F0")} %";
            }
            else
            {
                percentText.text = "0 %";
            }
        }
    }
}