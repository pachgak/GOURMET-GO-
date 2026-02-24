using UnityEngine;

public class EnemyDisguise : MonoBehaviour
{
    [Header("Disguise Settings")]
    public GameObject[] enemyCloneList; // รายชื่อมอนสเตอร์ที่จะแปลงร่างเลียนแบบ
    public float revealRange = 5f;      // ระยะที่ผู้เล่นเดินเข้ามาแล้วจะคืนร่าง
    public GameObject revealVFX;        // เอฟเฟกต์ควันตอนคืนร่าง

    [Header("References")]
    public Transform target;            // ผู้เล่น
    public GameObject enemyGraphics;    // โมเดลกราฟิกของร่างจริง

    private GameObject _currentClone;
    private EnemyHealth _myHealth;
    private BaseEnemyAI _myAI;
    private BaseEnemyMovement _myMovement;
    private BaseEnemyCombat _myCombat;
    private Collider _myCollider;

    private bool _isDisguised = false;

    private void Awake()
    {
        _myHealth = GetComponent<EnemyHealth>();
        _myAI = GetComponent<BaseEnemyAI>();
        _myMovement = GetComponent<BaseEnemyMovement>();
        _myCombat = GetComponent<BaseEnemyCombat>();
        _myCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        SetupDisguise();
    }

    private void SetupDisguise()
    {
        if (enemyCloneList == null || enemyCloneList.Length == 0) return;

        // 1. สุ่มมอนสเตอร์จำแลง
        GameObject prefabToClone = enemyCloneList[Random.Range(0, enemyCloneList.Length)];

        // 2. เสกโคลนออกมาตรงตำแหน่งเดียวกับร่างจริง
        _currentClone = ObjectPoolingManager.Instance.Spawn(prefabToClone, transform.position);

        // 3. ปิดการดรอปไอเทมของโคลน (พอมัน disable มันจะถอด Event OnDie ให้เอง)
        if (_currentClone.TryGetComponent(out SpawnItemDropPoor dropSystem))
        {
            dropSystem.enabled = false;
        }

        // 4. ปิดการโจมตีของโคลน (ให้มันเดินโง่ๆ อย่างเดียว)
        if (_currentClone.TryGetComponent(out BaseEnemyCombat cloneCombat))
        {
            cloneCombat.enabled = false;
        }

        // 5. ดักจับตอนที่โคลนโดนโจมตี ให้เรียกฟังก์ชันคืนร่าง
        if (_currentClone.TryGetComponent(out EnemyHealth cloneHealth))
        {
            // ใช้ Lambda ส่งดาเมจกลับมาให้ร่างจริงด้วย
            cloneHealth.OnTakeDamage += (damage) => RevealTrueForm(damage);
        }

        // 6. ซ่อนร่างจริง ปิด AI, Movement, และ Collider
        enemyGraphics.SetActive(false);
        if (_myAI != null) _myAI.enabled = false;
        if (_myMovement != null) _myMovement.enabled = false;
        if (_myCombat != null) _myCombat.enabled = false;
        if (_myCollider != null) _myCollider.enabled = false;

        _isDisguised = true;
    }

    private void Update()
    {
        if (!_isDisguised) return;

        // ถ้าร่างโคลนหายไปหรือพัง ให้หยุดทำงาน
        if (_currentClone == null) return;

        // ซิงค์ตำแหน่งร่างจริงให้แอบตามร่างโคลนไปเรื่อยๆ (เผื่อโคลนมันเดิน Roam ไปไกล)
        transform.position = _currentClone.transform.position;

        // เช็คระยะผู้เล่น ถ้าระยะใกล้เกินไป ให้คืนร่าง!
        if (target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);
            if (dist <= revealRange)
            {
                RevealTrueForm(0f); // 0f คือไม่ได้โดนตี แต่คืนร่างเพราะเดินมาใกล้
            }
        }
    }

    private void RevealTrueForm(float damageTaken)
    {
        if (!_isDisguised) return;
        _isDisguised = false;

        // 1. เล่น VFX ควันตรงตำแหน่งร่างโคลน
        if (revealVFX != null)
        {
            ObjectPoolingManager.Instance.Spawn(revealVFX, _currentClone.transform.position);
        }

        // 2. เอาร่างโคลนกลับเข้า Pool (สำคัญ! อย่าลืมเอา Event ออกด้วย)
        if (_currentClone.TryGetComponent(out EnemyHealth cloneHealth))
        {
            cloneHealth.OnTakeDamage -= (damage) => RevealTrueForm(damage); // ป้องกัน Memory Leak
        }
        ObjectPoolingManager.Instance.Respawn(_currentClone);

        // 3. เปิดการใช้งานร่างจริงทั้งหมด
        enemyGraphics.SetActive(true);
        if (_myAI != null) _myAI.enabled = true;
        if (_myMovement != null) _myMovement.enabled = true;
        if (_myCombat != null) _myCombat.enabled = true;
        if (_myCollider != null) _myCollider.enabled = true;

        // 4. ถ้าร่างโคลนโดนตี ร่างจริงก็รับดาเมจนั้นด้วย
        if (damageTaken > 0 && _myHealth != null)
        {
            _myHealth.TakeDamage(damageTaken);
        }

        Debug.Log($"{gameObject.name} เผยร่างจริงแล้ว!");
    }
}