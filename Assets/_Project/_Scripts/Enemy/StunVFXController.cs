using UnityEngine;

public class StunVFXController : MonoBehaviour
{
    [Header("Stun VFX Settings")]
    public GameObject stunVFXPrefab; // ลาก Prefab ดาวหมุนติ้วๆ มาใส่

    [Tooltip("ตำแหน่งที่ดาวจะโผล่ (ถ้าไม่มี headRoot จะอ้างอิงจากจุดศูนย์กลางมอน)")]
    public Vector3 offset = new Vector3(0, 2f, 0);

    [Tooltip("ใส่ Transform ตำแหน่งหัว (Optional) เพื่อให้ดาวเกาะติดหัวเป๊ะๆ")]
    public Transform headRoot;

    private BaseEnemyAI _enemyAI;
    private GameObject _currentStunVFX; // ตัวแปรคอยจำว่าสร้างดาวดวงไหนไว้ จะได้ลบถูกตัว

    private void Awake()
    {
        _enemyAI = GetComponent<BaseEnemyAI>();
    }

    private void OnEnable()
    {
        if (_enemyAI != null)
        {
            // รอฟังข่าวสารการสตันจาก AI
            _enemyAI.OnStunStateChanged += HandleStunStateChanged;
        }
    }

    private void OnDisable()
    {
        if (_enemyAI != null)
        {
            _enemyAI.OnStunStateChanged -= HandleStunStateChanged;
        }

        // *** ป้องกันบั๊กขยะตกค้าง *** // เผื่อมอนสเตอร์ตาย หรือถูกลบออกจากฉากไปตอนที่ "กำลังมึน" อยู่พอดี
        RemoveStunVFX();
    }

    private void HandleStunStateChanged(bool isStunned)
    {
        if (isStunned)
        {
            ShowStunVFX();
        }
        else
        {
            RemoveStunVFX();
        }
    }

    private void ShowStunVFX()
    {
        // ถ้าลืมใส่ Prefab หรือมีดาวอยู่บนหัวแล้ว ให้ข้ามไปเลย
        if (stunVFXPrefab == null || _currentStunVFX != null) return;

        // กำหนดจุดที่จะให้ดาวไปเกาะ (ถ้ามี headRoot ใช้หัว ถ้าไม่มีใช้ตัวแม่)
        Transform parentTransform = (headRoot != null) ? headRoot : transform;

        // 1. เสกดาวออกมาจาก Pool และ "บังคับให้เป็นลูก (Child)" ของมอนสเตอร์
        // เพื่อที่เวลาหมูป่ากระเด็นถอยหลัง ดาวจะได้ขยับตามหัวหมูไปด้วย!
        _currentStunVFX = ObjectPoolingManager.Instance.Spawn(stunVFXPrefab, parentTransform);

        // 2. ปรับตำแหน่งให้อยู่บนหัวตาม Offset
        _currentStunVFX.transform.localPosition = offset;

        // 3. รีเซ็ตการหมุน เผื่อเกม 2.5D แล้วสเกลมอนสเตอร์มีพลิกซ้ายขวา (-1, 1) ดาวจะได้ไม่กลับหัว
        _currentStunVFX.transform.localRotation = Quaternion.identity;
    }

    private void RemoveStunVFX()
    {
        // ถ้ามีดาวค้างอยู่บนหัว ให้จับโยนกลับลงบ่อ Pool
        if (_currentStunVFX != null)
        {
            ObjectPoolingManager.Instance.Respawn(_currentStunVFX);
            _currentStunVFX = null; // เคลียร์ความจำทิ้ง
        }
    }
}