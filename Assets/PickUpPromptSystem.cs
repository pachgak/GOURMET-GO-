using Inventory.Model;
using System;
using UnityEngine;

public class PickUpPromptSystem : MonoBehaviour
{
    [Header("UI Prompt Settings")]
    // ลาก Prefab ของ UIPickItemUpPrompt ที่มี PickUpPromptUI.cs แนบไว้มาใส่ในช่องนี้
    [SerializeField] private GameObject pickupPromptPrefab;
    [SerializeField] private Transform promptSpawnPoint;

    [SerializeField]
    private InventorySO inventoryData;

    private void Awake()
    {

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        inventoryData.OnAddItem += HeadleAddItem;
        inventoryData.OnRemoveItem += HeadleRemoveItem;
    }

    public void HeadleAddItem(ItemSO item, int quantity)
    {
        CreatePickupPrompt(item.ItemImage, quantity);
    }

    public void HeadleRemoveItem(ItemSO item, int quantity)
    {
        CreatePickupPrompt(item.ItemImage, -quantity);
    }

    private void CreatePickupPrompt(Sprite icon, int quantity)
    {
        // 1. สร้าง GameObject จาก Prefab
        GameObject promptGO = ObjectPoolingManager.Instance.Spawn(pickupPromptPrefab, promptSpawnPoint);

        // 2. ดึงสคริปต์ควบคุม UI และตั้งค่า
        if (promptGO.TryGetComponent(out PickUpPromptUI promptUI))
        {
            promptUI.SetupAndShow(icon, quantity);
        }
    }
}
