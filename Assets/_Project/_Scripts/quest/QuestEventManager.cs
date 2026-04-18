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
        if (targetRewardID == RewardID.None) return; // ถ้าเป็น None ก็ไม่ต้องทำอะไร

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