using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("References")]
    [Tooltip("ลาก Object Player ที่มีสคริปต์ PlayerStats มาใส่")]
    public PlayerHealth playerHel;

    [Header("Death UI")]
    public GameObject failPanel;
    public Button returnToBaseButton;

    [Header("Respawn Events")]
    public UnityEvent onPlayerRespawn;

    private void Start()
    {
        if (failPanel != null) failPanel.SetActive(false);

        if (returnToBaseButton != null)
        {
            returnToBaseButton.onClick.AddListener(RespawnPlayer);
        }

        // *** 3. สมัครรับ Event การตายจาก PlayerHealth ***
        if (playerHel != null)
        {
            playerHel.OnDie += HandlePlayerDeath;
        }
        else
        {
            Debug.LogError("[DeathHandler] ไม่ได้ใส่ Reference ของ PlayerHealth!");
        }
    }

    // *** ยกเลิกการสมัครเมื่อ Object ถูกทำลาย เพื่อป้องกันบั๊ก Memory Leak ***
    private void OnDestroy()
    {
        if (playerHel != null)
        {
            playerHel.OnDie -= HandlePlayerDeath;
        }
    }

    // ฟังก์ชันนี้จะทำงานอัตโนมัติเมื่อ playerHel.OnDie ถูกเรียก
    public void HandlePlayerDeath()
    {
        Debug.Log("[DeathHandler] ได้รับสัญญาณ OnDie: กำลังเปิดหน้า Fail Panel");

        if (PlayerInputActionsManager.instance != null)
        {
            // *** แก้ตรงนี้: ปิดแค่หมวด Player เพื่อให้ยังใช้เมาส์คลิก UI ได้! ***
            PlayerInputActionsManager.instance.playerControls.Player.Disable();
        }

        if (failPanel != null)
        {
            failPanel.SetActive(true);
        }
    }

    private void RespawnPlayer()
    {
        if (failPanel != null) failPanel.SetActive(false);

        // 1. ฮีลเลือดและรีเซ็ตสถานะ isDead จากสคริปต์คุณโดยตรง (รวดเร็ว ชัวร์สุด!)
        playerHel.RestoreMaxHP();

        // 2. เรียก Event เผื่อมีเอฟเฟกต์อื่นๆ ที่อยากลากมาใส่ใน Inspector ทีหลัง
        onPlayerRespawn?.Invoke();

        // 3. สั่ง GameLoop วาปกลับฐาน (GameLoop จะเคลียร์มอนและ Enable Input คืนให้เอง)
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.ReturnToBase();
        }
    }
}