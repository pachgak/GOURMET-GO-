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
    EndClear           // จบเกม
}

public class QuestEventManager : MonoBehaviour
{
    public static QuestEventManager Instance { get; private set; }

    [System.Serializable]
    public class QuestReward
    {
        // --- 2. เปลี่ยนจาก string เป็น RewardID ---
        public RewardID rewardID;
        public UnityEvent onRewardClaimed;
    }

    [Header("Global Quest Rewards")]
    public List<QuestReward> allRewardEvents;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    // --- 3. เปลี่ยน Parameter เป็น RewardID ---
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
}