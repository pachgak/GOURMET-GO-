using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using static SettingPlayerControllerManager;

public class PlayerSkill : MonoBehaviour
{
    public UISkilPage skillUI;
    public UISkillBarItem[] UISkillBarItems = new UISkillBarItem[0];

    public List<SkillData> initialSkills;
    // ใช้ Array ของ Skill ScriptableObject
    public SkillData[] skillDatas = new SkillData[0];
    //public int skillSize = 5;
    public float useSkillDely = 0f;

    [Header("System")]
    private Vector3 _mousePosition;
    private bool _isSkilling = false;
    private bool _canSkill = true;
    private float _canSkillDelyTimer;
    [HideInInspector] public Coroutine _skillStepCoroutine;

    [Header("_Scripts References")]
    private PlayerMovement _playerMovement;
    [Header("_Manager References")]
    private PlayerInputActionsManager _inputManager;
    private SettingPlayerControllerManager _settingControllerManager;

    private bool _isDash;


    public Action<bool> OnCanSkillUseStateChange;
    public Action<bool,float> OnSkillingStateChange;
    public Action OnInventoryUpdated;

    private void Awake()
    {
        _inputManager = PlayerInputActionsManager.instance;
        _settingControllerManager = SettingPlayerControllerManager.instance;
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        _inputManager.OnSkillInput += HandleSkillSlotInput;
        _inputManager.OnMountPosition += HandleGetMountPos;

        _playerMovement.OnDashSkillCancelInput += HandleDashSkillCancelInput;
        _playerMovement.OnDashStateChange += HandleDashStateChange;
    }

    private void OnDisable()
    {
        _inputManager.OnSkillInput -= HandleSkillSlotInput;
        _inputManager.OnMountPosition -= HandleGetMountPos;

        _playerMovement.OnDashSkillCancelInput -= HandleDashSkillCancelInput;
        _playerMovement.OnDashStateChange -= HandleDashStateChange;
    }

    internal void HandleDashStateChange(bool isState, Vector3 vector)
    {
        _isDash = isState;

        if (_isDash)
        {
            if(_skillStepCoroutine != null) StopCoroutine(_skillStepCoroutine);
            DoSkillEnd();

        }
    }

    internal void HandleDashSkillCancelInput()
    {
        DoSkillEnd();
    }

    internal void HandleGetMountPos(Vector3 mousePosition)
    {
        _mousePosition = mousePosition;
    }

    internal void HandleSkillSlotInput(int slot)
    {
        Debug.Log($"slot : {slot}");
        if (slot > skillDatas.Length) return;
        if (_isSkilling) return;
        if (!_canSkill) return;
        if (skillDatas[slot - 1].assignedSkills == null) return;
        if (skillDatas[slot - 1].cooldown > 0) return;

            SetSkillingState(true, skillDatas[slot - 1].assignedSkills.skillLifeTime);

        Vector3 targetPosition = Vector3.forward;
        switch (_settingControllerManager.skillDiraction)
        {
            case AttackDiractionType.movement:
                targetPosition = transform.position + (_playerMovement.lastMoveDirection);
                Debug.Log($"movement {targetPosition}");
                break;
            case AttackDiractionType.mouse:
                targetPosition = _mousePosition;
                Debug.Log($"mouse {targetPosition}");
                break;

        }

        _skillStepCoroutine = skillDatas[slot - 1].assignedSkills.Use(gameObject, targetPosition);
        Invoke(nameof(DoSkillEnd), skillDatas[slot - 1].assignedSkills.skillLifeTime);
        //skillDatas[slot - 1].uesdCount--;
        skillDatas[slot - 1].cooldown = skillDatas[slot - 1].assignedSkills.cooldown;

        skillDatas[slot - 1].cooldownCoroutine = StartCoroutine(SetCountDownCooldown(slot - 1, skillDatas[slot - 1].assignedSkills.cooldown));

        if (skillDatas[slot - 1].uesdCount <= 0)
        {
            ResetSkill(slot - 1);
        }

        _canSkill = false;
        _canSkillDelyTimer = useSkillDely;
        OnCanSkillUseStateChange?.Invoke(_canSkill);

        InformAboutChange();

        //float skillLifeTime = assignedSkills[slot - 1].skillLifeTime;
        //_skillingEndTimer = skillLifeTime;
    }

