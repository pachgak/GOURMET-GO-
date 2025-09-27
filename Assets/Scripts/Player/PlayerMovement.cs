using System;
using UnityEngine;
using System.Collections;
using Inventory;

public class PlayerMovement : MonoBehaviour
{
    [Header("Walk")]
    public float moveSpeed = 5f;
    [Header("Sprint")]
    public float sprintSpeed = 10f;
    public float sonicSpeed = 10f;
    public float speedUpTime = 2f; // �ѵ�ҡ��������������
    public float timeToSonicSpeed = 5f; // ���ҷ����㹡����觡�͹���������� sonic
    //public AnimationCurve sprintCurve;
    [Header("Dash")]
    public float dashCooldown = 1.5f; // ���� Cooldown �ͧ Dash
    public Vector2 dashSpeedFactor = new Vector2(4f, 2f);
    public float dashTime = 0.2f;
    [Header("Attack Delay Movement")]
    public float attackMovementDelay = 0.2f; // ����˹�ǧ��ѧ����
    private Coroutine movementDelayCoroutine;
    [Header("Sliding")]
    public Vector2 slideBrakeFactor = new Vector2(0.9f, 0.98f); // ����������Է������ä (���¡��� 1)
    public float minSlideSpeed = 2f; // �������Ǣ�鹵�ӷ����������ش��
    [Header("Jun System")]
    public float lastDirectionDelay = 0.05f; // ����˹�ǧ���س��ͧ���
    public float gravity = -20f; // CharacterController ���ç�����ǧ�ͧ����ͧ
    public float resetClick = 0.3f;

    [Header("_Scripts References")]
    private PlayerCombatController _playerCombat;
    private PlayerSkillController _playerSkill;

    [Header("_Manager References")]
    private PlayerInputActionsManager _inputManager;
    private OpenUiManager _uiManager;

    [Header("_System")]
    private CharacterController controller;

    //movement
    private float _currentSpeed; // �������ǻѨ�غѹ�ͧ����Ф�
    private bool _canMove = true; // ʶҹ�����Ѻ�Ǻ����������͹���
    [SerializeField] private bool _holdSprinte = false; // ʶҹ�����Ѻ�Ǻ����������͹���
    [SerializeField] private bool _canSprinte = true; // ʶҹ�����Ѻ�Ǻ����������͹���
    [SerializeField] private bool _isSprinting = false;
    [SerializeField] private bool _isSonic = false;
    private float _sprintTimer;
    private Coroutine _sonicTimerCoroutine;
    private Coroutine _canSprinteCoroutine;


    //Direction
    private Vector3 _moveDirection;
    private Vector3 _lastMoveDirection;
    private Vector3 _delayedMoveDirection;
    private Vector3 _beforeMoveDirection;
    private Coroutine _directionUpdateCoroutine;

    //Slide
    [SerializeField] private bool _isSliding = false; // �����ʶҹ���������Ѻ�Ǻ��������
    private float _trueSlideBrakeFactor; // ��������ù�������
    private Vector3 _slideVelocity;

    //Dash
    [SerializeField] private bool _isDashing = false;
    [SerializeField] private bool _dashClick = false;
    private bool _canDash = true; // ʶҹ�����Ѻ��Ǩ�ͺ��� Dash �������ҹ�������
    private float _dashTimeCounter;
    private float _resetDashClickTimer;
    private Vector3 _dashVelocity; // �纤�����������Ѻ��þ��

    //attack
    [SerializeField] private bool _isAttackingForward = false;
    private float _attackForwardTimeCounter;
    private bool _canAttack = true;
    private Vector3 _attackForwardDirection;

    //Inventory
    private bool _isUiOpening;

    //Skill Dash
    private bool _isDashSkilling = false;
    private float _dashSkillTimeCounter;
    private Vector3 _dashSkillVelocity;
    private Coroutine _skillDelayCoroutine;

    private bool _isSkilling = false;


    public Action<bool,Vector3> OnDashStateChange;
    public Action<bool> OnSprinteStateChange;
    public Action<bool> OnSonicStateChange;
    public Action<bool, Vector3> OnSlideStateChange;
    public Action<Vector3> OnLastMoveDirectionChange;
    public Action<Vector3, float, float, Coroutine> OnSkillDash;
    public Action<bool> OnDashSkillStateChange;
    public Action OnDashSkillCancelInput;

