using System.Collections.Generic;
using UnityEngine;

// --- คลาสสำหรับเก็บข้อมูลอัตราการดรอป/การเกิด ---
[System.Serializable]
public class PossiblePrefabsRate
{
    [Tooltip("ใส่ Prefab ที่ต้องการให้เกิด\n(ถ้าปล่อยเว้นว่าง/None ไว้ คือ 'โอกาสที่จะไม่เกิดอะไรเลย')")]
    public GameObject possiblePrefab;

    [Tooltip("โอกาสการเกิด (น้ำหนัก)")]
    [Min(1)]
    public int rate = 1;
}

// --- แม่พิมพ์สำหรับสร้างไฟล์ SO โซนเกิดมอนสเตอร์ ---
[CreateAssetMenu(fileName = "New Possible Prefabs", menuName = "Spawn System/Possible Prefabs Zone")]
public class PossiblePrefabsSO : ScriptableObject
{
    [Header("Zone Spawner Data")]
    [Tooltip("รายชื่อมอนสเตอร์หรือพืช พร้อมอัตราการสุ่มเกิด")]
    public List<PossiblePrefabsRate> prefabs = new List<PossiblePrefabsRate>();
}