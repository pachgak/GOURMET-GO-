using System.Collections.Generic;
using System.Linq;
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

    // List สำหรับแสดงผลใน Inspector เท่านั้น
    [Header("RUNTIME DEBUG VIEW")]
    [SerializeField]
    [Tooltip("แสดงสถานะ Pool ที่กำลังทำงานอยู่ (อัพเดททุกเฟรม)")]
    private List<Pool> debugPoolList = new List<Pool>();

    private float debugUpdateTimer = 0f;
    private const float DEBUG_UPDATE_INTERVAL = 0.5f; // อัพเดททุก 0.5 วินาที (2 ครั้ง/วินาที)

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

    private void Update()
    {
        DebugUpdateTime();
        
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


    public GameObject Spawn(GameObject callObjPrefab)
    {
        Pool pool;
        if (!poolDictionary.TryGetValue(callObjPrefab, out pool))
        {
            pool = new Pool { prefabObj = callObjPrefab, initialSize = 1 };
            poolDictionary.Add(callObjPrefab, pool);

            GameObject newObj = InstantiateObject(callObjPrefab, 0, pool.gameObjs);
            newObj.SetActive(true);
            UpdateUsageStats(pool, true);
            return newObj;
        }

        // ค้นหา Object ที่ว่าง (ใช้วิธีวนลูปถอยหลัง เพื่อจัดการตัวที่ถูกทำลายไปแล้ว)
        for (int i = pool.gameObjs.Count - 1; i >= 0; i--)
        {
            GameObject obj = pool.gameObjs[i];

            if (obj == null)
            {
                pool.gameObjs.RemoveAt(i); // ลบตัวที่โดน Destroy ทิ้งออกจาก List
                continue;
            }

            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                UpdateUsageStats(pool, true);
                return obj;
            }
        }

        GameObject addedObj = InstantiateObject(callObjPrefab, pool.gameObjs.Count, pool.gameObjs);
        addedObj.SetActive(true);
        UpdateUsageStats(pool, true);
        return addedObj;
    }

    public GameObject Spawn(GameObject callObjPrefab, Vector3 setPositon)
    {
        Pool pool;
        if (!poolDictionary.TryGetValue(callObjPrefab, out pool))
        {
            pool = new Pool { prefabObj = callObjPrefab, initialSize = 1 };
            poolDictionary.Add(callObjPrefab, pool);

            GameObject newObj = InstantiateObject(callObjPrefab, 0, pool.gameObjs);
            newObj.SetActive(true);
            newObj.transform.position = setPositon;
            UpdateUsageStats(pool, true);
            return newObj;
        }

        // ค้นหา Object ที่ว่าง
        for (int i = pool.gameObjs.Count - 1; i >= 0; i--)
        {
            GameObject obj = pool.gameObjs[i];

            if (obj == null)
            {
                pool.gameObjs.RemoveAt(i);
                continue;
            }

            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                obj.transform.position = setPositon;
                UpdateUsageStats(pool, true);
                return obj;
            }
        }

        GameObject addedObj = InstantiateObject(callObjPrefab, pool.gameObjs.Count, pool.gameObjs);
        addedObj.SetActive(true);
        addedObj.transform.position = setPositon;
        UpdateUsageStats(pool, true);
        return addedObj;
    }

    public GameObject Spawn(GameObject callObjPrefab, Transform setParant)
    {
        Pool pool;
        if (!poolDictionary.TryGetValue(callObjPrefab, out pool))
        {
            pool = new Pool { prefabObj = callObjPrefab, initialSize = 1 };
            poolDictionary.Add(callObjPrefab, pool);

            GameObject newObj = InstantiateObject(callObjPrefab, 0, pool.gameObjs);
            newObj.SetActive(true);
            if (newObj.transform.parent != setParant) newObj.transform.parent = setParant;
            UpdateUsageStats(pool, true);
            return newObj;
        }

        // ค้นหา Object ที่ว่าง
        for (int i = pool.gameObjs.Count - 1; i >= 0; i--)
        {
            GameObject obj = pool.gameObjs[i];

            if (obj == null)
            {
                pool.gameObjs.RemoveAt(i);
                continue;
            }

            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                if (obj.transform.parent != setParant) obj.transform.parent = setParant;
                UpdateUsageStats(pool, true);
                return obj;
            }
        }

        GameObject addedObj = InstantiateObject(callObjPrefab, pool.gameObjs.Count, pool.gameObjs);
        addedObj.SetActive(true);
        if (addedObj.transform.parent != setParant) addedObj.transform.parent = setParant;
        UpdateUsageStats(pool, true);
        return addedObj;
    }

    public GameObject Spawn(GameObject callObjPrefab, Transform setParant, Vector3 setPositon)
    {
        Pool pool;
        if (!poolDictionary.TryGetValue(callObjPrefab, out pool))
        {
            pool = new Pool { prefabObj = callObjPrefab, initialSize = 1 };
            poolDictionary.Add(callObjPrefab, pool);

            GameObject newObj = InstantiateObject(callObjPrefab, 0, pool.gameObjs);
            newObj.SetActive(true);
            if (newObj.transform.parent != setParant) newObj.transform.parent = setParant;
            newObj.transform.position = setPositon;
            UpdateUsageStats(pool, true);
            return newObj;
        }

        // ค้นหา Object ที่ว่าง
        for (int i = pool.gameObjs.Count - 1; i >= 0; i--)
        {
            GameObject obj = pool.gameObjs[i];

            if (obj == null)
            {
                pool.gameObjs.RemoveAt(i);
                continue;
            }

            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                if (obj.transform.parent != setParant) obj.transform.parent = setParant;
                obj.transform.position = setPositon;
                UpdateUsageStats(pool, true);
                return obj;
            }
        }

        GameObject addedObj = InstantiateObject(callObjPrefab, pool.gameObjs.Count, pool.gameObjs);
        addedObj.SetActive(true);
        if (addedObj.transform.parent != setParant) addedObj.transform.parent = setParant;
        addedObj.transform.position = setPositon;
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

    // ฟังก์ชันสำหรับคืน Object กลับ Pool ReturnObjectToPool
    public void Respawn(GameObject obj)
    {
        // ... (ถ้ามีการ Implement PoolIdentifier จะเพิ่มการตรวจสอบตรงนี้) ...

        obj.SetActive(false);

        // **อัพเดทสถิติ**
        //เราต้องหาว่า Object นี้เป็นของ Pool ไหนก่อน
        //ในการทำงานจริง จะต้องใช้ PoolIdentifier เพื่อระบุเจ้าของ แต่ในที่นี้จะใช้การวนลูป(ช้า) เพื่อหาสาธิต
        foreach (var pair in poolDictionary)
        {
            if (pair.Value.gameObjs.Contains(obj))
            {
                UpdateUsageStats(pair.Value, false);
                return;
            }
        }
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

    private void UpdateDebugList()
    {
        // ใช้วิธีที่รวดเร็วในการคัดลอก Value ทั้งหมดจาก Dictionary มาใส่ใน List
        // เมธอด Values.ToList() จะทำตรงนี้ให้ 
        debugPoolList = poolDictionary.Values.ToList();
    }

    private void DebugUpdateTime()
    {
        // ตรวจสอบว่า Manager ถูกใช้งานหรือไม่ (ไม่จำเป็นต้องรันถ้าเป็น Editor Mode)
        if (Application.isPlaying)
        {
            debugUpdateTimer -= Time.deltaTime;
            if (debugUpdateTimer <= 0)
            {
                UpdateDebugList(); // เรียกใช้ ToList() เพียง 2 ครั้งต่อวินาที
                debugUpdateTimer = DEBUG_UPDATE_INTERVAL;
            }
        }
    }
}