using UnityEngine;

public class MiniMapEntity : MonoBehaviour
{
    [Header("Icon Setup (ไม่ต้องลาก Prefab แล้ว!)")]
    [Tooltip("ลากรูป Sprite ที่อยากให้แสดง (เช่น จุดกลม รูปหัวกะโหลก ใบไม้)")]
    public Sprite iconSprite;
    [Tooltip("สีของ Icon (เผื่อใช้ Sprite สีขาว แล้วค่อยมาเปลี่ยนสีตรงนี้เอา)")]
    public Color iconColor = Color.white;

    [Header("Entity Type")]
    [Tooltip("ติ๊กถูกถ้าเป็นต้นไม้ หรือของที่ไม่ขยับ")]
    public bool isStatic = false;

    private RectTransform myIcon;

    void Start()
    {
        if (CornerMapManager.Instance == null) return;

        // เรียกให้ Controller สร้าง Icon ให้หน่อย พร้อมส่งรูปและสีไป
        myIcon = CornerMapManager.Instance.CreateIcon(iconSprite, iconColor);

        if (myIcon != null && isStatic)
        {
            UpdatePosition();
        }
    }

    void Update()
    {
        if (myIcon == null || CornerMapManager.Instance == null) return;

        if (!isStatic)
        {
            UpdatePosition();
        }

        // ปรับขนาดสู้ซูม (Counter-Scale)
        float fixedScale = 1f / CornerMapManager.Instance.zoomLevel;
        myIcon.localScale = new Vector3(fixedScale, fixedScale, 1f);
    }

    void UpdatePosition()
    {
        myIcon.anchoredPosition = CornerMapManager.Instance.GetMapPosition(transform.position);
    }

    private void OnDisable()
    {
        Disable();
    }

    void OnDestroy()
    {
        Disable();
    }

    public void Disable()
    {
        if (myIcon != null) Destroy(myIcon.gameObject);
    }
}