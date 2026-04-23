using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// --- 1. สร้างรายชื่อ ID ตรงนี้ (เพิ่มลดได้ตามใจชอบ) ---
public enum RewardID
{
    None,               // ไม่มีรางวัล
    CanFindGo,
    UnlockPotStation,   // ปลดล็อคหม้อ
    OpenNorthGate,      // เปิดประตูทิศเหนือ
    OpenEastGate,      // เปิดประตูทิศตะวันตก
    UnlockMenuIdex,
    EndClear,          // จบเกม
    C_Station2,
    C_StationFull,
}

public class QuestEventManager : MonoBehaviour
{
    public static QuestEventManager Instance { get; private set; }

    [Header("NPC Database")]
    [HideInInspector] // ซ่อนไว้เพราะเราจะให้โค้ดมัน Add ตัวเองอัตโนมัติ ไม่ต้องลากใส่
    public List<NPCQuestGiver> allNPCsInScene = new List<NPCQuestGiver>();

    [Header("Map Objects To Toggle")]
    public List<GameObject> enableObject;
    public List<GameObject> diableObject;

    [System.Serializable]
    public class QuestReward
    {
        public RewardID rewardID;
        public UnityEvent onRewardClaimed;
    }

    [Header("Global Quest Rewards")]
    public List<QuestReward> allRewardEvents;

    // เก็บรายชื่อ Reward ที่ถูกปลดล็อคไปแล้ว
    [HideInInspector]
    public List<RewardID> triggeredRewards = new List<RewardID>();

    // *** เพิ่มตัวแปรสำหรับหน้าจอจบเกมตรงนี้ ***
    [Header("End Game Settings")]
    [Tooltip("ลาก UI หน้าจบเกม (newOpenUIController) มาใส่ตรงนี้")]
    public GameObject endGameUIController;


    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        foreach (var obj in enableObject) { obj.SetActive(true); }
        foreach (var obj in diableObject) { obj.SetActive(false); }

        endGameUIController.SetActive(false);
    }

    public void TriggerReward(RewardID targetRewardID)
    {
        if (targetRewardID == RewardID.None) return;

        // จดจำว่า Reward นี้ถูกปลดล็อคแล้ว (ถ้ายังไม่เคยจด)
        if (!triggeredRewards.Contains(targetRewardID))
        {
            triggeredRewards.Add(targetRewardID);
        }

        foreach (var reward in allRewardEvents)
        {
            if (reward.rewardID == targetRewardID)
            {
                reward.onRewardClaimed?.Invoke();
                Debug.Log($"[QuestManager] แจกรางวัลสำเร็จ: {targetRewardID}");
                return;
            }
        }
        Debug.LogWarning($"[QuestManager] หา Reward ID ไม่เจอ: {targetRewardID}");
    }

    // --- ฟังก์ชันสำหรับโหลดเกม (สั่งปลดล็อคแมพที่เคยทำไว้ใหม่) ---
    public void LoadTriggeredRewards(List<string> savedRewardStrings)
    {
        triggeredRewards.Clear();
        foreach (string rewardStr in savedRewardStrings)
        {
            // แปลงข้อความ String กลับเป็น Enum RewardID
            if (System.Enum.TryParse(rewardStr, out RewardID parsedID))
            {
                TriggerReward(parsedID); // สั่งยิง Event เพื่อเปิดประตู/สิ่งของทันที
            }
        }
    }

    // ==========================================
    // *** Method สำหรับเรียกตอนจบเกม (เอาไปผูกกับ UnityEvent ได้เลย) ***
    // ==========================================
    public void TriggerEndGameClear()
    {
        Debug.Log(" [QuestManager] จบเกมแล้ว! ปิดการควบคุมและโชว์หน้า End Game");

        // 1. ปิดการควบคุมทั้งหมดของผู้เล่น (รวมถึงปุ่ม UI อื่นๆ ด้วย ป้องกันผู้เล่นกด Esc หรือเดินเล่น)
        if (PlayerInputActionsManager.instance != null)
        {
            PlayerInputActionsManager.instance.playerControls.Disable();
        }

        // 2. สั่งเปิดหน้า UI จบเกมผ่าน UI Manager
        if (endGameUIController != null)
        {
            endGameUIController.SetActive(true);
        }
    }
}