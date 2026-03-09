using UnityEngine;

public class SnapToGridManager : MonoBehaviour
{
    // สร้าง Singleton Instance เพื่อให้สคริปต์อื่นเรียกใช้ได้ง่ายๆ
    public static SnapToGridManager instance;

    [Tooltip("ลาก Grid หลักของฉากมาใส่ที่นี่เพียงจุดเดียว")]
    public Grid targetGrid;

    private void Awake()
    {
        // Setup Singleton
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
}