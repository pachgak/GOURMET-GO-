using UnityEngine;

public class SpriteForeshortening : MonoBehaviour
{
    [Header("Camera Reference")]
    public Camera targetCamera;
    public Vector3 originalScale = Vector3.one;

    private Transform foreshorteningParent;

    void Start()
    {
        // ใช้โลจิกเดียวกันกับ ContextMenu เพื่อป้องกันการสร้าง Parent ซ้ำซ้อน
        if (foreshorteningParent == null)
        {
            SetupForeshorteningParent();
        }
        ApplyForeshortening();
    }

    // --- เพิ่ม Context Menu ตรงนี้ ---
    // คุณสามารถคลิกขวาที่คอมโพเนนต์ SpriteForeshortening ใน Inspector แล้วเลือกคำสั่งนี้ได้เลย
    [ContextMenu("Setup & Apply Foreshortening")]
    public void ExecuteFromContextMenu()
    {
        // ถ้ายังไม่มี Parent ให้สร้างก่อน
        if (foreshorteningParent == null)
        {
            SetupForeshorteningParent();
        }

        // จากนั้นค่อยทำการคำนวณและปรับสเกล
        ApplyForeshortening();
    }

    private void SetupForeshorteningParent()
    {
        if (targetCamera == null) targetCamera = Camera.main;

        // สร้าง GameObject เปล่าขึ้นมาใหม่
        GameObject parentObj = new GameObject(gameObject.name + "_ForeshorteningParent");
        foreshorteningParent = parentObj.transform;

        // เอา Parent ใหม่ ไปอยู่ใต้ Parent เดิมของ Graphics
        foreshorteningParent.SetParent(this.transform.parent, false);
        foreshorteningParent.localPosition = this.transform.localPosition;

        // ย้ายตัว Graphics (this) ไปเป็นลูกของ Parent ใหม่
        this.transform.SetParent(foreshorteningParent, false);
        this.transform.localPosition = Vector3.zero;
    }

    public void ApplyForeshortening()
    {
        if (targetCamera == null || foreshorteningParent == null) return;

        float cameraAngleX = targetCamera.transform.eulerAngles.x;

        // ดักจับกรณีมุม 90 องศา
        if (cameraAngleX >= 89f && cameraAngleX <= 91f) cameraAngleX = 89f;

        float cosTheta = Mathf.Cos(cameraAngleX * Mathf.Deg2Rad);
        float compensatedScaleY = originalScale.y * (1f / cosTheta);

        // สั่งแก้สเกลที่ Parent ตัวใหม่
        foreshorteningParent.localScale = new Vector3(originalScale.x, compensatedScaleY, originalScale.z);
    }
}