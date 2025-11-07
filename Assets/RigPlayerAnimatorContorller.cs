using Inventory.Model;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RigPlayerAnimatorContorller : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteFDown;
    [SerializeField] private SpriteRenderer spriteBUp;
    [SerializeField] private SpriteRenderer spriteFLeft;
    [SerializeField] private SpriteRenderer spriteBLeft;
    //[SerializeField] private Animator animator;
    private SpriteRenderer curreanSprite;

    [SerializeField] private bool _isDashing = false;
    private bool _isSprinte = false;
    private bool _isSonic = false;
    private bool _isCombo = false;
    private bool _isSliding = false;

    private Vector3 _inputDirection;

    private Animator _animatorFDown;
    private Animator _animatorBUp;
    private Animator _animatorFLeft;
    private Animator _animatorBLeft;

    private List<Animator> _allAnimator = new List<Animator>();

    [Header("_References")]
    private PlayerInputActionsManager _inputManager;
    private PlayerMovement _playerMovement;
    private PlayerCombatController _playerCombat;
    private SettingPlayerControllerManager _settingController;

    private void Awake()
    {
        _inputManager = PlayerInputActionsManager.instance;
        _settingController = SettingPlayerControllerManager.instance;
        _playerMovement = GetComponent<PlayerMovement>();
        _playerCombat = GetComponent<PlayerCombatController>();

        //SetToPlay
        spriteFDown.gameObject.SetActive(true);
        spriteFDown.transform.localPosition = Vector3.zero;
        spriteBUp.  gameObject.SetActive(true);
        spriteBUp.transform.localPosition = Vector3.zero;
        spriteFLeft.gameObject.SetActive(true);
        spriteFLeft.transform.localPosition = Vector3.zero;
        spriteBLeft.gameObject.SetActive(true);
        spriteBLeft.transform.localPosition = Vector3.zero;

        _animatorFDown = spriteFDown.GetComponent<Animator>() ;
        _animatorBUp =   spriteBUp.GetComponent<Animator>();
        _animatorFLeft = spriteFLeft.GetComponent<Animator>();
        _animatorBLeft = spriteBLeft.GetComponent<Animator>();

        _allAnimator.Add(_animatorFDown);
        _allAnimator.Add(_animatorBUp);
        _allAnimator.Add(_animatorFLeft);
        _allAnimator.Add(_animatorBLeft);
}

    private void OnEnable()
    {
        _inputManager.OnMoveInput += HandleMoveAnimation;

        _playerMovement.OnDashStateChange += HandleDashAnimation;
        _playerMovement.OnSprinteStateChange += HandleSprinteAnimation;
        _playerMovement.OnLastMoveDirectionChange += HandleLastMoveDirectionChange;

        _playerCombat.OnAttackForward += HandleAttackForwardAnimation;
        _playerCombat.OnComboingStateChange += HandleComboingdAnimation;
    }

    private void OnDisable()
    {
        _inputManager.OnMoveInput -= HandleMoveAnimation;

        _playerMovement.OnDashStateChange -= HandleDashAnimation;
        _playerMovement.OnSprinteStateChange -= HandleSprinteAnimation;
        _playerMovement.OnLastMoveDirectionChange -= HandleLastMoveDirectionChange;

        _playerCombat.OnAttackForward -= HandleAttackForwardAnimation;
        _playerCombat.OnComboingStateChange -= HandleComboingdAnimation;
    }

    internal void HandleMoveAnimation(Vector3 moveDirection)
    {

        _inputDirection = moveDirection;

        bool isMoving = moveDirection.magnitude > 0.01f;

        foreach (Animator animator in _allAnimator)
        {
            animator.SetBool("isMoving", isMoving);
        }
        //_moveDirection = moveDirection;
        //ถ้ากำลังกลิ้งอยู่ ให้ไม่ต้องทำอะไร
        if (_isDashing || _isCombo || _isSliding) return;

        // ตรวจสอบว่ามีการเคลื่อนที่หรือไม่

        if (!isMoving)
        {
            switch (_settingController.meleeAttackDiraction)
            {
                case SettingPlayerControllerManager.AttackDiractionType.mouse:
                    NewSpritDirection(spriteFDown);
                    break;
                case SettingPlayerControllerManager.AttackDiractionType.movement:
                    SetDirectionMoveAnimationSprite(_playerMovement.lastMoveDirection);
                    //Nope
                    break;
            }
        }

        //if (isMoving)
        //{
        //    SetDirectionMoveAnimationSprite(moveDirection);

        //}
        //else
        //{
        //    switch (_settingController.meleeAttackDiraction)
        //    {
        //        case SettingPlayerControllerManager.AttackDiractionType.mouse:
        //            NewSpritDirection(spriteFDown);
        //            break;
        //        case SettingPlayerControllerManager.AttackDiractionType.movement:
        //            SetDirectionMoveAnimationSprite(_playerMovement.lastMoveDirection);
        //            Nope
        //            break;
        //    }
        //}
    }

    internal void HandleLastMoveDirectionChange(Vector3 lastDirection)
    {

        if (_isDashing || _isCombo || _isSliding) return;

        SetDirectionMoveAnimationSprite(lastDirection);
    }

    internal void HandleDashAnimation(bool isDashingState, Vector3 actionDirection)
    {
        _isDashing = isDashingState; // อัปเดตสถานะ
        if (isDashingState)
        {
            foreach (Animator animator in _allAnimator)
            {
                animator.SetTrigger("atDash");
            }
        }
        SetActionStateDirection(isDashingState, actionDirection.normalized);

        if (!isDashingState) SetDirectionMoveAnimationSprite(_playerMovement.lastMoveDirection);
    }

    internal void HandleComboingdAnimation(bool isCombo)
    {
        if (!_isCombo)
        {
            //_animatorFLeft.SetTrigger("atStartCombo");
            //_animatorBLeft.SetTrigger("atStartCombo");
            //_animatorFDown.SetTrigger("atStartCombo");
            //_animatorBUp.SetTrigger("atStartCombo");

            foreach (Animator animator in _allAnimator)
            {
                animator.SetTrigger("atStartCombo");
            }
        }
        _isCombo = isCombo;


        foreach (Animator animator in _allAnimator)
        {
            animator.SetBool("isCombo", _isCombo);
        }

        if (_isDashing) return;
        if (!isCombo) SetDirectionMoveAnimationSprite(_playerMovement.lastMoveDirection);
    }

    internal void HandleAttackForwardAnimation(Vector3 vector, float arg2, float arg3)
    {
        foreach (Animator animator in _allAnimator)
        {
            animator.SetTrigger("atAttack");
        }

        SetActionStateDirection(true, vector);
    }

    internal void HandleSprinteAnimation(bool isSprinteState)
    {
        _isSprinte = isSprinteState;

        foreach (Animator animator in _allAnimator)
        {
            animator.SetBool("isSprinte", _isSprinte);
        }
        
    }



    private void Start()
    {
        spriteFDown.enabled = false;
        spriteBUp.enabled = false;
        spriteFLeft.enabled = false;
        spriteBLeft.enabled = false;

        NewSpritDirection(spriteFDown);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void SetDirectionMoveAnimationSprite(Vector3 direction)
    {
        if (direction.z <= 0) //Font
        {
            if (direction.x < 0)
            {
                NewSpritDirection(spriteFLeft);
                curreanSprite.flipX = false;

            }
            else if (direction.x > 0)
            {
                NewSpritDirection(spriteFLeft);
                curreanSprite.flipX = true;
            }
            else
            {
                NewSpritDirection(spriteFDown);
            }
        }
        else //Back
        {
            if (direction.x < 0)
            {
                NewSpritDirection(spriteBLeft);
                curreanSprite.flipX = false;

            }
            else if (direction.x > 0)
            {
                NewSpritDirection(spriteBLeft);
                curreanSprite.flipX = true;
            }
            else
            {
                NewSpritDirection(spriteBUp);
            }
        }

    }
    
    private void NewSpritDirection(SpriteRenderer sprite)
    {
        if(curreanSprite != null) curreanSprite.enabled = false;
        curreanSprite = sprite;
        curreanSprite.enabled = true;
    }

    private void MoveFalse()
    {
        foreach (Animator animator in _allAnimator)
        {
            animator.SetBool("isMoving", false);
        }
    }

    //private void FlipSprite(Vector3 Direction)
    //{
    //    //SpriteRenderer spriteRenderer = (Direction.z >= 0) ? playerBLeft : playerFLeft;

    //    // จัดการการพลิก Sprite
    //    if (Direction.x > 0)
    //    {
    //        curreanSprite.flipX = false;
    //    }
    //    else if (Direction.x < 0)
    //    {
    //        curreanSprite.flipX = true;
    //    }
    //}
    private void SetActionStateDirection(bool isState, Vector3 actionDirection)
    {
        if (isState)
        {
            if (actionDirection.z <= 0.2) //Font
            {
                if (actionDirection.x < -0.45)
                {
                    NewSpritDirection(spriteFLeft);
                    curreanSprite.flipX = false;

                }
                else if (actionDirection.x > 0.45)
                {
                    NewSpritDirection(spriteFLeft);
                    curreanSprite.flipX = true;
                }
                else
                {
                    NewSpritDirection(spriteFDown);
                }
            }
            else //Back
            {
                if(actionDirection.x < -0.45)
                {
                    NewSpritDirection(spriteBLeft);
                    curreanSprite.flipX = false;

                }
                else if (actionDirection.x > 0.45)
                {
                    NewSpritDirection(spriteBLeft);
                    curreanSprite.flipX = true;
                }
                else
                {
                    NewSpritDirection(spriteBUp);
                }
                
            }
        }
        else
        {
            HandleMoveAnimation(_inputDirection);
        }
    }
}
