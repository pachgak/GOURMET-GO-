using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public enum SpawnType { RandomMob, RandomPlant, FixedBoss }

    [Header("Settings")]
    public SpawnType type;
    [Tooltip("ระยะการสุ่มเกิดรอบๆ จุดนี้")]
    public float spawnRadius = 2f;

    [Header("Spawn Data")]
    [Tooltip("ถ้าใส่ SO ตัวนี้ ระบบจะดึงข้อมูลจาก SO นี้ไปใช้ก่อน")]
    public PossiblePrefabsSO possibleSO;

    [Tooltip("ถ้าไม่ได้ใส่ SO ด้านบน ระบบจะมาใช้ List ตัวนี้แทน (ใช้ทำจุดเกิดเฉพาะกิจ)")]
    public List<PossiblePrefabsRate> possiblePrefabs; // <--- เปลี่ยน Type แล้ว

    private GameObject _spawnedInstance;

    // --- คำสั่งเสก --- 
    public void SpawnObject()
    {
        ClearObject();

        // 1. เลือกว่าจะใช้ข้อมูลจาก SO หรือจากตัวเอง
        List<PossiblePrefabsRate> activeList = null;

        if (possibleSO != null && possibleSO.prefabs != null && possibleSO.prefabs.Count > 0)
        {
            activeList = possibleSO.prefabs; // ใช้จาก SO (กลุ่มโซน)
        }
        else if (possiblePrefabs != null && possiblePrefabs.Count > 0)
        {
            activeList = possiblePrefabs; // ใช้จากจุดนี้โดยเฉพาะ (เฉพาะกิจ)
        }

        if (activeList == null || activeList.Count == 0) return;

        // ==========================================
        // 2. ระบบสุ่มแบบมีน้ำหนัก (Weighted Random)
        // ==========================================
        int totalRate = 0;

        // 2.1 หาผลรวมของ Rate ทั้งหมดก่อน
        foreach (var item in activeList)
        {
            totalRate += item.rate;
        }

        if (totalRate <= 0) return; // กันบั๊กกรณีเผลอใส่ Rate เป็น 0 ทุกอัน

        // 2.2 สุ่มตัวเลขตั้งแต่ 0 ถึง totalRate - 1
        int randomValue = Random.Range(0, totalRate);
        int currentSum = 0;
        GameObject selectedPrefab = null;

        // 2.3 หาว่าตัวเลขที่สุ่มได้ ไปตกอยู่ในช่วงของของชิ้นไหน
        foreach (var item in activeList)
        {
            currentSum += item.rate;
            if (randomValue < currentSum)
            {
                selectedPrefab = item.possiblePrefab;
                break;
            }
        }

        // ==========================================
        // 3. จัดการตอนเสกของ
        // ==========================================

        // ถ้าระบบจั่วได้ช่องที่ "Prefab ว่างเปล่า (null)" แปลว่าตาเบอร์นี้ "ไม่เกิดอะไรเลย" ให้หยุดทำงานทันที!
        if (selectedPrefab == null)
        {
            return;
        }

        // เสกของออกมาแบบสุ่มระยะวงกลมรอบจุด
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 randomPosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        _spawnedInstance = Instantiate(selectedPrefab, randomPosition, transform.rotation);
        _spawnedInstance.transform.SetParent(this.transform);
    }

    // --- คำสั่งล้างบาง ---
    public void ClearObject()
    {
        // 1. เคลียร์ตัวแปรอ้างอิงตัวหลักทิ้งไปก่อน
        _spawnedInstance = null;

        // 2. วนลูปทำลาย Child "ทุกตัว" ที่อยู่ภายใต้ SpawnPoint นี้ 
        // (เทคนิค: วนลูปถอยหลัง ป้องกันบั๊กข้าม Index เวลา Object ถูกทำลาย)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    // --- วาดเส้นขอบเขตใน Scene View ---
    private void OnDrawGizmos()
    {

        // 3. วาดเส้นขอบเขตรัศมี (Wire Sphere) และเปลี่ยนสีตามประเภท (Type)
        switch (type)
        {
            case SpawnType.RandomMob: Gizmos.color = Color.red; break;
            case SpawnType.RandomPlant: Gizmos.color = Color.green; break;
            case SpawnType.FixedBoss: Gizmos.color = Color.magenta; break;
        }

        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // 1. เช็คก่อนว่ามีข้อมูลให้เสกไหม?
        bool hasSO = possibleSO != null && possibleSO.prefabs != null && possibleSO.prefabs.Count > 0;
        bool hasLocal = possiblePrefabs != null && possiblePrefabs.Count > 0;

        // ถ้าไม่มีข้อมูลอะไรเลยในทั้ง 2 ช่อง ให้ซ่อน Gizmos ไปเลย (จะได้รู้ว่าลืมตั้งค่า)
        if (!hasSO && !hasLocal) return;

        // 2. วาดจุดกึ่งกลางทึบ (Solid Sphere) เพื่อบอกว่าใช้แหล่งข้อมูลจากไหน
        if (hasSO)
        {
            // ถ้าดึงข้อมูลจาก SO ให้จุดศูนย์กลางเป็น สีฟ้า (Cyan)
            Gizmos.color = Color.cyan;
        }
        else
        {
            // ถ้าดึงข้อมูลจาก Local List ให้จุดศูนย์กลางเป็น สีส้ม (Orange)
            Gizmos.color = new Color(1f, 0.5f, 0f);
        }
        Gizmos.DrawSphere(transform.position, 0.25f); // ขนาดลูกแก้วตรงกลาง

    }
}