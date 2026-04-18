using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static PlayerLoadoutSkill.LoadoutData;

public class PlayerLoadoutSkill : MonoBehaviour
{
    private PlayerSkill _playerSkill;

    public UILoadoutSkillPage loadoutUI;
    public LoadoutData loadoutData;

    // *** 1. เปลี่ยนจาก AttacksSkill[] เป็น PlayerSkillSO[] เพื่อให้รับได้ทั้งสกิลเก่าและใหม่ ***
    public PlayerSkillSO[] baseSkillList;

    public List<loadoutItem> initialItems = new List<loadoutItem>();

    [System.Serializable]
    public class LoadoutData
    {
        public List<loadoutItem> loadoutItems;

        public event Action<List<loadoutItem>> OnLoadoutUpdated;

        [System.Serializable]
        public struct loadoutItem
        {
            // *** 2. เปลี่ยนเป็น PlayerSkillSO ***
            public PlayerSkillSO skill;
            public int exp;
            public bool IsEmpty => skill == null;

            public loadoutItem ChangeQuantity(int newQuantity)
            {
                return new loadoutItem
                {
                    skill = this.skill,
                    exp = newQuantity,
                };
            }

            public static loadoutItem GetEmptyItem()
            => new loadoutItem
            {
                skill = null,
                exp = 0,
            };

            // *** 3. เปลี่ยน Parameter เป็น PlayerSkillSO ***
            public static loadoutItem GetNewItem(PlayerSkillSO newSkill, int newExp)
            => new loadoutItem
            {
                skill = newSkill,
                exp = newExp,
            };
        }

        // *** 4. เปลี่ยน Parameter เป็น PlayerSkillSO[] ***
        public void InitializeData(PlayerSkillSO[] skillList)
        {
            loadoutItems = new List<loadoutItem>();
            for (int i = 0; i < skillList.Length; i++)
            {
                if (skillList[i] == null) continue;

                loadoutItem loadoutskill = loadoutItem.GetEmptyItem();
                loadoutskill.skill = skillList[i];

                loadoutItems.Add(loadoutskill);
            }
        }

        public loadoutItem GetItemAt(int itemIndex)
        {
            return loadoutItems[itemIndex];
        }

        public void AddItem(loadoutItem item)
        {
            AddItem(item.skill, item.exp);
        }

        // *** 5. เปลี่ยน Parameter เป็น PlayerSkillSO ***
        public int AddItem(PlayerSkillSO item, int exp)
        {
            int initialQuantity = exp;
            int collectedQuantity = 0;

            exp = AddStackableItem(item, exp);
            InformAboutChange();

            collectedQuantity = initialQuantity - exp;
            return exp;
        }

        // *** 6. เปลี่ยน Parameter เป็น PlayerSkillSO ***
        private int AddStackableItem(PlayerSkillSO skill, int quantity)
        {
            for (int i = 0; i < loadoutItems.Count; i++)
            {
                if (loadoutItems[i].IsEmpty)
                    continue;
                if (loadoutItems[i].skill == skill)
                {

                    loadoutItems[i] = loadoutItems[i]
                        .ChangeQuantity(loadoutItems[i].exp + quantity);

                    InformAboutChange();
                    return 0;

                }
            }

            loadoutItem newSkill = loadoutItem.GetNewItem(skill, quantity);

            loadoutItems.Add(newSkill);

            InformAboutChange();
            return 0;
        }

        public void InformAboutChange()
        {
            OnLoadoutUpdated?.Invoke(loadoutItems);
        }

        public Dictionary<int, loadoutItem> GetCurrentInventoryState()
        {
            Dictionary<int, loadoutItem> returnValue =
                new Dictionary<int, loadoutItem>();

            for (int i = 0; i < loadoutItems.Count; i++)
            {
                if (loadoutItems[i].IsEmpty)
                    continue;
                returnValue[i] = loadoutItems[i];
            }
            return returnValue;
        }

    }

    private void Awake()
    {
        _playerSkill = GetComponent<PlayerSkill>();
    }

    private void Start()
    {
        loadoutData = new LoadoutData();

        loadoutUI.CleanLoadoutSlot();

        PrepareData();
        PrepareUI();

        UpdateUI(loadoutData.loadoutItems);

        PrepareSkillPage();
    }


    private void PrepareSkillPage()
    {
        _playerSkill.skillUI.OnSwapItems += HandleAddSkillBar;
    }

    private void HandleAddSkillBar(int currentlyDraggedItemIndex, int targetDrop)
    {
        int loadoutIndex = loadoutUI.GetCurrentlyDraggedItemIndex();

        if (loadoutIndex <= -1)
            return;

        int skillBarIndex = targetDrop;

        loadoutItem inventoryItem = loadoutData.GetItemAt(loadoutIndex);

        _playerSkill.SetAtSkill(inventoryItem.skill, 1, skillBarIndex);
    }

    private void PrepareData()
    {
        loadoutData.InitializeData(baseSkillList);
        loadoutData.OnLoadoutUpdated += UpdateUI;

        foreach (loadoutItem item in initialItems)
        {
            if (item.skill == null)
                continue;
            loadoutData.AddItem(item);
        }
    }

    private void PrepareUI()
    {
        loadoutUI.InitializeUI(baseSkillList.Length);

        loadoutUI.OnItemSelection += HandleSelect;
        loadoutUI.OnStartDragging += HandleDragging;
        loadoutUI.OnPointEnterItem += HandlePointEnterItem;
        loadoutUI.OnPointExitItem += HandlePointExitItem;
    }

    private void UpdateUI(List<loadoutItem> loadoutItems)
    {
        if (loadoutItems.Count != loadoutUI.GetListOfUIItems())
        {
            loadoutUI.InitializeUI(loadoutItems.Count);
        }

        loadoutUI.ResetAllItems();

        for (int i = 0; i < loadoutItems.Count; i++)
        {
            loadoutUI.UpdateData(
                i,
                loadoutItems[i].skill.skillIcon,
                loadoutItems[i].exp);
        }
    }

    private void HandleSelect(int itemIndex)
    {
        loadoutItem inventoryItem = loadoutData.GetItemAt(itemIndex);
        if (inventoryItem.IsEmpty)
        {
            loadoutUI.ResetSelection();
            return;
        }

        loadoutUI.UpdateSelect(itemIndex);
    }

    private void HandleDragging(int itemIndex)
    {
        loadoutItem inventoryItem = loadoutData.GetItemAt(itemIndex);
        if (inventoryItem.IsEmpty)
            return;

        loadoutUI.CreateDraggedItem(inventoryItem.skill.skillIcon, inventoryItem.exp);
    }

    private void HandlePointEnterItem(int itemIndex)
    {
        loadoutItem inventoryItem = loadoutData.GetItemAt(itemIndex);
        if (inventoryItem.IsEmpty)
            return;

        // *** 7. เปลี่ยนเป็น PlayerSkillSO ***
        PlayerSkillSO skill = inventoryItem.skill;
        string description = PrepareDescription(inventoryItem);

        loadoutUI.OpenItemDescription();

        loadoutUI.UpdateItemDescription(skill.skillIcon, skill.name, description);
    }

    private string PrepareDescription(loadoutItem inventoryItem)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(inventoryItem.skill.Description);
        sb.AppendLine();
        return sb.ToString();
    }

    private void HandlePointExitItem(int itemIndex)
    {
        loadoutItem inventoryItem = loadoutData.GetItemAt(itemIndex);
        if (inventoryItem.IsEmpty)
            return;

        loadoutUI.CloseItemDescription();
    }
}