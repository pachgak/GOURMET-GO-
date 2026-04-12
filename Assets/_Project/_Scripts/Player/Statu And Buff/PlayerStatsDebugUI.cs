using UnityEngine;
using TMPro; // จำเป็นต้องใช้สำหรับ TextMeshPro

public class PlayerStatsDebugUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("ลาก Player หรือ Object ที่มี PlayerStats มาใส่")]
    public PlayerStats playerStats;

    [Header("Debug Texts")]
    public TMP_Text moveSpeedText;
    public TMP_Text attackPowerText;
    public TMP_Text maxHealthText;
    public TMP_Text dashRangeText;

    private void Update()
    {
        // ถ้าหา PlayerStats ไม่เจอ ให้หยุดทำงานเพื่อป้องกัน Error
        if (playerStats == null) return;

        // ดึงค่า Multiplier มาแสดง โดยใช้ :F2 เพื่อให้แสดงทศนิยมแค่ 2 ตำแหน่ง (เช่น 1.20)
        if (moveSpeedText != null)
        {
            moveSpeedText.text = $"Move Speed : x{playerStats.moveSpeed.GetMultiplier():F2}";
        }

        if (attackPowerText != null)
        {
            attackPowerText.text = $"Attack Power : x{playerStats.attackPower.GetMultiplier():F2}";
        }

        if (maxHealthText != null)
        {
            maxHealthText.text = $"Max Health : x{playerStats.maxHealth.GetMultiplier():F2}";
        }

        if (dashRangeText != null)
        {
            dashRangeText.text = $"Dash Range : x{playerStats.dashRang.GetMultiplier():F2}";
        }
    }
}