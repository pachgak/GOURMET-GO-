using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffUIItem : MonoBehaviour
{
    [Header("UI References")]
    public Image buffIcon;
    public TMP_Text durationText;
    public TMP_Text stackText; // เผื่ออนาคตคุณมีบัพแบบเก็บ Stack 

    public void Setup(Sprite icon, float duration, int stack)
    {
        if (buffIcon != null) buffIcon.sprite = icon;
        UpdateUI(duration, stack);
    }

    public void UpdateUI(float duration, int stack)
    {
        // อัปเดตเวลา (แปลงเป็น นาที:วินาที หรือ วินาที)
        if (durationText != null)
        {
            int minutes = Mathf.FloorToInt(duration / 60F);
            int seconds = Mathf.FloorToInt(duration - minutes * 60);

            if (minutes > 0)
                durationText.text = string.Format("{0}:{1:00}", minutes, seconds); // เช่น 2:05
            else
                durationText.text = seconds.ToString() + "s"; // เช่น 45s
        }

        // อัปเดตจำนวน Stack (ถ้ามีแค่ 1 ไม่ต้องโชว์เลข)
        if (stackText != null)
        {
            stackText.text = stack > 1 ? stack.ToString() : "";
        }
    }
}