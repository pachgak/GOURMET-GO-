using UnityEngine;

public class SpriteForeshortening : MonoBehaviour
{
    public Vector3 originalScale = Vector3.one;

    [Header("Check ForeshorteningParent")]
    [SerializeField] private Transform foreshorteningParent;

    void Start()
    {
        if (foreshorteningParent == null)
        {
            SetupForeshorteningParent();
        }

        // ดึงค่าเริ่มต้นมาใช้ทันทีตอนเกิด เผื่อมุมกล้องเซ็ตไว้แล้ว
        if (SpriteForeshorteningManager.instance != null)
        {
            ApplyForeshortening(SpriteForeshorteningManager.instance.GetCurrentMultiplier());
        }
    }

    private void OnEnable()
    {
        // สมัครรับ Event จาก Manager (ทำงานตอน Play Mode)
        if (SpriteForeshorteningManager.instance != null)
        {
            SpriteForeshorteningManager.instance.OnForeshorteningUpdated += ApplyForeshortening;
        }
    }

    private void OnDisable()
    {
        // ยกเลิกการรับ Event ป้องกัน Error ตอนลบ Object
        if (SpriteForeshorteningManager.instance != null)
        {
            SpriteForeshorteningManager.instance.OnForeshorteningUpdated -= ApplyForeshortening;
        }
    }

    // =========================================================
    // โหมด Editor (จัดฉาก)
    // =========================================================
    [ContextMenu("Setup & Apply Foreshortening")]
    public void ExecuteFromContextMenu()
    {
        if (foreshorteningParent == null)
        {
            SetupForeshorteningParent();
        }

        // ค้นหากล้องหลักด้วยตัวเอง เพราะตอน Editor Mode Manager อาจยังไม่ทำงาน
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("ไม่พบ Camera.main ในฉาก ไม่สามารถพรีวิว Foreshortening ได้");
            return;
        }

        // คำนวณ Multiplier สดๆ เพื่อพรีวิวในโหมดจัดฉาก
        float cameraAngleX = mainCam.transform.eulerAngles.x;
        if (cameraAngleX >= 89f && cameraAngleX <= 91f) cameraAngleX = 89f;

        float cosTheta = Mathf.Cos(cameraAngleX * Mathf.Deg2Rad);
        float previewMultiplier = 1f / cosTheta;

        ApplyForeshortening(previewMultiplier);
        
        Debug.Log($"<color=green>จัดฉาก Foreshortening เรียบร้อย! (มุมกล้อง: {cameraAngleX})</color>");
    }

    // =========================================================

    private void SetupForeshorteningParent()
    {
        // 1. สร้าง GameObject เปล่าขึ้นมาใหม่
        GameObject parentObj = new GameObject("_ForeshorteningParent");
        foreshorteningParent = parentObj.transform;

        // 2. เก็บรายชื่อ Child เดิมทั้งหมด
        int childCount = this.transform.childCount;
        Transform[] originalChildren = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            originalChildren[i] = this.transform.GetChild(i);
        }

        // 3. เอา Parent ใหม่ ไปเสียบเป็นลูก
        foreshorteningParent.SetParent(this.transform, false);
        foreshorteningParent.localPosition = Vector3.zero;

        // 4. ย้าย Child เดิมทั้งหมด ไปอยู่ใต้กล่อง Parent ใหม่
        foreach (Transform child in originalChildren)
        {
            child.SetParent(foreshorteningParent, false);
        }
    }

    // ฟังก์ชันนี้รับค่ามาจาก Event ของ Manager (ตอนเล่นเกม) หรือรับค่าจากการคลิกขวา (ตอนจัดฉาก)
    public void ApplyForeshortening(float multiplier)
    {
        if (foreshorteningParent == null) return;

        // เอา originalScale.y มาคูณกับตัวคูณที่รับมาได้เลย ไม่ต้องคำนวณ Cosine ซ้ำในโหมดปกติ
        float compensatedScaleY = originalScale.y * multiplier;

        foreshorteningParent.localScale = new Vector3(originalScale.x, compensatedScaleY, originalScale.z);
    }
}