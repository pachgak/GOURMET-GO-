using UnityEngine;

namespace Inventory.UI // หรือ Namespace ที่คุณใช้
{
    // บังคับว่าต้องมี CookingStationController อยู่ด้วยกัน
    [RequireComponent(typeof(CookingStationController))]
    public class CookingAutoReturnHandler : MonoBehaviour
    {
        private CookingStationController _cookingController;

        [Header("Settings")]
        [SerializeField] private bool returnOnClose = true; // เผื่ออยากเปิด/ปิดระบบนี้ใน Inspector

        private void Awake()
        {
            _cookingController = GetComponent<CookingStationController>();
        }

        private void Start()
        {
            // Subscribe Event จาก Singleton Manager
            if (CookingManager.instance != null)
            {
                CookingManager.instance.OnOpenCookingStateChange += HandleCookingStateChanged;
            }
        }

        private void OnDestroy()
        {
            // อย่าลืม Unsubscribe เพื่อป้องกัน Memory Leak
            if (CookingManager.instance != null)
            {
                CookingManager.instance.OnOpenCookingStateChange -= HandleCookingStateChanged;
            }
        }

        private void HandleCookingStateChanged(bool isOpen)
        {
            // ถ้า UI ถูกสั่งปิด (isOpen = false) และเราเปิดระบบคืนของไว้
            if (!isOpen && returnOnClose)
            {
                // เรียกฟังก์ชันคืนของใน Controller (ที่คุณต้องเพิ่มเข้าไป)
                _cookingController.ReturnItemsToPlayer();
            }
        }

        // เพิ่ม OnApplicationQuit เพื่อกันเหนียว (กรณีปิดเกมเลย)
        private void OnApplicationQuit()
        {
            // ถ้าอยากให้คืนของตอนปิดเกมด้วย
            if (_cookingController != null)
            {
                _cookingController.ReturnItemsToPlayer();
            }
        }
    }
}