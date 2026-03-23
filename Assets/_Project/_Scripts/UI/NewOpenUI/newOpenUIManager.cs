using System;
using System.Collections.Generic;
using UnityEngine;

public class newOpenUIManager : MonoBehaviour
{
    public static newOpenUIManager instance;

    public Action<bool> OnUiOpeningStateChange;
    private bool _isUiCurrentlyOpen = false; // ตัวแปรคอยเช็คว่าเปลี่ยนสถานะหรือยัง

    // ไม่ต้องมี Array registeredPanels ให้รก Inspector แล้ว!

    // Dictionary พร้อมใช้งานตั้งแต่บรรทัดนี้เลย
    private Dictionary<string, newOpenUIController> panelDictionary = new Dictionary<string, newOpenUIController>();

    private Stack<newOpenUIController> activeUIStack = new Stack<newOpenUIController>();

    private void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        // ไม่ต้องวนลูป Add ในนี้แล้ว
    }

    // --- ฟังก์ชันใหม่: ให้ UI มาลงทะเบียนด้วยตัวเอง ---
    public void RegisterPanel(newOpenUIController panel)
    {
        if (!panelDictionary.ContainsKey(panel.panelID))
        {
            panelDictionary.Add(panel.panelID, panel);
            // Debug.Log($"[UIManager] ลงทะเบียนหน้าต่าง: {panel.panelID} สำเร็จ"); // เปิดคอมเมนต์ไว้ดูตอนเทสได้ครับ
        }
        else
        {
            Debug.LogWarning($"[UIManager] มีชื่อ PanelID '{panel.panelID}' ซ้ำกันในระบบ! กรุณาเช็คชื่อ");
        }
    }

    // ระบบ ควบคุม UI =================================================================
    // --- ฟังก์ชันหลักสำหรับเรียกเปิด/ปิด UI (ยุบรวมแล้ว) ---
    public void _TogglePanel(string id)
    {
        if (panelDictionary.TryGetValue(id, out newOpenUIController openUI))
        {
            // 1. เช็คก่อนว่าหน้าต่างนี้ "อยู่บนสุด" และกำลังเปิดอยู่ไหม?
            // ถ้าใช่ แปลว่าผู้เล่นต้องการ "ปิด" (Toggle Off)
            if (activeUIStack.Count > 0 && activeUIStack.Peek() == openUI)
            {
                _CloseTopPanel();
                return; // จบการทำงาน
            }

            // 2. ถ้าไม่ได้อยู่บนสุด แปลว่าผู้เล่นต้องการ "เปิด" (Toggle On)
            // เช็คว่าเป็น Main หรือ Sub
            if (openUI.panelType == newOpenUIController.PanelType.Main)
            {
                // เงื่อนไขของ Main: ต้องไม่มีอะไรเปิดอยู่เลยถึงจะเปิดได้
                if (activeUIStack.Count > 0)
                {
                    Debug.Log($"บล็อก! ไม่สามารถเปิด {id} เพราะ {activeUIStack.Peek().panelID} เปิดทับอยู่");
                    return; // ยกเลิกการเปิด
                }

                PushPanelToStack(openUI); // เปิดได้เลย
            }
            else if (openUI.panelType == newOpenUIController.PanelType.Sub)
            {
                // เงื่อนไขของ Sub: เปิดซ้อนทับขึ้นไปบน Stack ได้เลย ไม่ต้องสนว่าใครเปิดอยู่
                PushPanelToStack(openUI);
            }
        }
        else
        {
            Debug.LogWarning($"ไม่พบ UI Panel ชื่อ: {id}");
        }
    }

    public void _TogglePanel(newOpenUIController ui)
    {
        _TogglePanel(ui.panelID);
    }

    // ฟังก์ชันย่อยสำหรับจัดการ Stack (เพื่อลดการเขียนโค้ดซ้ำซ้อน)
    private void PushPanelToStack(newOpenUIController panel)
    {
        panel.OpenPanel();
        activeUIStack.Push(panel);
        UpdatePlayerInputState();
    }

    public void _CloseTopPanel()
    {
        if (activeUIStack.Count > 0)
        {
            newOpenUIController topPanel = activeUIStack.Pop();
            topPanel.ClosePanel();
            UpdatePlayerInputState();
        }
    }

    // --- พระเอกของเรา: ควบคุมการเดิน/ตี อัตโนมัติ ---
    private void UpdatePlayerInputState()
    {
        bool hasUI = activeUIStack.Count > 0;

        // ถ้ามีหน้าต่างเปิดอยู่ และก่อนหน้านี้มันเคย "ปิด" อยู่ (สถานะเปลี่ยนจาก 0 -> 1)
        if (hasUI && !_isUiCurrentlyOpen)
        {
            _isUiCurrentlyOpen = true;

            // ตะโกนบอกทุกคนในเกมว่า "ตอนนี้มี UI เปิดอยู่นะ!"
            OnUiOpeningStateChange?.Invoke(true);

            PlayerInputActionsManager.instance.playerControls.Player.Disable();
            //PlayerInputActionsManager.instance.playerControls.UI.Enable();
            Debug.Log("UI เปิดอยู่ -> สลับเข้าโหมด UI");
        }
        // ถ้าไม่มีหน้าต่างเปิดเลย และก่อนหน้านี้มันเคย "เปิด" อยู่ (สถานะเปลี่ยนจาก 1 -> 0)
        else if (!hasUI && _isUiCurrentlyOpen)
        {
            _isUiCurrentlyOpen = false;

            // ตะโกนบอกทุกคนในเกมว่า "ตอนนี้หน้าจอเคลียร์แล้วนะ!"
            OnUiOpeningStateChange?.Invoke(false);

            PlayerInputActionsManager.instance.playerControls.Player.Enable();
            //PlayerInputActionsManager.instance.playerControls.UI.Disable();
            Debug.Log("หน้าจอเคลียร์ -> สลับเข้าโหมด Player");
        }
    }

    // -----------------------------------------------------------
    // Helper Methods สำหรับให้ KeyAction (หรือสคริปต์อื่น) เรียกใช้งาน
    // -----------------------------------------------------------

    // เช็คว่าตอนนี้มีหน้าต่าง UI อะไรเปิดอยู่บ้างไหม?
    public bool HasActiveUI()
    {
        return activeUIStack.Count > 0;
    }

    // สั่งปิดหน้าต่างบนสุด (โดยเช็คเงื่อนไข closeOnEsc ให้เรียบร้อย)
    public void CloseTopPanelIfAllowed()
    {
        if (activeUIStack.Count > 0)
        {
            if (activeUIStack.Peek().closeOnEsc)
            {
                _CloseTopPanel();
            }
            else
            {
                Debug.Log($"บล็อก! หน้าต่าง {activeUIStack.Peek().panelID} ไม่อนุญาตให้ปิดด้วย Esc");
            }
        }
    }

    // ฟังก์ชันใหม่! สำหรับปุ่ม Interact (E)
    public void CloseTopPanelByInteractIfAllowed()
    {
        if (activeUIStack.Count > 0)
        {
            newOpenUIController topPanel = activeUIStack.Peek();

            // เช็ค 2 อย่าง: 
            // 1. อนุญาตให้ปิดด้วย E ไหม?
            // 2. หน้าต่างนี้เปิดมา "เกิน 0.1 วินาที" หรือยัง? (ป้องกันบั๊กกด E เปิดแล้วมันเบิ้ลปิดในเฟรมเดียวกัน)
            if (topPanel.closeOnInteract && (Time.unscaledTime - topPanel.openedTime > 0.1f))
            {
                _CloseTopPanel();
            }
        }
    }

}