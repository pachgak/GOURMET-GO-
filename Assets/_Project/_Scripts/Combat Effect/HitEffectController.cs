using UnityEngine;

public class HitEffectController : MonoBehaviour
{
    [Tooltip("เอฟเฟกต์พื้นฐาน (เช่น ประกายไฟปกติ)")]
    public GameObject effecfHitPrefab;
    public Vector3 offSet;
    public ITakeDamage takeDamage;

    public void Awake()
    {
        takeDamage = GetComponent<ITakeDamage>();
    }

    private void OnEnable()
    {
        takeDamage.OnTakeDamage += HandleInstantiateEffect;
    }
    private void OnDisable()
    {
        takeDamage.OnTakeDamage -= HandleInstantiateEffect;
    }

    // *** อัปเดตพารามิเตอร์ ให้รับ GameObject เข้ามาด้วย ***
    public void HandleInstantiateEffect(float damage, GameObject incomingHitVFX)
    {
        Vector3 effectPos = transform.position + offSet;

        // 1. ตรรกะการเลือก VFX: ถ้าสกิลมี VFX เฉพาะตัวส่งมา (รอยฟัน) ให้ใช้ตัวนั้น
        // แต่ถ้าส่ง null มา (สกิลธรรมดา) ให้กลับไปใช้ effecfHitPrefab ตัว Default ของมอนสเตอร์
        GameObject vfxToSpawn = (incomingHitVFX != null) ? incomingHitVFX : effecfHitPrefab;

        if (vfxToSpawn != null)
        {
            // 2. เสกเอฟเฟกต์ออกมาจาก Pool
            GameObject cloneEffecfHit = ObjectPoolingManager.Instance.Spawn(vfxToSpawn);
            cloneEffecfHit.transform.parent = transform;
            cloneEffecfHit.transform.position = effectPos;
        }
    }
}