using UnityEngine;
using UnityEngine.UI;

public class LatchUIManager : MonoBehaviour
{
    public static LatchUIManager instance;

    [Header("UI Elements")]
    public GameObject latchPanel; // Panel หลัก (เอาไว้เปิด/ปิดทั้งหมด)
    public GameObject escapeProgressUI; // ตัวกรอบ UI ที่จะให้วิ่งตามผู้เล่น
    public Slider escapeProgressBar;

    [Header("Tracking Settings")]
    public Vector3 trackingOffset = new Vector3(0, 2f, 0); // ระยะความสูงเหนือหัวผู้เล่น (ปรับให้พอดีใน Inspector)

    private Transform _targetToTrack;
    private Camera _mainCamera;

    private void Awake()
    {
        if (instance == null) instance = this;

        _mainCamera = Camera.main; // หา Camera หลักในฉาก
        latchPanel.SetActive(false);
    }

    private void Update()
    {
        // ถ้าเปิด UI อยู่ และมีเป้าหมายให้ตาม
        if (latchPanel.activeSelf && _targetToTrack != null)
        {
            // แปลงพิกัดจาก World Space (ตำแหน่งในฉาก 3D) เป็น Screen Space (ตำแหน่งบนจอ 2D)
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(_targetToTrack.position + trackingOffset);

            // ถ้าค่า Z มากกว่า 0 แปลว่าผู้เล่นอยู่ "ข้างหน้า" กล้อง (ไม่หลุดไปอยู่หลังกล้อง)
            if (screenPos.z > 0)
            {
                escapeProgressUI.SetActive(true);
                // ขยับ UI ไปที่ตำแหน่งที่คำนวณได้
                escapeProgressUI.transform.position = screenPos;
            }
            else
            {
                // ถ้ากล้องหันหนีผู้เล่น (ผู้เล่นอยู่หลังกล้อง) ให้ซ่อน UI ไว้ชั่วคราว
                escapeProgressUI.SetActive(false);
            }
        }
    }

    // *** อัปเดต: เพิ่มการรับค่า Transform ของผู้เล่นเข้ามาด้วย ***
    public void ShowLatchUI(float maxHits, Transform playerTransform) // เปลี่ยนเป็น float
    {
        _targetToTrack = playerTransform;
        latchPanel.SetActive(true);
        escapeProgressBar.maxValue = maxHits;
        escapeProgressBar.value = 0;
    }

    // เรียกตอนผู้เล่นคลิกเมาส์
    public void UpdateProgress(float currentHits) // เปลี่ยนเป็น float
    {
        escapeProgressBar.value = currentHits;
    }

    // เรียกตอนไก่หลุด
    public void HideLatchUI()
    {
        _targetToTrack = null; // ล้างค่าเป้าหมาย
        latchPanel.SetActive(false);
    }
}