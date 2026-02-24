using UnityEngine;
using UnityEngine.UI;

public class CornerMapManager : MonoBehaviour
{
    public static CornerMapManager Instance;

    [Header("UI References")]
    public RectTransform mapImage;
    public RectTransform playerIcon;
    public Transform playerTransform;

    [Header("Icon Settings")]
    [Tooltip("ใส่ Prefab กลางที่มีแค่ UI Image เปล่าๆ (ตั้งชื่อว่า DefaultIcon)")]
    public GameObject baseIconPrefab;

    [Header("Map Settings")]
    public Vector2 worldSize = new Vector2(200, 200);
    public Vector2 worldCenter = Vector2.zero;

    [Header("Zoom Settings")]
    public float zoomLevel = 1f;
    public float minZoom = 0.5f;
    public float maxZoom = 3f;
    public float zoomSpeed = 0.5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if (playerTransform == null || mapImage == null) return;

        //// ซูม
        //float scroll = Input.GetAxis("Mouse ScrollWheel");
        //if (scroll != 0f)
        //{
        //    zoomLevel += scroll * zoomSpeed;
        //    zoomLevel = Mathf.Clamp(zoomLevel, minZoom, maxZoom);
        //}

        //// ปรับสเกลภาพแผนที่
        //mapImage.localScale = new Vector3(zoomLevel, zoomLevel, 1f);

        // หาพิกัดและเลื่อนแผนที่
        Vector2 playerUIPos = GetMapPosition(playerTransform.position);
        mapImage.anchoredPosition = -playerUIPos * zoomLevel;

        // หมุนไอคอนผู้เล่น
        if (playerIcon != null)
        {
            playerIcon.localRotation = Quaternion.Euler(0, 0, -playerTransform.eulerAngles.y);
        }
    }

    // ฟังก์ชันคำนวณตำแหน่ง
    public Vector2 GetMapPosition(Vector3 worldPos)
    {
        float normalizedX = (worldPos.x - worldCenter.x) / worldSize.x;
        float normalizedZ = (worldPos.z - worldCenter.y) / worldSize.y;

        float uiX = normalizedX * mapImage.rect.width;
        float uiY = normalizedZ * mapImage.rect.height;

        return new Vector2(uiX, uiY);
    }

    // ?? [เพิ่มใหม่] ฟังก์ชันสำหรับให้ Entity เรียกเพื่อสร้าง Icon
    public RectTransform CreateIcon(Sprite customSprite, Color colorTint)
    {
        if (baseIconPrefab == null || mapImage == null) return null;

        // สร้างจาก Prefab กลาง ให้อยู่ใต้ mapImage ทันที
        GameObject newIcon = Instantiate(baseIconPrefab, mapImage);

        // เข้าถึง Image Component เพื่อเปลี่ยนรูปและสีตามที่ขอมา
        Image img = newIcon.GetComponent<Image>();
        if (img != null)
        {
            if (customSprite != null) img.sprite = customSprite; // เปลี่ยนรูป
            img.color = colorTint;                               // เปลี่ยนสี (เช่น เปลี่ยนจุดขาวเป็นจุดแดง/เขียว)
        }

        return newIcon.GetComponent<RectTransform>();
    }
}