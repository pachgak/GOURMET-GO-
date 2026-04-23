using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Inventory.Model; // เพื่อให้รู้จัก InventoryItem

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("References to Systems")]
    //PlayerHadlth
    public PlayerHealth playerHealth;
    //Item
    public InventorySO playerInventory;
    public InventorySO baseStorageInventory; // <--- เพิ่มบรรทัดนี้สำหรับ Storage
    public ItemDatabaseSO itemDatabase; // พจนานุกรมไอเทมที่คุณได้โค้ดมา
    //Buff
    public PlayerBuffManager playerBuffManager;
    public BuffDatabaseSO buffDatabase;
    //skill
    public PlayerLoadoutSkill playerLoadout;
    public SkillDatabaseSO skillDatabase;
    public PlayerSkill playerSkillBar;

    //public PlayerLoadoutSkill playerLoadout;
    //public MenuIndexManager menuManager;

    private string _saveFilePath;
    private GameSaveData _currentData = new GameSaveData();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        // กำหนดที่อยู่ไฟล์เซฟ
        _saveFilePath = Path.Combine(Application.persistentDataPath, "SaveGame.json");
    }

    // *** เปลี่ยนเป็น IEnumerator เพื่อให้มันหน่วงเวลาได้ ***
    private System.Collections.IEnumerator Start()
    {
        // 1. สั่งให้ SaveManager รอ 1 เฟรม เพื่อให้ PlayerSkill, Inventory, Loadout รัน Start() ของตัวเองให้เสร็จก่อน!
        yield return null;

        // 2. พอทุกคนเตรียมตัวเสร็จแล้ว ค่อยเริ่มอ่านจดหมายน้อยและโหลดเซฟ
        int isNewGame = PlayerPrefs.GetInt("IsNewGame", 0);

        if (isNewGame == 0 && File.Exists(_saveFilePath))
        {
            LoadGame();
        }
        else
        {
            ResetGameToDefault();
        }
    }

    // ฟังก์ชันสำหรับเคลียร์ค่า ScriptableObject และ State ต่างๆ กลับเป็น 0
    public void ResetGameToDefault()
    {
        // ลบไฟล์เซฟเก่าทิ้ง
        if (File.Exists(_saveFilePath)) File.Delete(_saveFilePath);

        // 1. ล้างกระเป๋า
        //if (playerInventory != null) playerInventory.Initialize();
        //if (baseStorageInventory != null) baseStorageInventory.Initialize();

        // 2. รีเซ็ตเลือด
        //if (playerHealth != null)
        //{
        //    playerHealth.currentMaxHealth = playerHealth.baseMaxHealth;
        //    playerHealth.setHp(playerHealth.baseMaxHealth);
        //}

        // 3. ล้างสกิล
        //if (playerLoadout != null) playerLoadout.loadoutData.loadoutItems.Clear();
        //if (playerSkillBar != null)
        //{
        //    for (int i = 0; i < playerSkillBar.skillDatas.Length; i++)
        //    {
        //        playerSkillBar.ResetSkill(i);
        //    }
        //}

        // 4. ล้างบัฟ
        //if (playerBuffManager != null) playerBuffManager.ClearAllBuffs();

        // 5. ล้างเมนู
        //if (MenuIndexManager.Instance != null) MenuIndexManager.Instance.GetFinishedMenus().Clear();

        // 6. ล้างเควส
        //if (QuestEventManager.Instance != null) QuestEventManager.Instance.triggeredRewards.Clear();

        Debug.Log("[SaveManager] เริ่มเกมใหม่: //รีเซ็ตข้อมูลและ ScriptableObject ทั้งหมดแล้ว!");
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
        SavePlayPositionAndHp();
        SavePlayInventory();
        SaveBaseStorage();
        SaveMenuProgress();
        SavePlayerBuffs();
        SaveLoadoutSkills();
        SaveSkillBar();
        SaveQuestAndWorldState();
    }

    private void LoadFormat()
    {
        LoadPlayPositionAndHp();
        LoadPlayInventory();
        LoadBaseStorage();
        LoadMenuProgress();
        LoadPlayerBuffs();
        LoadLoadoutSkills();
        LoadSkillBar();
        LoadQuestAndWorldState();
    }

    private void SavePlayPositionAndHp()
    {
        // --- ข้อมูลผู้เล่น ---------------------------------------------------------------
        _currentData.currentHealth = playerHealth.currentHealth;
        //_currentData.playerPosition[0] = playerHealth.transform.position.x;
        //_currentData.playerPosition[1] = playerHealth.transform.position.y;
        //_currentData.playerPosition[2] = playerHealth.transform.position.z;
    }

    private void LoadPlayPositionAndHp()
    {
        // --- โหลดข้อมูลผู้เล่น ---------------------------------------------------------------
        playerHealth.setHp(_currentData.currentHealth);

        //if (playerHealth.TryGetComponent(out CharacterController controller))
        //{
        //    controller.enabled = false;
        //    playerHealth.transform.position = new Vector3(_currentData.playerPosition[0], _currentData.playerPosition[1], _currentData.playerPosition[2]);
        //    controller.enabled = true;
        //}
        //else
        //{
        //    playerHealth.transform.position = new Vector3(_currentData.playerPosition[0], _currentData.playerPosition[1], _currentData.playerPosition[2]);
        //}
    }

    private void SavePlayInventory()
    {
        // --- ข้อมูลกระเป๋า ---------------------------------------------------------------
        _currentData.inventoryItems.Clear(); // ล้างของเก่าในไฟล์เซฟก่อน

        for (int i = 0; i < playerInventory.Size; i++)
        {
            var invItem = playerInventory.GetItemAt(i);

            if (!invItem.IsEmpty) // ถ้าช่องนั้นมีของ
            {
                _currentData.inventoryItems.Add(new GameSaveData.SavedItem
                {
                    slotIndex = i,
                    itemID = invItem.item.ID, // เซฟแค่ ID (String)
                    amount = invItem.quantity
                });
            }
        }
    }

    private void LoadPlayInventory()
    {
        // --- โหลดข้อมูลกระเป๋า ---------------------------------------------------------------
        playerInventory.Initialize(); // เคลียร์ของในกระเป๋าก่อนโหลด

        foreach (var savedItem in _currentData.inventoryItems)
        {
            // เอา ID ไปค้นหา ItemSO ตัวจริงจาก Database
            ItemSO foundItemSO = itemDatabase.GetItemByID(savedItem.itemID);

            if (foundItemSO != null)
            {
                // สร้างกล่องไอเทมขึ้นมาใหม่
                InventoryItem loadedItem = new InventoryItem
                {
                    item = foundItemSO,
                    quantity = savedItem.amount
                };

                // ยัดกลับลงไปในช่อง (Slot) เดิมเป๊ะๆ
                playerInventory.SetItemAt(savedItem.slotIndex, loadedItem);
            }
            else
            {
                Debug.LogWarning($"[SaveManager] หาไอเทม ID: {savedItem.itemID} ไม่เจอใน Database!");
            }
        }
    }

    private void SaveBaseStorage()
    {
        if (baseStorageInventory == null) return;

        // --- ข้อมูล Storage ---------------------------------------------------------------
        _currentData.storageItems.Clear(); // ล้างของเก่า

        for (int i = 0; i < baseStorageInventory.Size; i++)
        {
            var invItem = baseStorageInventory.GetItemAt(i);

            if (!invItem.IsEmpty)
            {
                _currentData.storageItems.Add(new GameSaveData.SavedItem
                {
                    slotIndex = i,
                    itemID = invItem.item.ID,
                    amount = invItem.quantity
                });
            }
        }
    }

    private void LoadBaseStorage()
    {
        if (baseStorageInventory == null) return;

        // --- โหลดข้อมูล Storage ---------------------------------------------------------------
        baseStorageInventory.Initialize();

        foreach (var savedItem in _currentData.storageItems)
        {
            ItemSO foundItemSO = itemDatabase.GetItemByID(savedItem.itemID);

            if (foundItemSO != null)
            {
                InventoryItem loadedItem = new InventoryItem
                {
                    item = foundItemSO,
                    quantity = savedItem.amount
                };

                baseStorageInventory.SetItemAt(savedItem.slotIndex, loadedItem);
            }
            else
            {
                Debug.LogWarning($"[SaveManager] หาไอเทม Storage ID: {savedItem.itemID} ไม่เจอใน Database!");
            }
        }
    }

    private void SaveMenuProgress()
    {
        if (MenuIndexManager.Instance == null) return;

        // --- ข้อมูลเมนูอาหารที่ปลดล็อคแล้ว ----------------------------------------------------
        _currentData.finishedMenuIDs.Clear();

        foreach (var recipe in MenuIndexManager.Instance.GetFinishedMenus())
        {
            _currentData.finishedMenuIDs.Add(recipe.ID); // เซฟเฉพาะ ID
        }
    }

    private void LoadMenuProgress()
    {
        if (MenuIndexManager.Instance == null) return;

        // --- โหลดข้อมูลเมนูอาหาร ----------------------------------------------------
        MenuIndexManager.Instance.LoadFinishedMenusFromSave(_currentData.finishedMenuIDs);
    }

    private void SavePlayerBuffs()
    {
        if (playerBuffManager == null) return;

        _currentData.activeBuffs.Clear();
        foreach (var buff in playerBuffManager.GetActiveBuffs())
        {
            _currentData.activeBuffs.Add(new GameSaveData.SavedBuff
            {
                buffID = buff.data.ID,
                timeRemaining = buff.durationTimer,
                stacks = buff.currentStacks
            });
        }
    }

    private void LoadPlayerBuffs()
    {
        if (playerBuffManager == null) return;

        // ล้างบัฟเก่าบนตัวทิ้งก่อนโหลดของใหม่
        playerBuffManager.ClearAllBuffs();

        foreach (var savedBuff in _currentData.activeBuffs)
        {
            BuffSO foundBuffSO = buffDatabase.GetBuffByID(savedBuff.buffID);

            if (foundBuffSO != null)
            {
                // ส่งข้อมูลให้ Manager เสกบัฟกลับมาพร้อมเวลาและ stack ที่เหลือ
                playerBuffManager.RestoreBuff(foundBuffSO, savedBuff.timeRemaining, savedBuff.stacks);
            }
            else
            {
                Debug.LogWarning($"[SaveManager] หาบัฟ ID: {savedBuff.buffID} ไม่เจอใน Database!");
            }
        }
    }

    private void SaveLoadoutSkills()
    {
        if (playerLoadout == null) return;

        _currentData.acquiredSkills.Clear();

        // ดึงของในกระเป๋า Loadout ออกมาเซฟ
        foreach (var item in playerLoadout.loadoutData.loadoutItems)
        {
            if (item.skill != null)
            {
                _currentData.acquiredSkills.Add(new GameSaveData.SavedLoadoutSkill
                {
                    skillID = item.skill.ID,
                    exp = item.exp
                });
            }
        }
    }

    private void LoadLoadoutSkills()
    {
        if (playerLoadout == null) return;

        // ล้างสกิลเก่าใน Loadout ทิ้งให้หมดก่อน
        playerLoadout.loadoutData.loadoutItems.Clear();

        // ดึงข้อมูลจากไฟล์เซฟมาประกอบร่าง
        foreach (var savedSkill in _currentData.acquiredSkills)
        {
            PlayerSkillSO foundSkill = skillDatabase.GetSkillByID(savedSkill.skillID);

            if (foundSkill != null)
            {
                // สร้างไอเทมสกิลแล้วยัดกลับเข้าไป
                playerLoadout.loadoutData.loadoutItems.Add(new PlayerLoadoutSkill.LoadoutData.loadoutItem
                {
                    skill = foundSkill,
                    exp = savedSkill.exp
                });
            }
            else
            {
                Debug.LogWarning($"[SaveManager] หาสกิล ID: {savedSkill.skillID} ไม่เจอใน Database!");
            }
        }

        // สั่งให้กระเป๋าสกิลแจ้งเตือน UI เพื่ออัปเดตภาพทันที
        playerLoadout.loadoutData.InformAboutChange();
    }

    private void SaveSkillBar()
    {
        if (playerSkillBar == null) return;

        _currentData.skillBarItems.Clear();

        // วนลูปเช็คตามจำนวนช่องใน Skill Bar (ของหน้าต่าง UI)
        for (int i = 0; i < playerSkillBar.skillDatas.Length; i++)
        {
            var skillData = playerSkillBar.skillDatas[i];

            // ถ้าช่องนั้นมีสกิลติดตั้งอยู่
            if (!skillData.IsEmpty && skillData.assignedSkills != null)
            {
                _currentData.skillBarItems.Add(new GameSaveData.SavedSkillBarItem
                {
                    slotIndex = i,
                    skillID = skillData.assignedSkills.ID,
                    usedCount = skillData.uesdCount
                });
            }
        }
    }

    private void LoadSkillBar()
    {
        if (playerSkillBar == null) return;

        // *** เพิ่มบรรทัดนี้: บังคับให้ PlayerSkill สร้างกล่องสกิล (Length = 3) ก่อนโหลด ***
        playerSkillBar.Initialize();

        // 1. ล้างช่องสกิลบาร์ทั้งหมดให้เป็นช่องว่างก่อนโหลดของใหม่
        for (int i = 0; i < playerSkillBar.skillDatas.Length; i++)
        {
            playerSkillBar.ResetSkill(i);
        }

        // 2. ดึงข้อมูลจากไฟล์เซฟมาประกอบร่าง
        foreach (var savedItem in _currentData.skillBarItems)
        {
            PlayerSkillSO foundSkill = skillDatabase.GetSkillByID(savedItem.skillID);

            if (foundSkill != null)
            {
                // ใช้ฟังก์ชัน SetAtSkill เพื่อยัดสกิลกลับเข้าช่องเดิมเป๊ะๆ
                playerSkillBar.SetAtSkill(foundSkill, savedItem.usedCount, savedItem.slotIndex);
            }
            else
            {
                Debug.LogWarning($"[SaveManager] หาสกิลบาร์ ID: {savedItem.skillID} ไม่เจอใน Database!");
            }
        }
    }

    private void SaveQuestAndWorldState()
    {
        // 1. เซฟเควส NPC ทั้งหมดในฉาก
        _currentData.npcQuestProgress.Clear();

        if (QuestEventManager.Instance != null)
        {
            // *** ใช้ List จาก Database แทนการใช้ FindObjectsOfType ***
            foreach (var npc in QuestEventManager.Instance.allNPCsInScene)
            {
                if (!string.IsNullOrEmpty(npc.npcID))
                {
                    _currentData.npcQuestProgress.Add(new GameSaveData.SavedNPCQuest
                    {
                        npcID = npc.npcID,
                        questIndex = npc.GetQuestIndex(),
                        stepIndex = npc.GetStepIndex()
                    });
                }
            }
        }

        // 2. เซฟพื้นที่/สิ่งของ (World State)
        _currentData.triggeredRewardIDs.Clear();
        if (QuestEventManager.Instance != null)
        {
            foreach (var rewardID in QuestEventManager.Instance.triggeredRewards)
            {
                _currentData.triggeredRewardIDs.Add(rewardID.ToString());
            }
        }
    }

    private void LoadQuestAndWorldState()
    {
        // 1. โหลดเควส NPC
        if (QuestEventManager.Instance != null)
        {
            foreach (var savedNPC in _currentData.npcQuestProgress)
            {
                // *** วนลูปหาใน List ของ Database ***
                foreach (var npc in QuestEventManager.Instance.allNPCsInScene)
                {
                    if (npc.npcID == savedNPC.npcID)
                    {
                        npc.LoadQuestState(savedNPC.questIndex, savedNPC.stepIndex);
                        break;
                    }
                }
            }
        }

        // 2. โหลดพื้นที่/สิ่งของ (World State)
        if (QuestEventManager.Instance != null)
        {
            QuestEventManager.Instance.LoadTriggeredRewards(_currentData.triggeredRewardIDs);
        }
    }
}