    private void Awake()
    {
        //Ref
        _inputManager = PlayerInputActionsManager.instance;
        _uiManager = OpenUiManager.instance;
        _playerCombat = GetComponent<PlayerCombatController>();
        _playerSkill = GetComponent<PlayerSkillController>();


        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("CharacterController component not found on the GameObject.");
        }

        OnSkillDash += HandleSkillDash;
    }

    private void OnEnable()
    {
        _inputManager.OnMoveInput += HandleMoveInput;
        _inputManager.OnSprintInput += HandleSprintInput;
        _inputManager.OnDashInput += HandleDashInput;

        _uiManager.OnUiOpeningStateChange += HandleUiOpeningStateChange;

        _playerCombat.OnAttackStateChange += HandleAttackStateChange;
        _playerCombat.OnAttackForward += HandleAttackForward;

        _playerSkill.OnSkillingStateChange += HandleSkillingStateChange;

    }

    private void OnDisable()
    {
        _inputManager.OnMoveInput -= HandleMoveInput;
        _inputManager.OnSprintInput -= HandleSprintInput;
        _inputManager.OnDashInput -= HandleDashInput;

        _uiManager.OnUiOpeningStateChange -= HandleUiOpeningStateChange;

        _playerCombat.OnAttackStateChange -= HandleAttackStateChange;
        _playerCombat.OnAttackForward -= HandleAttackForward;

        _playerSkill.OnSkillingStateChange -= HandleSkillingStateChange;
        
    }

    internal void HandleUiOpeningStateChange(bool isUiOpeningState)
    {
        _isUiOpening = isUiOpeningState;
    }

    internal void HandleMoveInput(Vector3 direction)
    {
        _moveDirection = direction;

        if (_holdSprinte && _moveDirection != Vector3.zero && _canSprinte  && !_isSprinting && !_isSliding && !_isDashing && !_isAttackingForward)
        {
            SetIsSprint(true);
        }

        // ��Ҽ����蹻���»����Թ (��ȷҧ�������͹������ٹ��)
        if (_moveDirection == Vector3.zero && _isSprinting)
        {
            // �ѧ�Ѻ���ʶҹС������� false �ѹ��
            SetIsSprint(false);
        }

        SaveLastMoveDirection();
    }

    internal void HandleSprintInput(bool isSprintingState)
    {
        _holdSprinte = isSprintingState;

        if (_isUiOpening) return;

        if (!_canSprinte) return;

        if (isSprintingState && _moveDirection != Vector3.zero && !_isSliding)
        {
            SetIsSprint(true);
        }
        // ��Ҽ����蹻���»������
        else
        {
            SetIsSprint(false);
        }
    }

    internal void HandleDashInput()
    {
        if (_isUiOpening) return;
        if (_isSkilling) return;

        _dashClick = true;
        _resetDashClickTimer = resetClick;

        if (_moveDirection != Vector3.zero && _canDash && _dashClick)
        {
            SetIsDash(true);

            SetSprinteCooldown(dashCooldown);

            if(_isSprinting) SetIsSprint(false,false);
            if(_isDashSkilling) SetIsDashSkill(false);

            _dashClick = false;
        }
    }

    internal void HandleAttackForward(Vector3 direction, float speed, float time)
    {
        _isAttackingForward = true;
        _attackForwardTimeCounter = time;
        _attackForwardDirection = direction * speed;

        SetSprinteCooldown(_attackForwardTimeCounter);

    }

    // �ѧ��ѹ����Ѻ�ѺʶҹС������
    internal void HandleAttackStateChange(bool canAttack)
    {
        _canAttack = canAttack;

        if (!canAttack)
        {
            SetMoveDeley(attackMovementDelay);

            if (_isSprinting) SetIsSprint(false, false);
        }
    }

    private void SetMoveDeley(float delay)
    {
        // ��ش Coroutine ��� (�����) ��͹�������������
        if (movementDelayCoroutine != null)
        {
            StopCoroutine(movementDelayCoroutine);
        }
        // ����� Coroutine ����˹�ǧ���ҡ������͹���
        movementDelayCoroutine = StartCoroutine(StartMovementDelay(delay));
    }

    internal void HandleSkillDash(Vector3 direction, float dashSkillSpeed, float dashSkillTime, Coroutine skillDelayCoroutine)
    {
        Debug.Log("HandleSkillDash");
        //Dash Script

        SetIsDashSkilling(true, direction, dashSkillSpeed, dashSkillTime, skillDelayCoroutine);


        SetSprinteCooldown(dashCooldown);
        if (_isSprinting) SetIsSprint(false, false);

        //if (!isDashing && !isSliding && !isAttackingForward && _canMove)
        //{
        //    isDashSkill = true;
        //    dashSkillTimeCounter = dashSkillTime;
        //    dashSkillDirection = direction * dashSkillSpeed;
        //}
    }

    internal void HandleSkillingStateChange(bool isState , float skillLifeTime)
    {
        _isSkilling = isState;

        if(isState) SetMoveDeley(skillLifeTime);
    }
    private void Update()
    {
        if (_dashClick && _canDash) HandleDashInput();
        if (_dashClick)
        {
            _resetDashClickTimer -= Time.deltaTime;
            if (_resetDashClickTimer <= 0)
            {
                _dashClick = false;
            }
        }

        Vector3 finalMovement = Vector3.zero;

        if (_isDashSkilling)
        {

            finalMovement = _dashSkillVelocity;
            _dashSkillTimeCounter -= Time.deltaTime;

            if (_dashSkillTimeCounter <= 0)
            {
                SetIsDashSkill(false);
            }
        }
        else if (_isDashing)
        {

            finalMovement = _dashVelocity;
            _dashTimeCounter -= Time.deltaTime;

            if (_dashTimeCounter <= 0)
            {
                SetIsDash(false);
            }
        }
        else if (_isAttackingForward)
        {
            finalMovement = _attackForwardDirection;
            _attackForwardTimeCounter -= Time.deltaTime;
            if (_attackForwardTimeCounter <= 0)
            {
                _isAttackingForward = false;
                finalMovement = Vector3.zero;
            }
        }
        else if (_isSliding)
        {
            // Ŵ��������ŧ������� ���ͨ��ͧ�����
            _currentSpeed *= _trueSlideBrakeFactor; // �����÷��ӹǳ����
            finalMovement = _slideVelocity * _currentSpeed; // �� lastMoveDirection

            // ��ش���������ͤ�������Ŵŧ�֧��ҷ���˹�
            if (_currentSpeed <= minSlideSpeed)
            {
                SetIsSliding(false);
            }
        }
        else if (_canMove && !_isUiOpening)
        {

            // ��ҡ��ѧ��觤�����������
            if (_isSprinting && _sprintTimer < speedUpTime)
            {
                _sprintTimer += Time.deltaTime;

                // ����� sprintTimer ����㹪�ǧ 0 �֧ sprintTime
                _sprintTimer = Mathf.Clamp(_sprintTimer, 0, speedUpTime);
            }
            // ��ҡ��ѧ���ͤ�������
            else if (!_isSprinting && !_isSonic && _sprintTimer != 0)
            {
                _sprintTimer -= Time.deltaTime * 2.5f;

                // ����� sprintTimer ����㹪�ǧ 0 �֧ sprintTime
                _sprintTimer = Mathf.Clamp(_sprintTimer, 0, speedUpTime);
            }

            // �� sprintTimer ���ͤӹǳ Lerp
            float t = _sprintTimer / speedUpTime;

            if (_isSprinting && _canSprinte)
            {
                if (!_isSonic)
                {
                    if (_currentSpeed != sprintSpeed) _currentSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, t);
                }
                else
                {
                    if (_currentSpeed != sonicSpeed) _currentSpeed = Mathf.Lerp(sprintSpeed, sonicSpeed, t);
                }
            }
            else
            {
                if (_currentSpeed != moveSpeed)
                {
                    _currentSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, t);
                }
                //currentSpeed = moveSpeed;
            }

            finalMovement = _moveDirection * _currentSpeed;
        }

        // �� isGrounded ���͵�Ǩ�ͺ������躹����������
        if (controller.isGrounded)
        {
            // ������躹��� ����������᡹ Y �� 0 �������������Фè�
            finalMovement.y = 0;
        }
        else
        {
            // ���������� ������ç�����ǧ
            finalMovement.y = gravity;
        }

        // �� controller.Move ����������Ф�����͹���
        controller.Move(finalMovement * Time.deltaTime);


    }

    // Coroutine ����Ѻ���˹�ǧ����
    private IEnumerator DirectionUpdateCoroutine()
    {
        // ��ش�������ҷ���˹�
        yield return new WaitForSeconds(lastDirectionDelay);

        // ��ѧ�ҡ������ ����Ǩ�ͺ��ҷ�ȷҧ�ѧ������͹����������
        if (_delayedMoveDirection == _moveDirection)
        {
            _beforeMoveDirection = _lastMoveDirection;
            _lastMoveDirection = _delayedMoveDirection;

            OnLastMoveDirectionChange?.Invoke(_lastMoveDirection);
        }
    }

    private IEnumerator SprintTimerCoroutine()
    {
        yield return new WaitForSeconds(timeToSonicSpeed);

        _isSonic = true;
        OnSonicStateChange?.Invoke(_isSonic);
        // ���� _sprintTimer ��������� Lerp ����
        _sprintTimer = 0f;
    }

    private void SaveLastMoveDirection()
    {
        // ��Ҽ�����������������Թ (direction �����ҡѺ zero)
        if (_moveDirection != Vector3.zero)
        {
            // �纤�ҷ�ȷҧ�Ѩ�غѹ���
            _delayedMoveDirection = _moveDirection;

            // ��ش Coroutine ��� (�����) ��͹���������
            if (_directionUpdateCoroutine != null)
            {
                StopCoroutine(_directionUpdateCoroutine);
            }

            // ����� Coroutine ���ͨѺ����
            _directionUpdateCoroutine = StartCoroutine(DirectionUpdateCoroutine());

        }
        // ��Ҽ����蹻���»���
        else
        {
            // ��ش Coroutine �ѹ��
            if (_directionUpdateCoroutine != null)
            {
                StopCoroutine(_directionUpdateCoroutine);
            }
        }
    }

    private IEnumerator StartMovementDelay(float movementDelay)
    {
        _canMove = false; // ��ش�������͹���
        yield return new WaitForSeconds(movementDelay);
        _canMove = true; // ��Ѻ������͹�����
    }

    private IEnumerator DashCooldownCoroutine(float cooldown)
    {
        _canDash = false;
        yield return new WaitForSeconds(cooldown);
        _canDash = true;
    }

    private void SetSprinteCooldown(float cooldown)
    {
        SetIsSprint(false, false);

        if (_canSprinteCoroutine != null) StopCoroutine(_canSprinteCoroutine);

        _canSprinteCoroutine = StartCoroutine(SprintCooldownCoroutine(cooldown));
        
    }

    private IEnumerator SprintCooldownCoroutine(float cooldown)
    {
        _canSprinte = false; // ��ش�������͹���
        yield return new WaitForSeconds(cooldown);
        _canSprinte = true; // ��Ѻ������͹�����

        if (_holdSprinte) SetIsSprint(true);
    }

    private void SetIsSliding(bool isSet)
    {
        if (isSet == true)
        {
            // �ӹǳ��������繵� (t) �ͧ currentSpeed
            float t = Mathf.InverseLerp(moveSpeed, sonicSpeed, _currentSpeed);
            // ���� t �����Ҥ���������Է������ä����������
            _trueSlideBrakeFactor = Mathf.Lerp(slideBrakeFactor.x, slideBrakeFactor.y, t);

            _isSliding = true;
            _slideVelocity = _lastMoveDirection;
            OnSlideStateChange?.Invoke(_isSliding, _slideVelocity);

            float timeToStopSliding = Mathf.Log(minSlideSpeed / _currentSpeed) / Mathf.Log(_trueSlideBrakeFactor) * Time.deltaTime;
            Debug.Log("�������һ���ҳ " + timeToStopSliding + " �Թҷ� 㹡����");
            SetSprinteCooldown(timeToStopSliding);
        }

        if (isSet == false)
        {
            _isSliding = false;
            OnSlideStateChange?.Invoke(_isSliding, _slideVelocity);

            _sprintTimer = 0;
        }
    }

    private void SetIsSprint(bool isSet)
    {
        SetIsSprint(isSet, true);
    }

    private void SetIsSprint(bool isSet, bool doSliding)
    {
        if (isSet == true)
        {
            _isSprinting = true;
            OnSprinteStateChange?.Invoke(_isSprinting);

            // ����� Coroutine ����Ѻ��ùѺ���� Sonic Speed
            if (timeToSonicSpeed >= 0 && !_isSonic) _sonicTimerCoroutine = StartCoroutine(SprintTimerCoroutine());
        }

        if (isSet == false)
        {
            _isSprinting = false;
            OnSprinteStateChange?.Invoke(_isSprinting);

            // ��ش Coroutine �ѹ���������ش���
            if (_sonicTimerCoroutine != null)
            {
                StopCoroutine(_sonicTimerCoroutine);
                _sonicTimerCoroutine = null;
            }

            if (doSliding)
            {
                if (_isSonic)
                {
                    _isSonic = false;
                    OnSonicStateChange?.Invoke(_isSonic);
                    SetIsSliding(true);
                    if (_sonicTimerCoroutine != null) StopCoroutine(_sonicTimerCoroutine);
                }
            }
            else
            {
                _sprintTimer = 0;
                _currentSpeed = moveSpeed;
                _isSonic = false;
                OnSonicStateChange?.Invoke(_isSonic);
                if (_sonicTimerCoroutine != null) StopCoroutine(_sonicTimerCoroutine);
            }

            //// �������ش��� �������Ҥ����������������
            //if (currentSpeed > moveSpeed)
            //{
            //    SetIsSliding(true);
            //}
        }
    }


    private void SetIsDash(bool isSet)
    {
        if (isSet == true)
        {
            _isDashing = true;
            _dashTimeCounter = dashTime;

            // InverseLerp ������� currentSpeed ��������˹��˹�����ҧ moveSpeed �Ѻ sprintSpeed
            float t = Mathf.InverseLerp(moveSpeed, sonicSpeed, _currentSpeed);
            // Lerp �Фӹǳ��������ҧ dashSpeedFactor.x �Ѻ dashSpeedFactor.y ������ t
            float trueDashSpeedFactor = Mathf.Lerp(dashSpeedFactor.x, dashSpeedFactor.y, t);
            // �纤�����������Ѻ��͹���
            _dashVelocity = _moveDirection.normalized * _currentSpeed * trueDashSpeedFactor;
            OnDashStateChange?.Invoke(_isDashing,_dashVelocity);

            // ����� Coroutine ����Ѻ Cooldown
            StartCoroutine(DashCooldownCoroutine(dashCooldown));
        }

        if (isSet == false)
        {
            _isDashing = false;
            _dashVelocity = Vector3.zero;
            OnDashStateChange?.Invoke(_isDashing, _dashVelocity);
        }
    }

    private void SetIsDashSkill(bool isSet)
    {
        if (isSet) return;

        _isDashSkilling = false;
        _dashSkillVelocity = Vector3.zero;
        OnDashSkillStateChange?.Invoke(isSet);

        //if (isStopIt && _skillDelayCoroutine != null)
        //{
        //    StopCoroutine(_skillDelayCoroutine);
        //    OnDashSkillCancelInput?.Invoke();
        //}
    }

    private void SetIsDashSkilling(bool isSet, Vector3 direction, float dashSkillSpeed, float dashSkillTime, Coroutine skillDelayCoroutine)
    {
        if (isSet == true)
        {
            _isDashSkilling = true;
            _dashSkillTimeCounter = dashSkillTime;
            _dashSkillVelocity = direction.normalized * dashSkillSpeed;
            _skillDelayCoroutine = skillDelayCoroutine;

            OnDashSkillStateChange?.Invoke(isSet);
        }

        if (isSet == false)
        {
            SetIsDashSkill(false);
        }
    }


}