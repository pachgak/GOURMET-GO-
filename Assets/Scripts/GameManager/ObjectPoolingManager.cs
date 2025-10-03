using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolingManager : MonoBehaviour
{
    // Singleton Pattern
    public static ObjectPoolingManager Instance;

    // Class สำหรับเก็บข้อมูล Pool
    [System.Serializable]
    public class Pool
    {
        public GameObject prefabObj;
        public int initialSize = 5;
        public List<GameObject> gameObjs = new List<GameObject>();

        // **ตัวแปรสำหรับ Debug และสถิติ**
        [Header("Runtime Stats")]
        [Tooltip("จำนวน Object ที่กำลังถูกใช้งานอยู่ในปัจจุบัน")]
        public int statCurrentUsed = 0;
        [Tooltip("จำนวน Object ที่เคยถูกใช้งานพร้อมกันสูงสุด")]
        public int statMaxUsed = 0;
    }

    // Dictionary หลัก: Key คือ Prefab, Value คือ Pool ของ Prefab นั้น
    private Dictionary<GameObject, Pool> poolDictionary = new Dictionary<GameObject, Pool>();

    [SerializeField]
    private List<Pool> poolsToPreWarm = new List<Pool>();


    private void Awake()
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

        InitializePools();
    }

    private void InitializePools()
    {
        foreach (Pool pool in poolsToPreWarm)
        {
            if (pool.prefabObj == null) continue;

            if (!poolDictionary.ContainsKey(pool.prefabObj))
            {
                // สำคัญ: ต้องสร้าง List ใหม่ใน Pool ที่ถูก Add เข้า Dictionary เพื่อป้องกัน Reference ปัญหา
                pool.gameObjs = new List<GameObject>();
                poolDictionary.Add(pool.prefabObj, pool);
            }

            for (int i = 0; i < pool.initialSize; i++)
            {
                InstantiateObject(pool.prefabObj, pool.gameObjs.Count, pool.gameObjs);
            }
        }
    }


    // เมธอดหลักสำหรับเรียกใช้ Object
    public GameObject GetPoorObj(GameObject callObjPrefab)
    {
        Pool pool;
        if (!poolDictionary.TryGetValue(callObjPrefab, out pool))
        {
            // ถ้าไม่มี Pool: สร้าง Pool ใหม่แบบ On-Demand
            pool = new Pool { prefabObj = callObjPrefab, initialSize = 1 };
            poolDictionary.Add(callObjPrefab, pool);

            // Instantiating ตัวแรก
            GameObject newObj = InstantiateObject(callObjPrefab, 0, pool.gameObjs);
            newObj.SetActive(true);
            // อัพเดทสถิติสำหรับตัวที่ถูกสร้างใหม่และถูกใช้งาน
            UpdateUsageStats(pool, true);
            return newObj;
        }

        // ค้นหา Object ที่ว่าง
        foreach (GameObject obj in pool.gameObjs)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);

                // อัพเดทสถิติ
                UpdateUsageStats(pool, true);
                return obj;
            }
        }

        // ถ้าทุกตัวถูกใช้งานหมด: Instantiate ตัวใหม่เพิ่ม (ขยาย Pool)
        GameObject addedObj = InstantiateObject(callObjPrefab, pool.gameObjs.Count, pool.gameObjs);

        // อัพเดทสถิติสำหรับตัวที่ถูกสร้างใหม่และถูกใช้งาน
        UpdateUsageStats(pool, true);
        return addedObj;
    }

    // ฟังก์ชันย่อยสำหรับการ Instantiate และตั้งค่าเริ่มต้น
    private GameObject InstantiateObject(GameObject prefab, int index, List<GameObject> poolList)
    {
        GameObject newObj = Instantiate(prefab, transform);
        newObj.name = prefab.name + " (" + index + ")";
        newObj.SetActive(false);
        poolList.Add(newObj);
        return newObj;
    }

    // ฟังก์ชันสำหรับคืน Object กลับ Pool
    public void ReturnObjectToPool(GameObject obj)
    {
        // ... (ถ้ามีการ Implement PoolIdentifier จะเพิ่มการตรวจสอบตรงนี้) ...

        obj.SetActive(false);

        // **อัพเดทสถิติ**
        // เราต้องหาว่า Object นี้เป็นของ Pool ไหนก่อน
        // ในการทำงานจริง จะต้องใช้ PoolIdentifier เพื่อระบุเจ้าของ แต่ในที่นี้จะใช้การวนลูป (ช้า) เพื่อหาสาธิต
        //foreach (var pair in poolDictionary)
        //{
        //    if (pair.Value.gameObjs.Contains(obj))
        //    {
        //        UpdateUsageStats(pair.Value, false);
        //        return;
        //    }
        //}
    }

    // ฟังก์ชันสำหรับอัพเดท statCurrentUsed และ statMaxUsed
    private void UpdateUsageStats(Pool pool, bool increase)
    {
        if (increase)
        {
            pool.statCurrentUsed++;
            // ตรวจสอบและบันทึกค่าสูงสุดที่เคยใช้
            if (pool.statCurrentUsed > pool.statMaxUsed)
            {
                pool.statMaxUsed = pool.statCurrentUsed;
            }
        }
        else
        {
            // ตรวจสอบไม่ให้ค่าต่ำกว่าศูนย์
            pool.statCurrentUsed = Mathf.Max(0, pool.statCurrentUsed - 1);
        }
    }
}