using UnityEngine;

public class SpriteForeshortening : MonoBehaviour
{
    [Header("Camera Reference")]
    public Camera targetCamera;
    public Vector3 originalScale = Vector3.one;

    private Transform foreshorteningParent;

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

        // --- 1. เก็บตำแหน่ง Index (ลำดับ Child) เดิมของตัว Graphics เอาไว้ก่อน ---
        int originalSiblingIndex = this.transform.GetSiblingIndex();

        GameObject parentObj = new GameObject(gameObject.name + "_ForeshorteningParent");
        foreshorteningParent = parentObj.transform;

        foreshorteningParent.SetParent(this.transform.parent, false);
        foreshorteningParent.localPosition = this.transform.localPosition;

        // --- 2. สั่งให้ Parent ใหม่ ไปแทรกอยู่ใน Index เดิมที่เราเก็บไว้ ---
        foreshorteningParent.SetSiblingIndex(originalSiblingIndex);

        this.transform.SetParent(foreshorteningParent, false);
        this.transform.localPosition = Vector3.zero;
    }

    public void ApplyForeshortening()
    {
        if (targetCamera == null || foreshorteningParent == null) return;

        float cameraAngleX = targetCamera.transform.eulerAngles.x;

        if (cameraAngleX >= 89f && cameraAngleX <= 91f) cameraAngleX = 89f;

        float cosTheta = Mathf.Cos(cameraAngleX * Mathf.Deg2Rad);
        float compensatedScaleY = originalScale.y * (1f / cosTheta);

        foreshorteningParent.localScale = new Vector3(originalScale.x, compensatedScaleY, originalScale.z);
    }
}