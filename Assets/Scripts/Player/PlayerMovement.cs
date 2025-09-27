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
    public float speedUpTime = 2f; // อัตราการเพิ่มความเร็ว
    public float timeToSonicSpeed = 5f; // เวลาที่ใช้ในการวิ่งก่อนเข้าสู่โหมด sonic
    //public AnimationCurve sprintCurve;
    [Header("Dash")]
    public float dashCooldown = 1.5f; // เวลา Cooldown ของ Dash
    public Vector2 dashSpeedFactor = new Vector2(4f, 2f);
    public float dashTime = 0.2f;
    [Header("Attack Delay Movement")]
    public float attackMovementDelay = 0.2f; // เวลาหน่วงหลังโจมตี
    private Coroutine movementDelayCoroutine;
    [Header("Sliding")]
    public Vector2 slideBrakeFactor = new Vector2(0.9f, 0.98f); // ค่าสัมประสิทธิ์การเบรค (น้อยกว่า 1)
    public float minSlideSpeed = 2f; // ความเร็วขั้นต่ำที่จะเริ่มหยุดไถล
    [Header("Jun System")]
    public float lastDirectionDelay = 0.05f; // เวลาหน่วงที่คุณต้องการ
    public float gravity = -20f; // CharacterController ใช้แรงโน้มถ่วงของตัวเอง
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
    private float _currentSpeed; // ความเร็วปัจจุบันของตัวละคร
    private bool _canMove = true; // สถานะสำหรับควบคุมการเคลื่อนที่
    [SerializeField] private bool _holdSprinte = false; // สถานะสำหรับควบคุมการเคลื่อนที่
    [SerializeField] private bool _canSprinte = true; // สถานะสำหรับควบคุมการเคลื่อนที่
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
    [SerializeField] private bool _isSliding = false; // ตัวแปรสถานะใหม่สำหรับควบคุมการไถล
    private float _trueSlideBrakeFactor; // เพิ่มตัวแปรนี้เข้ามา
    private Vector3 _slideVelocity;

    //Dash
    [SerializeField] private bool _isDashing = false;
    [SerializeField] private bool _dashClick = false;
    private bool _canDash = true; // สถานะสำหรับตรวจสอบว่า Dash พร้อมใช้งานหรือไม่
    private float _dashTimeCounter;
    private float _resetDashClickTimer;
    private Vector3 _dashVelocity; // เก็บความเร็วสำหรับการพุ่ง

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

        // ถ้าผู้เล่นปล่อยปุ่มเดิน (ทิศทางการเคลื่อนที่เป็นศูนย์)
        if (_moveDirection == Vector3.zero && _isSprinting)
        {
            // บังคับให้สถานะการวิ่งเป็น false ทันที
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
        // ถ้าผู้เล่นปล่อยปุ่มวิ่ง
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

    // ฟังก์ชันสำหรับรับสถานะการโจมตี
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
        // หยุด Coroutine เก่า (ถ้ามี) ก่อนที่จะเริ่มใหม่
        if (movementDelayCoroutine != null)
        {
            StopCoroutine(movementDelayCoroutine);
        }
        // เริ่ม Coroutine เพื่อหน่วงเวลาการเคลื่อนที่
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
            // ลดความเร็วลงเรื่อยๆ เพื่อจำลองการไถล
            _currentSpeed *= _trueSlideBrakeFactor; // ใช้ตัวแปรที่คำนวณใหม่
            finalMovement = _slideVelocity * _currentSpeed; // ใช้ lastMoveDirection

            // หยุดการไถลเมื่อความเร็วลดลงถึงค่าที่กำหนด
            if (_currentSpeed <= minSlideSpeed)
            {
                SetIsSliding(false);
            }
        }
        else if (_canMove && !_isUiOpening)
        {

            // ถ้ากำลังเร่งความเร็วอยู่
            if (_isSprinting && _sprintTimer < speedUpTime)
            {
                _sprintTimer += Time.deltaTime;

                // ทำให้ sprintTimer อยู่ในช่วง 0 ถึง sprintTime
                _sprintTimer = Mathf.Clamp(_sprintTimer, 0, speedUpTime);
            }
            // ถ้ากำลังชะลอความเร็ว
            else if (!_isSprinting && !_isSonic && _sprintTimer != 0)
            {
                _sprintTimer -= Time.deltaTime * 2.5f;

                // ทำให้ sprintTimer อยู่ในช่วง 0 ถึง sprintTime
                _sprintTimer = Mathf.Clamp(_sprintTimer, 0, speedUpTime);
            }

            // ใช้ sprintTimer เพื่อคำนวณ Lerp
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

        // ใช้ isGrounded เพื่อตรวจสอบว่าอยู่บนพื้นหรือไม่
        if (controller.isGrounded)
        {
            // ถ้าอยู่บนพื้น ให้ความเร็วแกน Y เป็น 0 เพื่อไม่ให้ตัวละครจม
            finalMovement.y = 0;
        }
        else
        {
            // ถ้าลอยอยู่ ให้ใช้แรงโน้มถ่วง
            finalMovement.y = gravity;
        }

        // ใช้ controller.Move เพื่อให้ตัวละครเคลื่อนที่
        controller.Move(finalMovement * Time.deltaTime);


    }

    // Coroutine สำหรับการหน่วงเวลา
    private IEnumerator DirectionUpdateCoroutine()
    {
        // หยุดรอเป็นเวลาที่กำหนด
        yield return new WaitForSeconds(lastDirectionDelay);

        // หลังจากรอแล้ว ให้ตรวจสอบว่าทิศทางยังคงเหมือนเดิมหรือไม่
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
        // รีเซ็ต _sprintTimer เพื่อเริ่ม Lerp ใหม่
        _sprintTimer = 0f;
    }

    private void SaveLastMoveDirection()
    {
        // ถ้าผู้เล่นเริ่มกดปุ่มเดิน (direction ไม่เท่ากับ zero)
        if (_moveDirection != Vector3.zero)
        {
            // เก็บค่าทิศทางปัจจุบันไว้
            _delayedMoveDirection = _moveDirection;

            // หยุด Coroutine เก่า (ถ้ามี) ก่อนเริ่มใหม่
            if (_directionUpdateCoroutine != null)
            {
                StopCoroutine(_directionUpdateCoroutine);
            }

            // เริ่ม Coroutine เพื่อจับเวลา
            _directionUpdateCoroutine = StartCoroutine(DirectionUpdateCoroutine());

        }
        // ถ้าผู้เล่นปล่อยปุ่ม
        else
        {
            // หยุด Coroutine ทันที
            if (_directionUpdateCoroutine != null)
            {
                StopCoroutine(_directionUpdateCoroutine);
            }
        }
    }

    private IEnumerator StartMovementDelay(float movementDelay)
    {
        _canMove = false; // หยุดการเคลื่อนที่
        yield return new WaitForSeconds(movementDelay);
        _canMove = true; // กลับมาเคลื่อนที่ได้
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
        _canSprinte = false; // หยุดการเคลื่อนที่
        yield return new WaitForSeconds(cooldown);
        _canSprinte = true; // กลับมาเคลื่อนที่ได้

        if (_holdSprinte) SetIsSprint(true);
    }

    private void SetIsSliding(bool isSet)
    {
        if (isSet == true)
        {
            // คำนวณค่าเปอร์เซ็นต์ (t) ของ currentSpeed
            float t = Mathf.InverseLerp(moveSpeed, sonicSpeed, _currentSpeed);
            // ใช้ค่า t เพื่อหาค่าสัมประสิทธิ์การเบรคที่เหมาะสม
            _trueSlideBrakeFactor = Mathf.Lerp(slideBrakeFactor.x, slideBrakeFactor.y, t);

            _isSliding = true;
            _slideVelocity = _lastMoveDirection;
            OnSlideStateChange?.Invoke(_isSliding, _slideVelocity);

            float timeToStopSliding = Mathf.Log(minSlideSpeed / _currentSpeed) / Mathf.Log(_trueSlideBrakeFactor) * Time.deltaTime;
            Debug.Log("จะใช้เวลาประมาณ " + timeToStopSliding + " วินาที ในการไถล");
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

            // เริ่ม Coroutine สำหรับการนับเวลา Sonic Speed
            if (timeToSonicSpeed >= 0 && !_isSonic) _sonicTimerCoroutine = StartCoroutine(SprintTimerCoroutine());
        }

        if (isSet == false)
        {
            _isSprinting = false;
            OnSprinteStateChange?.Invoke(_isSprinting);

            // หยุด Coroutine ทันทีเมื่อหยุดวิ่ง
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

            //// เมื่อหยุดวิ่ง ให้เช็คว่าควรเริ่มไถลหรือไม่
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

            // InverseLerp จะหาว่า currentSpeed อยู่ที่ตำแหน่งไหนระหว่าง moveSpeed กับ sprintSpeed
            float t = Mathf.InverseLerp(moveSpeed, sonicSpeed, _currentSpeed);
            // Lerp จะคำนวณค่าระหว่าง dashSpeedFactor.x กับ dashSpeedFactor.y ตามค่า t
            float trueDashSpeedFactor = Mathf.Lerp(dashSpeedFactor.x, dashSpeedFactor.y, t);
            // เก็บความเร็วสำหรับใช้ตอนพุ่ง
            _dashVelocity = _moveDirection.normalized * _currentSpeed * trueDashSpeedFactor;
            OnDashStateChange?.Invoke(_isDashing,_dashVelocity);

            // เริ่ม Coroutine สำหรับ Cooldown
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