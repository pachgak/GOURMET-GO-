using UnityEngine;

public class FullMapManager : MonoBehaviour
{
    public static FullMapManager Instance;

    [Header("UI References")]
    [Tooltip("ภาพแผนที่ (อยู่นิ่งๆ ขยายเต็มจอ)")]
    public RectTransform mapContainer;
    [Tooltip("ไอคอนผู้เล่น (ตัวนี้จะวิ่งไปมาบนแผนที่)")]
    public RectTransform playerIcon;
    [Tooltip("ตัวละคร 3D ของเรา")]
    public Transform playerTransform;

    [Header("Map Settings")]
    public Vector2 worldSize = new Vector2(200, 200);
    public Vector2 worldCenter = Vector2.zero;

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
        if (playerTransform == null || mapContainer == null || playerIcon == null) return;

        // อัปเดตตำแหน่งไอคอนวิ่งบนแผนที่ และหมุนตามทิศทาง
        playerIcon.anchoredPosition = GetMapPosition(playerTransform.position);
        playerIcon.localRotation = Quaternion.Euler(0, 0, -playerTransform.eulerAngles.y);
    }

    public Vector2 GetMapPosition(Vector3 worldPos)
    {
        float normalizedX = (worldPos.x - worldCenter.x) / worldSize.x;
        float normalizedZ = (worldPos.z - worldCenter.y) / worldSize.y;

        float uiX = normalizedX * mapContainer.rect.width;
        float uiY = normalizedZ * mapContainer.rect.height;

        return new Vector2(uiX, uiY);
    }
}