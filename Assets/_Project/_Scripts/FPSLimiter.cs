using com.cyborgAssets.inspectorButtonPro;
using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    public int setFps = 120; 
    void Awake()
    {
        // กำหนด Target Frame Rate ที่ต้องการ
        SetFps(); // หรือค่าที่คุณต้องการ เช่น 30, 90, 120
    }

    [ProButton]
    public void SetFps()
    {
        Application.targetFrameRate = setFps;
    }
}