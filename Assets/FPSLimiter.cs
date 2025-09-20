using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    void Awake()
    {
        // กำหนด Target Frame Rate ที่ต้องการ
        Application.targetFrameRate = 120; // หรือค่าที่คุณต้องการ เช่น 30, 90, 120
    }
}