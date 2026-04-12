using Inventory;
using Inventory.Model; // จำเป็นต้องใช้เพื่อเข้าถึง InventoryItem และ InventorySO
using System.Collections.Generic; // จำเป็นต้องใช้เพื่อเข้าถึง Dictionary
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PalyerUIController : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private InventoryController playerInventoryController;

    public Slider hpBar;
    public TMP_Text hpText;

    public TMP_Text BackPackText;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerInventoryController = GetComponent<InventoryController>();
    }

    private void Start()
    {
        //// ตั้งค่า HP เริ่มต้น
        //hpBar.maxValue = playerHealth.maxHealth;
        //playerHealth.setHp(playerHealth.maxHealth);

        // ตั้งค่า Inventory UI เริ่มต้น
        if (playerInventoryController != null && playerInventoryController.InventoryData != null)
        {
            // อัปเดตครั้งแรกทันที
            UpdateBackpackUI(playerInventoryController.InventoryData.GetCurrentInventoryState());

            // สมัครรับข้อมูลเมื่อมีการเปลี่ยนแปลงในอนาคต
            playerInventoryController.InventoryData.OnInventoryUpdated += UpdateBackpackUI;
        }
    }

    private void OnDestroy() // หรือใช้ OnDisable ก็ได้ แต่ต้องระวังเรื่องลำดับการเรียก
    {
        // ยกเลิกการสมัครเพื่อป้องกัน Error
        if (playerInventoryController != null && playerInventoryController.InventoryData != null)
        {
            playerInventoryController.InventoryData.OnInventoryUpdated -= UpdateBackpackUI;
        }
    }

    // เอา hpBar.maxValue ออกจาก Start ได้เลย เพราะ Event ส่งมาให้แล้ว
    private void OnEnable()
    {
        playerHealth.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        playerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(float current, float max)
    {
        hpBar.maxValue = max;
        hpBar.value = current;
        hpText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
    }

    // ฟังก์ชันสำหรับอัปเดตข้อความกระเป๋า
    private void UpdateBackpackUI(Dictionary<int, InventoryItem> inventoryState)
    {
        // 1. นับจำนวนช่องที่ถูกใช้ (Dictionary นี้ส่งมาเฉพาะช่องที่มีของ ไม่ส่งช่องว่าง)
        int occupiedSlots = inventoryState.Count;

        // 2. ดึงขนาดกระเป๋าทั้งหมด
        int totalSize = playerInventoryController.InventoryData.Size;

        // 3. แสดงผล
        BackPackText.text = $"{occupiedSlots}/{totalSize}";
    }

    void Update()
    {

    }
}