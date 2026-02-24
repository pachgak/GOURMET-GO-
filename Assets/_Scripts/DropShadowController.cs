using UnityEngine;

public class DropShadowController : MonoBehaviour
{
    [Header("References")]
    public GameObject shadowPrefab; // ลาก Prefab เงา (ตัวแม่ ShadowRoot) มาใส่ตรงนี้

    [Header("Raycast Settings")]
    public LayerMask floorLayer;      // เลเยอร์พื้น
    public float yOffset = 0.05f;     // ระยะลอยกันเงากะพริบ
    public float maxRayDistance = 30f;// ระยะที่ยิง Raycast ลงไปหาพื้น

    [Header("Dynamic Shadow Settings")]
    public float maxHeightEffect = 10f; // ความสูงที่จะทำให้เงาจางหายไป 100%
    public float baseAlpha = 0.5f;     // ความเข้มตอนอยู่ติดพื้น
    public float baseScale = 1f;       // ขนาดตอนอยู่ติดพื้น
    public float effectScaleMutiply = 2f; // ขนาดตัวคูณตอนอยู่สูง

    // *** แยกตัวแปรเก็บแม่และลูก ***
    private GameObject _shadowRootObj;        // เก็บตัวแม่ (เอาไว้ขยับ/หมุน/สเกล)
    private SpriteRenderer _shadowSprite;     // เก็บตัวลูก (เอาไว้เปลี่ยนสี/ความจาง)

    private const float _yRayOffset = 0.1f;    // ยกจุดยิง Raycast ขึ้นนิดนึงกันจมพื้น

    private void OnEnable()
    {
        // สร้างเงาตอนที่มอนสเตอร์เกิด (หรือถูกเรียกจาก Pool)
        if (shadowPrefab != null && _shadowRootObj == null)
        {
            // *** ถ้าคุณอยากให้เงาใช้ระบบ Pool ด้วย สามารถเปลี่ยนบรรทัดล่างนี้เป็น:
            // _shadowRootObj = ObjectPoolingManager.Instance.Spawn(shadowPrefab, transform.position);
            _shadowRootObj = Instantiate(shadowPrefab, transform.position, Quaternion.identity);

            // หา SpriteRenderer ในลูกชั้นที่ 2
            _shadowSprite = _shadowRootObj.GetComponentInChildren<SpriteRenderer>();

            if (_shadowSprite == null)
            {
                Debug.LogWarning($"{gameObject.name}: หา SpriteRenderer ในลูกไม่เจอ! เช็ค Prefab ด่วนครับ");
            }
        }

        // เปิดการแสดงผลเงา
        if (_shadowRootObj != null)
        {
            _shadowRootObj.SetActive(true);
        }
    }

    private void OnDisable()
    {
        // ซ่อนเงา (หรือคืนเข้า Pool) ตอนที่มอนสเตอร์ตาย/ถูกเก็บเข้า Pool
        if (_shadowRootObj != null)
        {
            // *** ถ้าเงาใช้ระบบ Pool ให้เปลี่ยนเป็น:
            // ObjectPoolingManager.Instance.Respawn(_shadowRootObj);
            // _shadowRootObj = null; // เคลียร์ค่าทิ้งเพื่อเสกใหม่รอบหน้า

            _shadowRootObj.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        // ต้องมีครบทั้งแม่และลูกถึงจะทำงานต่อได้
        if (_shadowRootObj == null || _shadowSprite == null) return;

        // ยิง Raycast ลงไปข้างล่าง
        if (Physics.Raycast(transform.position + new Vector3(0, _yRayOffset, 0), Vector3.down, out RaycastHit hit, maxRayDistance, floorLayer))
        {
            if (!_shadowSprite.enabled) _shadowSprite.enabled = true;

            // 1. ตั้งตำแหน่งและการเอียงที่ "ตัวแม่ (_shadowRootObj)"
            _shadowRootObj.transform.position = hit.point + new Vector3(0, yOffset, 0);
            _shadowRootObj.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            // 2. คำนวณความสูงแบบเปอร์เซ็นต์ (0.0 ถึง 1.0)
            float heightPercent = Mathf.Clamp01(hit.distance / maxHeightEffect);

            // 3. ปรับขนาด (Scale) ที่ "ตัวแม่ (_shadowRootObj)"
            float currentScale = Mathf.Lerp(baseScale, baseScale * effectScaleMutiply, heightPercent);
            _shadowRootObj.transform.localScale = new Vector3(currentScale, currentScale, currentScale);

            // 4. ปรับความจาง (Alpha) ที่ "ตัวลูก (_shadowSprite)" : ยิ่งสูงยิ่งจางลง
            Color shadowColor = _shadowSprite.color;
            shadowColor.a = Mathf.Lerp(baseAlpha, 0f, heightPercent);
            _shadowSprite.color = shadowColor;
        }
        else
        {
            if (_shadowSprite.enabled) _shadowSprite.enabled = false;
        }
    }

    private void OnDestroy()
    {
        // ป้องกันขยะตกค้าง ถ้าร่างต้น (มอนสเตอร์) ถูก Destroy ลบออกจากฉากไปเลยจริงๆ
        if (_shadowRootObj != null)
        {
            Destroy(_shadowRootObj);
        }
    }
}