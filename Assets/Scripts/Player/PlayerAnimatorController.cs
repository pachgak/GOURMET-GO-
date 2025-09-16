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


    private void OnEnable()
    {
        PlayerInputActionsManager.instance.OnMoveInput += HandleMoveAnimation;

        // �� Coroutine ������ PlayerMovement
        StartCoroutine(WaitForMovementInstance());

        // �� Coroutine ������ PlayerCombatController
        StartCoroutine(WaitForCombatControllerInstance());
    }

    private void OnDisable()
    {
        PlayerInputActionsManager.instance.OnMoveInput -= HandleMoveAnimation;

        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.OnDashStateChange -= HandleDashAnimation;
            PlayerMovement.instance.OnSprinteStateChange -= HandleSprinteAnimation;
            PlayerMovement.instance.OnSonicStateChange -= HandleSonicAnimation;
            PlayerMovement.instance.OnSlideStateChange -= HandleSlideAnimation;
        }
        if (PlayerCombatController.instance != null)
        {
            PlayerCombatController.instance.OnAttackForward -= HandleAttackForwardAnimation;
            PlayerCombatController.instance.OnComboingStateChange -= HandleComboingdAnimation;
        }
    }

    private System.Collections.IEnumerator WaitForMovementInstance()
    {
        // �ͨ����� PlayerMovement.instance ������� null
        while (PlayerMovement.instance == null)
        {
            yield return null;
        }
        // ����� instance ��������� �֧�ӡ����Ѥ��Ѻ�ѧ
        PlayerMovement.instance.OnDashStateChange += HandleDashAnimation;
        PlayerMovement.instance.OnSprinteStateChange += HandleSprinteAnimation;
        PlayerMovement.instance.OnSonicStateChange += HandleSonicAnimation;
        PlayerMovement.instance.OnSlideStateChange += HandleSlideAnimation;
    }

    private System.Collections.IEnumerator WaitForCombatControllerInstance()
    {
        // �ͨ����� PlayerCombatController.instance ������� null
        while (PlayerCombatController.instance == null)
        {
            yield return null;
        }
        // ����� instance ��������� �֧�ӡ����Ѥ��Ѻ�ѧ
        PlayerCombatController.instance.OnAttackForward += HandleAttackForwardAnimation;
        PlayerCombatController.instance.OnComboingStateChange += HandleComboingdAnimation;
    }

    private void HandleComboingdAnimation(bool isCombo)
    {
        if(!_isCombo && !_isDashing) animator.SetTrigger("atStartCombo");

        _isCombo = isCombo;
        
        animator.SetBool("isCombo", _isCombo);

        if(!isCombo) HandleMoveAnimation(_moveDirection);
    }

    private void HandleAttackForwardAnimation(Vector3 vector, float arg2, float arg3)
    {
        animator.SetTrigger("atAttack");

        SetActionStateDirection(true, vector);
    }

    private void HandleSprinteAnimation(bool isSprinteState)
    {
        _isSprinte = isSprinteState;
        animator.SetBool("isSprinte", _isSprinte);
    }

    private void HandleSonicAnimation(bool isSonicState)
    {
        _isSonic = isSonicState;
        animator.SetBool("isSonic", _isSonic);
    }

    private void HandleSlideAnimation(bool isState, Vector3 actionDirection)
    {
        _isSliding = isState;
        animator.SetBool("isSlide", isState);

        SetActionStateDirection(isState, actionDirection);
    }

    private void HandleMoveAnimation(Vector3 moveDirection)
    {
        _moveDirection = moveDirection;

        // ��ҡ��ѧ�������� �������ͧ������
        if (_isDashing || _isCombo || _isSliding) return;

        // ��Ǩ�ͺ����ա������͹����������
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

        // Flip Sprite �����ȷҧ����Թ ������������ѧ����
        FlipSprite(moveDirection);
    }

    // �ѧ��ѹ����Ѻ��� Animation ����
    private void HandleDashAnimation(bool isDashingState, Vector3 actionDirection)
    {
        _isDashing = isDashingState; // �ѻവʶҹ�
        if(isDashingState) animator.SetTrigger("atDash");

        SetActionStateDirection(isDashingState, actionDirection);

    }

    private void FlipSprite(Vector3 Direction)
    {
        // �Ѵ��á�þ�ԡ Sprite
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