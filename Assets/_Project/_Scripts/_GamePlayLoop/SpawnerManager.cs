using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    public static SpawnerManager Instance { get; private set; }

    [Tooltip("ลาก SpawnPoint ทุกตัวในฉากมาใส่ที่นี่")]
    public List<SpawnPoint> allSpawnPoints;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    // ฟังก์ชันสำหรับดึง SpawnPoint ทั้งหมดอัตโนมัติ (ไม่ต้องลากเอง)
    [ContextMenu("Auto Find All Spawn Points")]
    public void FindAllSpawnPoints()
    {
        allSpawnPoints = new List<SpawnPoint>(FindObjectsOfType<SpawnPoint>());
    }

    public void TriggerAllSpawns()
    {
        Debug.Log("[Spawner] เสกมอนสเตอร์และพืชทั้งหมด!");
        foreach (var point in allSpawnPoints)
        {
            if(point == null) continue;
            point.SpawnObject();
        }
    }

    public void ResetAllSpawns()
    {
        Debug.Log("[Spawner] ล้างบางมอนสเตอร์และพืชทั้งหมด!");
        foreach (var point in allSpawnPoints)
        {
            if (point == null) continue;
            point.ClearObject();
        }
    }
}