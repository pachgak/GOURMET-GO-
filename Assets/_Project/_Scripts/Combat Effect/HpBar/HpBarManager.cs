using UnityEngine;

public class HpBarManager : MonoBehaviour
{
    // สร้างเป็น Singleton เพื่อให้สคริปต์อื่นเรียกใช้ได้จากทุกที่ผ่าน HpBarManager.Instance
    public static HpBarManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Prefab ของหลอดเลือดมอนสเตอร์ทั่วไป")]
    public EnemyBarUI enemyBarUIPrefab;

    [Tooltip("Canvas ที่ตั้งค่าเป็น World Space สำหรับให้หลอดเลือดมอนสเตอร์ไปเกาะ")]
    public Canvas canvasWorldParent;

    [Tooltip("UI หลอดเลือดบอสที่มีอยู่ใน Scene อยู่แล้ว")]
    public BossBarUI bossBarUI;

    private void Awake()
    {
        // ตรวจสอบว่ามี Manager ตัวอื่นอยู่ใน Scene ไหม
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // ป้องกันไม่ให้มี Manager ซ้ำซ้อน
        }
    }
}