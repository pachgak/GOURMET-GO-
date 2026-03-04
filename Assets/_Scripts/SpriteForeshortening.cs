using UnityEngine;

public class SpriteForeshortening : MonoBehaviour
{
    [Header("Camera Reference")]
    public Camera targetCamera;
    public Vector3 originalScale = Vector3.one;

    [Header("Check ForeshorteningParent")]
    [SerializeField] private Transform foreshorteningParent;

    void Start()
    {
        if (foreshorteningParent == null)
        {
            SetupForeshorteningParent();
        }
        ApplyForeshortening();
    }

    [ContextMenu("Setup & Apply Foreshortening")]
    public void ExecuteFromContextMenu()
    {
        if (foreshorteningParent == null)
        {
            SetupForeshorteningParent();
        }
        ApplyForeshortening();
    }

    private void SetupForeshorteningParent()
    {
        if (targetCamera == null) targetCamera = Camera.main;

        // 1. สร้าง GameObject เปล่าขึ้นมาใหม่
        GameObject parentObj = new GameObject("_ForeshorteningParent");
        foreshorteningParent = parentObj.transform;

        // 2. เก็บรายชื่อ Child เดิมทั้งหมดของ GraphicsScal เอาไว้ก่อนทำการย้าย
        int childCount = this.transform.childCount;
        Transform[] originalChildren = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            originalChildren[i] = this.transform.GetChild(i);
        }

        // 3. เอา Parent ใหม่ ไปเสียบเป็นลูกของ GraphicsScal (this.transform)
        foreshorteningParent.SetParent(this.transform, false);
        foreshorteningParent.localPosition = Vector3.zero;

        // 4. ย้าย Child เดิมทั้งหมด (พวก Sprite1, Sprite2) ไปอยู่ใต้กล่อง Parent ใหม่
        foreach (Transform child in originalChildren)
        {
            child.SetParent(foreshorteningParent, false);
        }
    }

    public void ApplyForeshortening()
    {
        if (targetCamera == null || foreshorteningParent == null) return;

        float cameraAngleX = targetCamera.transform.eulerAngles.x;
        //Debug.Log($"cameraAngleX : {cameraAngleX}");

        if (cameraAngleX >= 89f && cameraAngleX <= 91f) cameraAngleX = 89f;

        float cosTheta = Mathf.Cos(cameraAngleX * Mathf.Deg2Rad);
        float compensatedScaleY = originalScale.y * (1f / cosTheta);

        foreshorteningParent.localScale = new Vector3(originalScale.x, compensatedScaleY, originalScale.z);
    }
}