using System;
using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    // ใช้ Array ของ Skill ScriptableObject
    public PlayerSkillSO[] assignedSkills = new PlayerSkillSO[5];
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
        _inputManager.OnSkillSlotInput += HandleSkillSlotInput;
        _inputManager.OnMountPosition += HandleGetMountPos;

        _playerMovement.OnDashSkillCancelInput += HandleDashSkillCancelInput;
        _playerMovement.OnDashStateChange += HandleDashStateChange;
    }

    private void OnDisable()
    {
        _inputManager.OnSkillSlotInput -= HandleSkillSlotInput;
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
        if (slot > assignedSkills.Length) return;
        if (_isSkilling) return;
        if (!_canSkill) return;
        if (assignedSkills[slot - 1] == null) return;

        SetSkillingState(true, assignedSkills[slot - 1].skillLifeTime);

        _skillStepCoroutine = assignedSkills[slot - 1].Use(gameObject, _mousePosition);
        Invoke(nameof(DoSkillEnd), assignedSkills[slot - 1].skillLifeTime);

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
}