using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Inventory.Model; // เพื่อให้รู้จัก InventoryItem

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("References to Systems")]
    public PlayerHealth playerHealth;
    //public InventorySO playerInventory;
    //public PlayerLoadoutSkill playerLoadout;
    //public MenuIndexManager menuManager;
    //public ItemDatabaseSO itemDatabase; // พจนานุกรมไอเทมที่คุณได้โค้ดมา

    private string _saveFilePath;
    private GameSaveData _currentData = new GameSaveData();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        // กำหนดที่อยู่ไฟล์เซฟ
        _saveFilePath = Path.Combine(Application.persistentDataPath, "SaveGame.json");
    }

    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        //ที่ใส่ Format save ต่างๆ
        SaveFormat();

        //ระบบ save
        // 4. แปลงเป็น JSON และเขียนไฟล์
        string json = JsonUtility.ToJson(_currentData, true);
        File.WriteAllText(_saveFilePath, json);

        Debug.Log("บันทึกสำเร็จที่: " + _saveFilePath);
    }


    [ContextMenu("Load Game")]
    public void LoadGame()
    {
        if (!File.Exists(_saveFilePath)) return;

        // 1. อ่านไฟล์ JSON
        string json = File.ReadAllText(_saveFilePath);
        _currentData = JsonUtility.FromJson<GameSaveData>(json);

        LoadFormat();

        Debug.Log("โหลดข้อมูลสำเร็จ!");
    }

    private void SaveFormat()
    {
        _currentData.currentHealth = playerHealth.currentHealth;
        _currentData.playerPosition[0] = playerHealth.transform.position.x;
        _currentData.playerPosition[1] = playerHealth.transform.position.y;
        _currentData.playerPosition[2] = playerHealth.transform.position.z;
    }


    private void LoadFormat()
    {
        // 2. ส่งค่ากลับไปที่ PlayerHealth
        playerHealth.setHp(_currentData.currentHealth);
        playerHealth.transform.position = new Vector3(_currentData.playerPosition[0], _currentData.playerPosition[1], _currentData.playerPosition[2]);

    }



}