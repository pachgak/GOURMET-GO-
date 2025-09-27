using Inventory;
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Components")]
    private PlayerMovement playerMovement;
    private PlayerCombatController playerCombat;
    private PlayerSkillController playerSkill;
    private PlayerAnimatorController playerAnimator;
    private InteractByPoint playerInteract;
    private InventoryController playerInventory;

    [Header("Manager References")]
    private PlayerInputActionsManager inputManager;
    private OpenUiManager uiManager;
    private InventoryManager inventoryManager;

    private void Awake()
    {
        // ใช้วิธี GetComponent/GetComponentInChildren เพื่อให้ได้ Reference
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombatController>();
        playerSkill = GetComponent<PlayerSkillController>();
        playerInteract = GetComponent<InteractByPoint>();
        playerInventory = GetComponent<InventoryController>();
        playerAnimator = GetComponent<PlayerAnimatorController>();

        // หา Manager (สมมติว่าเป็น Singleton ทั่วโลก)
        inputManager = PlayerInputActionsManager.instance;
        uiManager = OpenUiManager.instance;
        inventoryManager = InventoryManager.instance;
    }

    private void OnEnable()
    {
        SetupComponentEvents();
    }

    private void OnDisable()
    {
        ResetComponentEvents();
    }

    private void SetupComponentEvents()
    {
        //PlayerMovement
        inputManager.OnMoveInput += playerMovement.HandleMoveInput;
        inputManager.OnSprintInput += playerMovement.HandleSprintInput;
        inputManager.OnDashInput += playerMovement.HandleDashInput;

        uiManager.OnUiOpeningStateChange += playerMovement.HandleUiOpeningStateChange;

        playerCombat.OnAttackStateChange += playerMovement.HandleAttackStateChange;
        playerCombat.OnAttackForward += playerMovement.HandleAttackForward;

        inventoryManager.OnOpenInventoryStateChange += playerMovement.HandleOpenInventoryStateChange;

        playerSkill.OnSkillingStateChange += playerMovement.HandleSkillingStateChange;

        //PlayerCombatController
        inputManager.OnMeleeAttack += playerCombat.HandleMeleeAttack;
        inputManager.OnMountPosition += playerCombat.HandleGetMountPos;

        uiManager.OnUiOpeningStateChange += playerCombat.HandleUiOpeningStateChange;

        playerMovement.OnDashStateChange += playerCombat.HandleDashStateChange;
        playerMovement.OnSprinteStateChange += playerCombat.HandleSprinteStateChange;
        playerMovement.OnSonicStateChange += playerCombat.HandleSonicStateChange;

        playerSkill.OnSkillingStateChange += playerCombat.HandleSkillingStateChange;

        //InteractByPoint
        inputManager.OnInteractInputDown += playerInteract.HandleInteractInputDown;
        inputManager.OnInteractInputUp += playerInteract.HandleInteractInputUp;
        inputManager.OnMountPosition += playerInteract.HandleGetMountPos;

        uiManager.OnUiOpeningStateChange += playerInteract.HandleUiOpeningStateChange;

        //PlayerSkillController
        inputManager.OnSkillSlotInput += playerSkill.HandleSkillSlotInput;
        inputManager.OnMountPosition += playerSkill.HandleGetMountPos;
        playerMovement.OnDashSkillCancelInput += playerSkill.HandleDashSkillCancelInput;
        playerMovement.OnDashStateChange += playerSkill.HandleDashStateChange;

        //InventoryController
        inventoryManager.OnOpenInventoryStateChange += playerInventory.HandleOpenInventoryStateChange;

        //PlayerAnimatorController
        inputManager.OnMoveInput += playerAnimator.HandleMoveAnimation;

        playerMovement.OnDashStateChange += playerAnimator.HandleDashAnimation;
        playerMovement.OnSprinteStateChange += playerAnimator.HandleSprinteAnimation;
        playerMovement.OnSonicStateChange += playerAnimator.HandleSonicAnimation;
        playerMovement.OnSlideStateChange += playerAnimator.HandleSlideAnimation;

        playerCombat.OnAttackForward += playerAnimator.HandleAttackForwardAnimation;
        playerCombat.OnComboingStateChange += playerAnimator.HandleComboingdAnimation;
    }
    private void ResetComponentEvents()
    {
        throw new NotImplementedException();
    }
}
