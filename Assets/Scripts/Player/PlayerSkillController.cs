using System;
using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    public static PlayerSkillController instance;

    // ใช้ Array ของ Skill ScriptableObject
    public SkillS[] assignedSkills = new SkillS[5];
    public float useSkillDely = 0f;

    private Vector3 _mousePosition;
    [SerializeField] private bool _isSkilling = false;
    [SerializeField] private bool _canSkill = true;
    [SerializeField] private float _canSkillDelyTimer;
    [HideInInspector] public Coroutine _skillStepCoroutine;

    private bool _isDash;


    public Action<bool> OnCanSkillUseStateChange;
    public Action<bool,float> OnSkillingStateChange;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void OnEnable()
    {
        PlayerInputActionsManager.instance.OnSkillSlotInput += HandleSkillSlotInput;
        PlayerInputActionsManager.instance.OnMountPosition += HandleGetMountPos;

        StartCoroutine(WaitForMovementInstance());
    }

    private void OnDisable()
    {
        PlayerInputActionsManager.instance.OnSkillSlotInput -= HandleSkillSlotInput;
        PlayerInputActionsManager.instance.OnMountPosition -= HandleGetMountPos;

        if(PlayerMovement.instance != null) PlayerMovement.instance.OnDashSkillCancelInput -= HandleDashSkillCancelInput;
        if(PlayerMovement.instance != null) PlayerMovement.instance.OnDashStateChange -= HandleDashStateChange;
    }


    private System.Collections.IEnumerator WaitForMovementInstance()
    {
        // รอจนกว่า PlayerMovement.instance จะไม่เป็น null
        while (PlayerMovement.instance == null)
        {
            yield return null;
        }
        // เมื่อ instance พร้อมแล้ว จึงทำการสมัครรับฟัง
        PlayerMovement.instance.OnDashSkillCancelInput += HandleDashSkillCancelInput;
        PlayerMovement.instance.OnDashStateChange += HandleDashStateChange;
    }


    private void HandleDashStateChange(bool isState, Vector3 vector)
    {
        _isDash = isState;

        if (_isDash)
        {
            if(_skillStepCoroutine != null) StopCoroutine(_skillStepCoroutine);
            DoSkillEnd();

            Debug.Log($"_isDash _skillStepCoroutine");
        }
    }

    private void HandleDashSkillCancelInput()
    {
        DoSkillEnd();
    }

    private void HandleGetMountPos(Vector3 mousePosition)
    {
        _mousePosition = mousePosition;
    }

    private void HandleSkillSlotInput(int slot)
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