    internal IEnumerator SetCountDownCooldown(int index, float initialCooldown)
    {
        // 1. กำหนดค่าเริ่มต้น Cooldown (ถ้าจำเป็น)
        skillDatas[index].cooldown = initialCooldown;

        while (skillDatas[index].cooldown > 0)
        {
            // 3. ลดค่า cooldown
            // Time.deltaTime คือเวลาที่ผ่านไปตั้งแต่เฟรมที่แล้ว (ขึ้นอยู่กับ Time Scale)
            skillDatas[index].cooldown -= Time.deltaTime;
            skillUI.UpdateCooldown(index, skillDatas[index].cooldown);
            // 4. รอจนกว่าจะถึงเฟรมถัดไป
            yield return null;
        }

        // 5. เมื่อ cooldown จบแล้ว ตรวจสอบให้แน่ใจว่าค่าเป็น 0
        skillDatas[index].cooldown = 0;
    }

    private void Start()
    {
        PrepareUI();
        PrepareSkillData();
    }

    void Update()
    {
        if (!_canSkill && !_isSkilling)
        {
            _canSkillDelyTimer -= Time.deltaTime;
            if (_canSkillDelyTimer <= 0)
            {
                _canSkill = true;
                OnCanSkillUseStateChange?.Invoke(_canSkill);
            } 
        }
    }

    public void Initialize()
    {
        skillDatas = new SkillData[UISkillBarItems.Length];
    }

    public void SetSkillingState(bool isState , float skillLifeTime)
    {
        _isSkilling = isState;
        OnSkillingStateChange?.Invoke(isState, skillLifeTime);
    }

    public void DoSkillEnd()
    {
        SetSkillingState(false,0);
    }

    public bool AddSkill(PlayerSkillSO skill,int usedCount)
    {
        for (int i = 0; i < skillDatas.Length; i++) 
        {
            if (skillDatas[i].assignedSkills == null)
            {
                skillDatas[i].assignedSkills = skill;
                skillDatas[i].uesdCount = usedCount;

                InformAboutChange();
                return true;
            }
        }
        return false;
    }

    public void ResetSkill(int Slot)
    {
        skillDatas[Slot] = new SkillData();
        InformAboutChange();
    }

    [Serializable]
    public struct SkillData
    {
        public PlayerSkillSO assignedSkills;
        public int uesdCount;
        public float cooldown;
        public Coroutine cooldownCoroutine;
        public bool IsEmpty => assignedSkills == null;
    }

    public void SwapItems(int itemIndex_1, int itemIndex_2)
    {
        SkillData item1 = skillDatas[itemIndex_1];
        skillDatas[itemIndex_1] = skillDatas[itemIndex_2];
        skillDatas[itemIndex_2] = item1;

        if (skillDatas[itemIndex_1].assignedSkills != null) skillDatas[itemIndex_1].cooldownCoroutine = StartCoroutine(SetCountDownCooldown(itemIndex_1, skillDatas[itemIndex_1].cooldown));
        if (skillDatas[itemIndex_2].assignedSkills != null) skillDatas[itemIndex_2].cooldownCoroutine = StartCoroutine(SetCountDownCooldown(itemIndex_2, skillDatas[itemIndex_2].cooldown));

        InformAboutChange();
    }

    private void InformAboutChange()
    {
        UpdateSkillUI();
    }

    private void PrepareSkillData()
    {
        Initialize();

        foreach (SkillData skill in initialSkills)
        {
            if (skill.IsEmpty)
                continue;
            AddSkill(skill.assignedSkills, skill.uesdCount);
        }

        InformAboutChange();
    }

    private void PrepareUI()
    {
        skillUI.InitializeInventoryUI(UISkillBarItems);

        skillUI.OnDescriptionRequested += HandleDescriptionRequest;
        skillUI.OnSwapItems += HandleSwapItems;
        skillUI.OnStartDragging += HandleDragging;
        skillUI.OnItemActionRequested += HandleItemActionRequest;
        //skillUI.OnItemPerformAction += HandleItemPerformAction;
        skillUI.OnPointEnterItem += HandlePointEnterItem;
        skillUI.OnPointExitItem += HandlePointExitItem;

        skillUI.OnDropItems += HandleDropItem;
    }

