using UnityEngine;
using UnityEngine.Events; // ใช้สำหรับผูก Event ใน Inspector ได้เลย

public class newOpenUIController : MonoBehaviour
{
    // สร้าง Enum ไว้ที่นี่
    public enum PanelType { Main, Sub }

    [Header("Panel Settings")]
    public GameObject uiPanel; //
    public string panelID; // ตั้งชื่อให้มัน เช่น "Inventory", "Map", "Cooking"

    // เพิ่มตัวแปร Enum ให้เลือกใน Inspector (ตั้งค่าเริ่มต้นเป็น Main)
    public PanelType panelType = PanelType.Main;

    public bool closeOnEsc = true; // อนุญาตให้กด Esc ปิดหน้านี้ได้ไหม?
    public bool closeOnInteract = false;

    // 2. ซ่อนไว้ไม่ให้รก Inspector เอาไว้เก็บ "เวลา" ที่หน้าต่างนี้ถูกเปิดขึ้นมา
    [HideInInspector] public float openedTime;

    [Header("Optional Elements to Toggle")]
    public GameObject[] enableOnOpen;
    public GameObject[] disableOnOpen;

    [Header("Events")]
    public UnityEvent OnPanelOpened; // เอาไว้สั่งงานอื่นตอนเปิด (เช่น หยุดเวลา)
    public UnityEvent OnPanelClosed;

    private void Awake()
    {
        // ปิดตัวเองไว้ก่อนตอนเริ่มเกม
        if (uiPanel != null) uiPanel.SetActive(false);
    }

    private void Start()
    {
        // 2. ไปรายงานตัวกับ Manager (ในขั้นตอน Start ซึ่งมั่นใจได้ว่า Manager ทำ Awake เสร็จแล้ว)
        if (newOpenUIManager.instance != null)
        {
            newOpenUIManager.instance.RegisterPanel(this);
        }
        else
        {
            Debug.LogError("ไม่พบ newOpenUIManager ในฉาก! ลืมวางหรือเปล่า?");
        }
    }

    public virtual void OpenPanel()
    {
        openedTime = Time.unscaledTime;

        uiPanel.SetActive(true);

        foreach (var obj in enableOnOpen) if (obj != null) obj.SetActive(true);
        foreach (var obj in disableOnOpen) if (obj != null) obj.SetActive(false);

        OnPanelOpened?.Invoke();
    }

    public virtual void ClosePanel()
    {
        uiPanel.SetActive(false);

        foreach (var obj in enableOnOpen) if (obj != null) obj.SetActive(false);
        foreach (var obj in disableOnOpen) if (obj != null) obj.SetActive(true);

        OnPanelClosed?.Invoke();
    }
}