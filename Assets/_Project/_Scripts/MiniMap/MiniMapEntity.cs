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

    [Header("Visibility Sync (Optional)")]
    [Tooltip("ลาก SpriteRenderer มาใส่ถ้าอยากให้ Icon หายไปตอนที่ล่องหน (ไม่ต้องใส่ก็ได้)")]
    public SpriteRenderer targetRenderer;

    [SerializeField] private RectTransform myIcon;

    private void OnEnable()
    {
        // 1. ผูก Event
        if (TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
        {
            enemyHealth.OnDie += Disable;
        }
        if (TryGetComponent<FoodTree>(out FoodTree foodTree))
        {
            foodTree.OnPick += Disable;
        }

        // 2. พยายามสร้าง Icon ทุกครั้งที่ถูกเปิดใช้งาน (SetActive = true)
        TryCreateIcon();
    }

    private void Start()
    {
        // เผื่อตอนเริ่มเกม OnEnable ทำงานก่อนที่ CornerMapManager.Awake จะทำงานเสร็จ
        if (myIcon == null)
        {
            TryCreateIcon();
        }
    }

    private void OnDisable()
    {
        Disable();

        // เอา Event ออก
        if (TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
        {
            enemyHealth.OnDie -= Disable;
        }
        if (TryGetComponent<FoodTree>(out FoodTree foodTree))
        {
            foodTree.OnPick -= Disable;
        }
    }

    void Update()
    {
        if (myIcon == null || CornerMapManager.Instance == null) return;

        // --- เพิ่มเติม: เช็คการมองเห็นของ SpriteRenderer ---
        if (targetRenderer != null)
        {
            // จะมองเห็นก็ต่อเมื่อ GameObject เปิดอยู่ และ Component Renderer ถูก Enable
            bool isVisible = targetRenderer.gameObject.activeInHierarchy && targetRenderer.enabled;

            // สั่งเปิด/ปิด Icon ให้ตรงกับ Renderer (เช็คก่อนค่อย SetActive เพื่อไม่ให้ UI คำนวณใหม่ทุกเฟรมจนกระตุก)
            if (myIcon.gameObject.activeSelf != isVisible)
            {
                myIcon.gameObject.SetActive(isVisible);
            }

            // ถ้าล่องหนอยู่ ก็ไม่ต้องให้มันคำนวณตำแหน่งและสเกลต่อให้เปลือง CPU
            if (!isVisible) return;
        }
        // --------------------------------------------

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

    void OnDestroy()
    {
        Disable();
    }

    public void Disable()
    {
        if (myIcon != null)
        {
            Destroy(myIcon.gameObject);
            myIcon = null; // สำคัญมาก: ต้องเคลียร์เป็น null
        }
    }

    private void TryCreateIcon()
    {
        // ถ้ามี Icon อยู่แล้ว หรือ Manager ยังไม่มีในฉาก ให้ข้ามไป
        if (myIcon != null || CornerMapManager.Instance == null) return;

        // เรียกให้ Controller สร้าง Icon ให้หน่อย พร้อมส่งรูปและสีไป
        myIcon = CornerMapManager.Instance.CreateIcon(iconSprite, iconColor);

        if (myIcon != null && isStatic)
        {
            UpdatePosition();
        }
    }
}