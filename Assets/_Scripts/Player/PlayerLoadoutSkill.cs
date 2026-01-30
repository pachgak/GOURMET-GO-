using com.cyborgAssets.inspectorButtonPro;
using Inventory.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UnityEngine;
using static PlayerLoadoutSkill;
using static PlayerLoadoutSkill.LoadoutData;
using static PlayerSkill;
using static UnityEditor.Progress;
using static UnityEditor.Timeline.Actions.MenuPriority;

public class PlayerLoadoutSkill : MonoBehaviour
{
    private PlayerSkill _playerSkill;

    public UILoadoutSkillPage loadoutUI;
    public LoadoutData loadoutData;

    //Size
    public AttacksSkill[] baseSkillList;

    public List<loadoutItem> initialItems = new List<loadoutItem>();

    [System.Serializable]
    public class LoadoutData
    {
        public List<loadoutItem> loadoutItems;

        public event Action<List<loadoutItem>> OnLoadoutUpdated;

        [System.Serializable]
        public struct loadoutItem
        {
            public AttacksSkill skill;
            public int exp;
            public bool IsEmpty => skill == null;

            public loadoutItem ChangeQuantity(int newQuantity)
            {
                return new loadoutItem
                {
                    skill = this.skill,
                    exp = newQuantity,
                    //itemParameter = new List<ItemParameter>(this.itemParameter)
                };
            }

            public static loadoutItem GetEmptyItem()
            => new loadoutItem
            {
                skill = null,
                exp = 0,
                //itemParameter = new List<ItemParameter>()
            };

            public static loadoutItem GetNewItem(AttacksSkill newSkill, int newExp)
            => new loadoutItem
            {
                skill = newSkill,
                exp = newExp,
                //itemParameter = new List<ItemParameter>()
            };
        }

        public void InitializeData(AttacksSkill[] skillList)
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

        public int AddItem(AttacksSkill item, int exp)
        {
            int initialQuantity = exp;
            int collectedQuantity = 0;

            exp = AddStackableItem(item, exp);
            InformAboutChange();

            collectedQuantity = initialQuantity - exp;
            //OnAddItem?.Invoke(item, collectedQuantity);
            return exp;
        }

        private int AddStackableItem(AttacksSkill skill, int quantity)
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
            //OnLoadoutUpdated?.Invoke(GetCurrentInventoryState());
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

    [ProButton]
    public void TestInformAboutData()
    {
        loadoutData.InformAboutChange();
    }

    private void Awake()
    {
        _playerSkill = GetComponent<PlayerSkill>();
    }

    private void Start()
    {
        loadoutData = new LoadoutData();

        //ห้ามสลับ data ก่อน UI เพราะ loadoutUI.InitializeUI(baseSkillList.Length); มาหลังแล้วมันจะ ผิดพลาด
        PrepareData();
        PrepareUI();

        UpdateUI(loadoutData.loadoutItems);

        //Invoke(nameof(PrepareSkillPage), 0.01f);
        PrepareSkillPage();
    }


    private void PrepareSkillPage()
    {
        _playerSkill.skillUI.OnSwapItems += HandleAddSkillBar;
    }

    private void HandleAddSkillBar(int currentlyDraggedItemIndex,int targetDrop)
    {
        int loadoutIndex = loadoutUI.GetCurrentlyDraggedItemIndex();

        if (loadoutIndex <= -1)
            return;

        int skillBarIndex = targetDrop;

        loadoutItem inventoryItem = loadoutData.GetItemAt(loadoutIndex);

        _playerSkill.SetAtSkill(inventoryItem.skill, 1, skillBarIndex);
    }

    // Update is called once per frame
    void Update()
    {

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
        //loadoutUI.OnSwapItems += HandleSwapItems;
        loadoutUI.OnStartDragging += HandleDragging;
        //loadoutUI.OnItemActionRequested += HandleItemActionRequest;
        //pageUI.OnItemPerformAction += HandleItemPerformAction;
        loadoutUI.OnPointEnterItem += HandlePointEnterItem;
        loadoutUI.OnPointExitItem += HandlePointExitItem;

        //loadoutUI.OnDropItems += HandleDropItem;
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

        /*
        //foreach (var item in loadoutItems)
        //{

        //    loadoutUI.UpdateData(
        //        item.Key, 
        //        item.Value.skill.skillIcon,
        //        item.Value.exp);
        //}
        */


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

        AttacksSkill skill = inventoryItem.skill;
        string description = PrepareDescription(inventoryItem);

        loadoutUI.OpenItemDescription();

        loadoutUI.UpdateItemDescription(skill.skillIcon, skill.name, description);
    }

    private string PrepareDescription(loadoutItem inventoryItem)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(inventoryItem.skill.Description);
        sb.AppendLine();
        //for (int i = 0; i < inventoryItem.itemParameter.Count; i++)
        //{
        //    sb.Append($"{inventoryItem.itemParameter[i].itemParameterSO.ParameterName} " +
        //        $": {inventoryItem.itemParameter[i].value} / " +
        //        $"{inventoryItem.item.DefaultParametersList[i].value}");
        //    sb.AppendLine();
        //}
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
