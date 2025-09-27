using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private bool _isDashing = false;
    [SerializeField] private bool _isSprinte = false;
    [SerializeField] private bool _isSonic = false;
    [SerializeField] private bool _isCombo = false;
    [SerializeField] private bool _isSliding = false;
    private Vector3 _moveDirection;

    [Header("_References")]
    private PlayerInputActionsManager _inputManager;
    private PlayerMovement _playerMovement;
    private PlayerCombatController _playerCombat;

    private void Awake()
    {
        _inputManager = PlayerInputActionsManager.instance;
        _playerMovement = GetComponent<PlayerMovement>();
        _playerCombat = GetComponent<PlayerCombatController>();
    }

    private void OnEnable()
    {
        _inputManager.OnMoveInput += HandleMoveAnimation;

        _playerMovement.OnDashStateChange += HandleDashAnimation;
        _playerMovement.OnSprinteStateChange += HandleSprinteAnimation;
        _playerMovement.OnSonicStateChange += HandleSonicAnimation;
        _playerMovement.OnSlideStateChange += HandleSlideAnimation;

        _playerCombat.OnAttackForward += HandleAttackForwardAnimation;
        _playerCombat.OnComboingStateChange += HandleComboingdAnimation;
    }

    private void OnDisable()
    {
        _inputManager.OnMoveInput -= HandleMoveAnimation;

        _playerMovement.OnDashStateChange -= HandleDashAnimation;
        _playerMovement.OnSprinteStateChange -= HandleSprinteAnimation;
        _playerMovement.OnSonicStateChange -= HandleSonicAnimation;
        _playerMovement.OnSlideStateChange -= HandleSlideAnimation;

        _playerCombat.OnAttackForward -= HandleAttackForwardAnimation;
        _playerCombat.OnComboingStateChange -= HandleComboingdAnimation;
    }

    internal void HandleComboingdAnimation(bool isCombo)
    {
        if(!_isCombo && !_isDashing) animator.SetTrigger("atStartCombo");

        _isCombo = isCombo;
        
        animator.SetBool("isCombo", _isCombo);

        if(!isCombo) HandleMoveAnimation(_moveDirection);
    }

    internal void HandleAttackForwardAnimation(Vector3 vector, float arg2, float arg3)
    {
        animator.SetTrigger("atAttack");

        SetActionStateDirection(true, vector);
    }

    internal void HandleSprinteAnimation(bool isSprinteState)
    {
        _isSprinte = isSprinteState;
        animator.SetBool("isSprinte", _isSprinte);
    }

    internal void HandleSonicAnimation(bool isSonicState)
    {
        _isSonic = isSonicState;
        animator.SetBool("isSonic", _isSonic);
    }

    internal void HandleSlideAnimation(bool isState, Vector3 actionDirection)
    {
        _isSliding = isState;
        animator.SetBool("isSlide", isState);

        SetActionStateDirection(isState, actionDirection);
    }

    internal void HandleMoveAnimation(Vector3 moveDirection)
    {
        _moveDirection = moveDirection;

        // ถ้ากำลังกลิ้งอยู่ ให้ไม่ต้องทำอะไร
        if (_isDashing || _isCombo || _isSliding) return;

        // ตรวจสอบว่ามีการเคลื่อนที่หรือไม่
        bool isMoving = moveDirection.magnitude > 0.01f;
        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            animator.SetFloat("MoveX", moveDirection.x);
            animator.SetFloat("MoveZ", moveDirection.z);
        }
        else
        {
            animator.SetFloat("MoveX", 0);
            animator.SetFloat("MoveZ", 0);
        }

        // Flip Sprite ตามทิศทางการเดิน เมื่อไม่ได้กำลังกลิ้ง
        FlipSprite(moveDirection);
    }

    // ฟังก์ชันสำหรับเล่น Animation กลิ้ง
    internal void HandleDashAnimation(bool isDashingState, Vector3 actionDirection)
    {
        _isDashing = isDashingState; // อัปเดตสถานะ
        if(isDashingState) animator.SetTrigger("atDash");

        SetActionStateDirection(isDashingState, actionDirection);

    }

    private void FlipSprite(Vector3 Direction)
    {
        // จัดการการพลิก Sprite
        if (Direction.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (Direction.x < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    private void SetActionStateDirection(bool isState, Vector3 actionDirection)
    {
        if (isState)
        {
            
            animator.SetFloat("ActionX", actionDirection.x);
            animator.SetFloat("ActionZ", actionDirection.z);

            FlipSprite(actionDirection);
        }
        else
        {
            HandleMoveAnimation(_moveDirection);
        }
    }
}