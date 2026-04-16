using System;
using UnityEngine;
using Inventory.Model; // สำหรับดึง ItemSO มาแจก

[Serializable]
public abstract class QuestRewardBase
{
    // สั่งแจกรางวัล
    public abstract void GiveReward(GameObject player);

    // เอาไว้โชว์บน UI เควส เช่น "รางวัล: เงิน 500G"
    public abstract string GetRewardText();
}

// --- หมวดที่ 1: แจกไอเทม (Item Reward) ---
[Serializable]
public class ItemQuestReward : QuestRewardBase
{
    public ItemSO item;
    public int amount = 1;

    public override void GiveReward(GameObject player)
    {
        if (player.TryGetComponent(out Inventory.InventoryController inv))
        {
            inv.InventoryData.AddItem(item, amount);
            Debug.Log($"แจกรางวัล: {item.ItemName} จำนวน {amount} ชิ้น");
        }
    }

    public override string GetRewardText() => $"{item.ItemName} x{amount}";
}

// --- หมวดที่ 2: แจกเงิน (Currency Reward) ---
[Serializable]
public class CurrencyQuestReward : QuestRewardBase
{
    public int goldAmount;

    public override void GiveReward(GameObject player)
    {
        // สมมติคุณมีสคริปต์จัดการเงิน
        // player.GetComponent<PlayerWallet>().AddGold(goldAmount);
        Debug.Log($"ได้รับเงิน: {goldAmount} G");
    }

    public override string GetRewardText() => $"เงิน {goldAmount} G";
}

// --- หมวดที่ 3: สั่งรัน Event (Event Reward) ---
// ** เอา Enum RewardID ที่เราทำไว้มาใช้ตรงนี้แหละครับ! **
[Serializable]
public class EventQuestReward : QuestRewardBase
{
    public RewardID eventID; // เช่น UnlockPotStation, OpenGate

    public override void GiveReward(GameObject player)
    {
        QuestEventManager.Instance.TriggerReward(eventID);
    }

    public override string GetRewardText() => $"ปลดล็อคระบบใหม่!"; // หรือข้อความอะไรก็ได้
}