    private void HandleDescriptionRequest(int itemIndex)
    {
        SkillData skillItem = skillDatas[itemIndex];
        if (skillItem.IsEmpty)
        {
            skillUI.ResetSelection();
            return;
        }
        PlayerSkillSO item = skillItem.assignedSkills;
        string description = skillItem.assignedSkills.Description;
        skillUI.UpdateDescription(itemIndex, item.skillIcon,
            item.name, description);
    }

    private void HandleSwapItems(int itemIndex_1, int itemIndex_2)
    {
        if (itemIndex_1 <= -1) return;
        //InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex_1);
        SkillData skillItem = skillDatas[itemIndex_1];
        if (skillItem.IsEmpty)
            return;
        SwapItems(itemIndex_1, itemIndex_2);
    }

    private void HandleDropItem(int idex)
    {
        if (idex <= -1) return;
        SkillData skillItem = skillDatas[idex];
        if (skillItem.IsEmpty)
            return;

        ResetSkill(idex);
    }

    private void UpdateSkillUI()
    {

        skillUI.ResetAllItems();

        for (int i = 0; i < skillDatas.Length; i++)
        {
            if (skillDatas[i].assignedSkills == null) continue;

            skillUI.UpdateData(i, skillDatas[i].assignedSkills.skillIcon,
            skillDatas[i].uesdCount, null, skillDatas[i].cooldown, skillDatas[i].assignedSkills.cooldown);
        }
    }

    private void HandleDragging(int itemIndex)
    {
        Debug.Log("Skill HandleDragging");
        SkillData skillItem = skillDatas[itemIndex];
        if (skillItem.IsEmpty)
            return;
        skillUI.CreateDraggedItem(skillItem.assignedSkills.skillIcon, skillItem.uesdCount, null, 0, skillItem.assignedSkills.cooldown);
        
    }

    private void HandleItemActionRequest(int itemIndex)
    {
        SkillData skillItem = skillDatas[itemIndex];
        if (skillItem.IsEmpty)
            return;

        //IItemAction itemAction = skillItem.assignedSkills as IItemAction;
        //if (itemAction != null)
        //{

        //    skillUI.ShowItemAction(itemIndex);
        //    skillUI.AddAction(itemAction.ActionName, () => PerformAction(itemIndex));
        //}

        //IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
        //if (destroyableItem != null)
        //{
        //    inventoryUI.AddAction("Drop", () => DropItem(itemIndex, inventoryItem.quantity));
        //}

    }

    private void HandleItemPerformAction(int itemIndex)
    {
        PerformAction(itemIndex);

        SkillData skillItem = skillDatas[itemIndex];
        if (skillItem.IsEmpty) skillUI.CheckCloseItemDetail();
    }

    public void PerformAction(int itemIndex)
    {
        //inventoryitem inventoryitem = inventorydata.getitemat(itemindex);
        //if (inventoryitem.isempty)
        //    return;

        //idestroyableitem destroyableitem = inventoryitem.item as idestroyableitem;
        //if (destroyableitem != null)
        //{
        //    inventorydata.removeitem(itemindex, 1);
        //}

        //iitemaction itemaction = inventoryitem.item as iitemaction;
        //if (itemaction != null)
        //{
        //    itemaction.performaction(gameobject, inventoryitem.itemparameter);
        //    if (itemaction.actionsfx != null) audiosource.playoneshot(itemaction.actionsfx);
        //    if (inventorydata.getitemat(itemindex).isempty)
        //        inventoryui.resetselection();
        //}
    }

    private void HandlePointEnterItem(int itemIndex)
    {
        SkillData skillItem = skillDatas[itemIndex];
        if (skillItem.IsEmpty)
        {
            Debug.Log("inventoryItem.IsEmpty");
            return;
        }
        PlayerSkillSO skill = skillItem.assignedSkills;
        skillUI.OpenItemDetail();
        skillUI.UpdateItemDetail(skill.skillIcon, skill.name, skill.Description);
    }

    private void HandlePointExitItem(int itemIndex)
    {
        SkillData skillItem = skillDatas[itemIndex];
        if (skillItem.IsEmpty)
        {
            return;
        }

        skillUI.CheckCloseItemDetail();
    }
}