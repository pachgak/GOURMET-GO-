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

    private Vector3 _moveDirection;

    private Animator _animatorFDown;
    private Animator _animatorBUp;
    private Animator _animatorFLeft;
    private Animator _animatorBLeft;

    private List<Animator> _allAnimator = new List<Animator>();

    [Header("_References")]
    private PlayerInputActionsManager _inputManager;
    private PlayerMovement _playerMovement;
    private PlayerCombatController _playerCombat;

    private void Awake()
    {
        _inputManager = PlayerInputActionsManager.instance;
        _playerMovement = GetComponent<PlayerMovement>();
        _playerCombat = GetComponent<PlayerCombatController>();

        spriteFDown.gameObject.SetActive(true);
        spriteBUp.  gameObject.SetActive(true);
        spriteFLeft.gameObject.SetActive(true);
        spriteBLeft.gameObject.SetActive(true);

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

        _playerCombat.OnAttackForward += HandleAttackForwardAnimation;
        _playerCombat.OnComboingStateChange += HandleComboingdAnimation;
    }

    private void OnDisable()
    {
        _inputManager.OnMoveInput -= HandleMoveAnimation;

        _playerMovement.OnDashStateChange -= HandleDashAnimation;
        _playerMovement.OnSprinteStateChange -= HandleSprinteAnimation;

        _playerCombat.OnAttackForward -= HandleAttackForwardAnimation;
        _playerCombat.OnComboingStateChange -= HandleComboingdAnimation;
    }

    internal void HandleMoveAnimation(Vector3 moveDirection)
    {
        _moveDirection = moveDirection;

        // ถ้ากำลังกลิ้งอยู่ ให้ไม่ต้องทำอะไร
        if (_isDashing || _isCombo || _isSliding) return;

        // ตรวจสอบว่ามีการเคลื่อนที่หรือไม่
        bool isMoving = moveDirection.magnitude > 0.01f;
        foreach (Animator animator in _allAnimator)
        {
            animator.SetBool("isMoving", isMoving);
        }

        if (isMoving)
        {
            if (moveDirection.z <= 0) //Font
            {
                if (moveDirection.x < 0)
                {
                    NewSpritDirection(spriteFLeft);
                    curreanSprite.flipX = false;

                }
                else if (moveDirection.x > 0) 
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
                if (moveDirection.x < 0)
                {
                    NewSpritDirection(spriteBLeft);
                    curreanSprite.flipX = false;

                }
                else if (moveDirection.x > 0)
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
            NewSpritDirection(spriteFDown);
        }
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

        //SetActionStateDirection(isDashingState, actionDirection);
    }

    internal void HandleComboingdAnimation(bool isCombo)
    {
        if (!_isCombo && !_isDashing)
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

        if (!isCombo) HandleMoveAnimation(_moveDirection);
    }

    internal void HandleAttackForwardAnimation(Vector3 vector, float arg2, float arg3)
    {
        foreach (Animator animator in _allAnimator)
        {
            animator.SetTrigger("atAttack");
        }

        Debug.Log($"{vector}");

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
            HandleMoveAnimation(_moveDirection);
        }
    }
}
