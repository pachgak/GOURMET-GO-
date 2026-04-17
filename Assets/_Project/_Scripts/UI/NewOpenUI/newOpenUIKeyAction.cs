using UnityEngine;

public class newOpenUIKeyAction : MonoBehaviour
{
    [HideInInspector] public newOpenUIManager openUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        openUI = newOpenUIManager.instance;
    }

    private void OnEnable()
    {
        if (PlayerInputActionsManager.instance != null)
        {
            // ให้ปุ่ม Esc ผูกกับฟังก์ชันของ UIManager โดยตรง (ไม่ต้องให้หน้า UI แย่งกันรับค่าแล้ว)
            PlayerInputActionsManager.instance.OnEscInput += HandleEscInput;
            PlayerInputActionsManager.instance.OnOpenInventoryInput += HandleInventoryInput;
            PlayerInputActionsManager.instance.OnCloseInteractUIInput += HandleCloseInteractInput;

            //PlayerInputActionsManager.instance.OnOpenLoadoutSkillInput += () => openUI._TogglePanel("LoadoutSkill");
            PlayerInputActionsManager.instance.OnOpenMapInput += () => openUI._TogglePanel("FullMap");
        }
    }

    private void OnDisable()
    {
        if (PlayerInputActionsManager.instance != null)
        {
            PlayerInputActionsManager.instance.OnEscInput -= HandleEscInput;
            PlayerInputActionsManager.instance.OnOpenInventoryInput -= HandleInventoryInput;
            PlayerInputActionsManager.instance.OnCloseInteractUIInput -= HandleCloseInteractInput;

            //PlayerInputActionsManager.instance.OnOpenLoadoutSkillInput -= () => openUI._TogglePanel("LoadoutSkill");
            PlayerInputActionsManager.instance.OnOpenMapInput -= () => openUI._TogglePanel("FullMap");
        }
    }

    private void HandleEscInput()
    {
        // 1. ถาม Manager ว่ามี UI เปิดอยู่ไหม? (แทนการเช็ค activeUIStack.Count)
        if (openUI.HasActiveUI())
        {
            // 2. ถ้ามี สั่งให้ Manager จัดการปิดตัวบนสุด (ถ้ามันยอมให้ปิด)
            openUI.CloseTopPanelIfAllowed();
        }
        else
        {
            // 3. ถ้าไม่มีอะไรเปิดอยู่เลย สั่งเปิด Pause Menu
            openUI._TogglePanel("PauseMenu");
        }
    }

    private void HandleInventoryInput()
    {
        // 1. ถาม Manager ว่ามี UI เปิดอยู่ไหม? (แทนการเช็ค activeUIStack.Count)
        if (openUI.HasActiveUI())
        {
            // 2. ถ้ามี สั่งให้ Manager จัดการปิดตัวบนสุด (ถ้ามันยอมให้ปิด)
            openUI.CloseTopPanelIfAllowed();
        }
        else
        {
            // 3. ถ้าไม่มีอะไรเปิดอยู่เลย สั่งเปิด Pause Menu
            openUI._TogglePanel("Inventory");
        }
    }

    private void HandleCloseInteractInput()
    {
        // ถ้ามีหน้าต่าง UI เปิดอยู่ ให้ลองสั่งปิดด้วยปุ่ม E ดู
        if (openUI.HasActiveUI())
        {
            openUI.CloseTopPanelByInteractIfAllowed();
        }
    }

}
