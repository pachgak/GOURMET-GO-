using System;
using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    [Serializable]
    public class skillData
    {
        public PlayerSkillSO assignedSkills;
        public int uesdCount;
        [SerializeField] private float _cooldown;
    }

    // ใช้ Array ของ Skill ScriptableObject
    public skillData[] skillDatas = new skillData[5];
    public float useSkillDely = 0f;

    private Vector3 _mousePosition;
    [SerializeField] private bool _isSkilling = false;
    [SerializeField] private bool _canSkill = true;
    [SerializeField] private float _canSkillDelyTimer;
    [HideInInspector] public Coroutine _skillStepCoroutine;

    [Header("_Scripts References")]
    private PlayerMovement _playerMovement;
    [Header("_Manager References")]
    private PlayerInputActionsManager _inputManager;

    private bool _isDash;


    public Action<bool> OnCanSkillUseStateChange;
    public Action<bool,float> OnSkillingStateChange;

    private void Awake()
    {
        _inputManager = PlayerInputActionsManager.instance;
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        _inputManager.OnInputNumber += HandleSkillSlotInput;
        _inputManager.OnMountPosition += HandleGetMountPos;

        _playerMovement.OnDashSkillCancelInput += HandleDashSkillCancelInput;
        _playerMovement.OnDashStateChange += HandleDashStateChange;
    }

    private void OnDisable()
    {
        _inputManager.OnInputNumber -= HandleSkillSlotInput;
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

            Debug.Log($"_isDash _skillStepCoroutine");
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

        SetSkillingState(true, skillDatas[slot - 1].assignedSkills.skillLifeTime);

        _skillStepCoroutine = skillDatas[slot - 1].assignedSkills.Use(gameObject, _mousePosition);
        Invoke(nameof(DoSkillEnd), skillDatas[slot - 1].assignedSkills.skillLifeTime);
        skillDatas[slot - 1].uesdCount--;

        if (skillDatas[slot - 1].uesdCount <= 0)
        {
            RemoveSkill(slot - 1);
        }

        _canSkill = false;
        _canSkillDelyTimer = useSkillDely;
        OnCanSkillUseStateChange?.Invoke(_canSkill);


        
        //float skillLifeTime = assignedSkills[slot - 1].skillLifeTime;
        //_skillingEndTimer = skillLifeTime;
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

                return true;
            }
        }
        return false;
    }

    public void RemoveSkill(int Slot)
    {
        skillDatas[Slot] = null;
    }
}