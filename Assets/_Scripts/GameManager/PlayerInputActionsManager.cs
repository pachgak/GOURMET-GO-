using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputActionsManager : MonoBehaviour
{
    public static PlayerInputActionsManager instance;

    //private PlayerInput _playerInput;
    //private InputAction _inventoryOpenAction;
    //public bool IsInventoryOpen;

    private PlayerControls _playerControls; // อ้างอิงถึงคลาสที่ถูกสร้างขึ้น

    [SerializeField] private LayerMask _groundLayerMask; // เพิ่ม LayerMask สำหรับพื้น

    // Action สำหรับส่งทิศทางการเคลื่อนที่
    public Action<Vector3> OnMoveInput;
    private Vector3 _lastMovement;

    // Action สำหรับบอกสถานะการวิ่ง
    public Action<bool> OnSprintInput;

    // Action สำหรับการพุ่ง
    public Action OnDashInput;

    // Action สำหรับการโจมตีระยะประชิด (ส่งตำแหน่งเมาส์ไปด้วย)
    public Action<Vector3> OnMountPosition;
    public Action OnMeleeAttack;

    // เพิ่ม Action สำหรับส่งสถานะการโจมตี
    public Action<bool> OnAttackStateChange;

    public Action OnOpenInventoryInput;

    public Action OnOpenLoadoutSkillInput;

    public Action OnOpenMenuInput;

    public Action OnEscInput;

    public Action OnInteractInputDown;
    public Action OnInteractInputUp;

    public Action<int> OnSkillInput;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject); // (แนะนำ)

        //_playerInput = GetComponent<PlayerInput>();

        _playerControls = new PlayerControls();

        LoadBindingToPlayerContrlorsCS();

        AddActionControl();

        //_inventoryOpenAction = _playerInput.actions["OpenInventory"];
        //if(_inventoryOpenAction != null) Debug.Log($"Set _inventoryOpenAction : {_inventoryOpenAction.name}");
    }

    private void OnEnable()
    {
        _playerControls.Enable(); // เปิดใช้งาน Action Map หลัก (Player)
    }

    private void OnDisable()
    {
        if (_playerControls != null) _playerControls.Disable(); // ปิดใช้งานเมื่อ GameObject ถูกปิด
    }


    private void Update()
    {
        //if (_inventoryOpenAction.WasPressedThisFrame())
        //{
        //    //OnOpenInventoryInput?.Invoke();
        //    Debug.Log($"IsInventoryOpen : {IsInventoryOpen}");
        //}

        // สร้าง Ray จากกล้องไปยังตำแหน่งเมาส์
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // ยิง Raycast เพื่อหาตำแหน่งที่ชนกับพื้นผิว
        if (Physics.Raycast(ray, out hit, 100f, _groundLayerMask))
        {
            // ได้ตำแหน่งที่แม่นยำแล้ว
            Vector3 mouseWorldPosition = hit.point;

            // ส่งสัญญาณการโจมตีพร้อมกับตำแหน่งที่ชน
            OnMountPosition?.Invoke(mouseWorldPosition);
        }

        /*
        //// ส่งทิศทางการเคลื่อนที่
        //Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        //if (movement.magnitude > 1)
        //{
        //    movement.Normalize();
        //}

        //// ตรวจสอบว่าค่าการเคลื่อนที่เปลี่ยนไปหรือไม่
        //if (movement != _lastMovement)
        //{
        //    OnMoveInput?.Invoke(movement);
        //    _lastMovement = movement;
        //}

        //// ตรวจสอบการกด Shift และส่งสถานะวิ่ง
        //if (Input.GetKeyDown(KeyCode.LeftShift)) OnSprintInput?.Invoke(true);
        //if (Input.GetKeyUp(KeyCode.LeftShift)) OnSprintInput?.Invoke(false);

        //// ตรวจสอบการกด Spacebar
        //if (Input.GetKeyDown(KeyCode.Space) && movement != Vector3.zero)
        //{
        //    OnDashInput?.Invoke();
        //}



        //// ตรวจสอบการกดปุ่มเมาส์ซ้าย (ปุ่ม 0)
        //if (Input.GetMouseButtonDown(0))
        //{
        //    OnMeleeAttack?.Invoke();
        //}

        //if (Input.GetKeyDown(KeyCode.Tab))
        //{
        //    OnOpenInventoryInput?.Invoke();
        //}

        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    OnOpenMenuInput?.Invoke();
        //    OnEscInput?.Invoke();
        //}
        
        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    OnInteractInputDown?.Invoke();
        //}
        //if (Input.GetKeyUp(KeyCode.E))
        //{
        //    OnInteractInputUp?.Invoke();
        //}

        //if (Input.inputString != null)
        //{
        //    bool isNumber = int.TryParse(Input.inputString, out int number);
        //    if (isNumber)
        //    {
        //        OnSkillInput?.Invoke(number);
        //    }
        //}
        */
    }
    [ContextMenu("AddActionControl")]
    public void AddActionControl()
    {
        // ** การเคลื่อนที่ (Value) **
        // ใช้ .performed เพื่อรับค่าทุกเฟรมเมื่อมีการเปลี่ยนทิศทาง
        _playerControls.Player.Move.performed += OnMovePerformed;
        _playerControls.Player.Move.canceled += OnMoveCanceled; // เมื่อปล่อยปุ่ม

        // ** การกด/ปล่อย (Button) **
        // ใช้ .started สำหรับการกดลง (Down), .performed สำหรับการกดค้าง (Hold/Down), .canceled สำหรับการปล่อย (Up)
        _playerControls.Player.Sprint.started += OnSprintStarted;
        _playerControls.Player.Sprint.canceled += OnSprintCanceled;

        _playerControls.Player.Dash.performed += OnDashPerformed;
        _playerControls.Player.MeleeAttack.performed += OnMeleeAttackPerformed;

        // ** แทนที่ Input.GetKeyDown(KeyCode.E) **
        _playerControls.Player.Interact.started += OnInteractStarted; // เหมือน GetKeyDown
        _playerControls.Player.Interact.canceled += OnInteractCanceled; // เหมือน GetKeyUp

        _playerControls.Player.OpenInventory.performed += OnOpenInventoryPerformed;
        _playerControls.Player.OpenLoadoutSkill.performed += OnOpenLoadoutSkillPerformed;

        _playerControls.Player.OpenMenu.performed += OnOpenMenuPerformed;

        _playerControls.Player.Skill1.performed += OnSkill1Performed;
        _playerControls.Player.Skill2.performed += OnSkill2Performed;
        _playerControls.Player.Skill3.performed += OnSkill3Performed;
        _playerControls.Player.Skill4.performed += OnSkill4Performed;
        _playerControls.Player.Skill5.performed += OnSkill5Performed;
    }

    // --- Implement Methods ที่ถูกผูกไว้ ---

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        // อ่านค่า Vector2 จากอินพุต (Horizontal, Vertical)
        Vector2 movement = context.ReadValue<Vector2>();
        // แปลงเป็น Vector3 (ตามสคริปต์เดิมของคุณ) และ Invoke Action
        OnMoveInput?.Invoke(new Vector3(movement.x, 0, movement.y));
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        // เมื่อปล่อยปุ่มให้ส่งค่าเป็น Vector3.zero
        OnMoveInput?.Invoke(Vector3.zero);
    }

    private void OnSprintStarted(InputAction.CallbackContext context)
    {
        OnSprintInput?.Invoke(true); // กด Shift ลง
        Debug.Log("OnSprintStarted : true");
    }

    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        OnSprintInput?.Invoke(false); // ปล่อย Shift ขึ้น
        Debug.Log("OnSprintStarted : false");
    }

    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        // คุณอาจต้องเช็คว่ากำลังเคลื่อนที่อยู่หรือไม่ในส่วนที่ใช้ Action นี้
        OnDashInput?.Invoke();
    }

    private void OnMeleeAttackPerformed(InputAction.CallbackContext context)
    {
        OnMeleeAttack?.Invoke();
    }

    private void OnInteractStarted(InputAction.CallbackContext context)
    {
        // แทนที่ Input.GetKeyDown(KeyCode.E)
        OnInteractInputDown?.Invoke();
    }

    private void OnInteractCanceled(InputAction.CallbackContext context)
    {
        // แทนที่ Input.GetKeyUp(KeyCode.E)
        OnInteractInputUp?.Invoke();
    }
     
    private void OnOpenInventoryPerformed(InputAction.CallbackContext context)
    {
        OnOpenInventoryInput?.Invoke();
    }
    private void OnOpenLoadoutSkillPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("K OnOpenLoadoutSkillPerformed");
        OnOpenLoadoutSkillInput?.Invoke();
    }
    private void OnOpenMenuPerformed(InputAction.CallbackContext context)
    {
        OnOpenMenuInput?.Invoke();
        OnEscInput?.Invoke();
    }

    private void OnSkill1Performed(InputAction.CallbackContext context)
    {
        OnSkillInput?.Invoke(1);
    }
    private void OnSkill2Performed(InputAction.CallbackContext context)
    {
        OnSkillInput?.Invoke(2);
    }
    private void OnSkill3Performed(InputAction.CallbackContext context)
    {
        OnSkillInput?.Invoke(3);
    }
    private void OnSkill4Performed(InputAction.CallbackContext context)
    {
        OnSkillInput?.Invoke(4);
    }
    private void OnSkill5Performed(InputAction.CallbackContext context)
    {
        OnSkillInput?.Invoke(5);
    }

    //public void RefreshActionBindingMap()
    //{
    //    string rebinds = _playerInput.actions.SaveBindingOverridesAsJson();
    //    _playerControls.LoadBindingOverridesFromJson(rebinds);


    //    return;
    //    // ** โหลดค่าใหม่กลับเข้าสู่ Instance ที่ถูก Disable **
    //    //string rebinds = PlayerPrefs.GetString("rebinds");
    //    if (!string.IsNullOrEmpty(rebinds))
    //    {
    //        _playerControls.LoadBindingOverridesFromJson(rebinds);
    //    }

    //    if (_playerControls.Player.enabled)
    //    {
    //        _playerControls.Player.Disable();

    //        _playerControls.Player.Enable();
    //        Debug.Log("Action Map Refreshed with new bindings.");
    //    }
    //}

    [ContextMenu("LoadBindingToPlayerContrlorsCS")]
    public void LoadBindingToPlayerContrlorsCS()
    {
        //โหลด 
        string rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            _playerControls.LoadBindingOverridesFromJson(rebinds);
            //Debug.Log($"LoadBindingToPlayerContrlorsCS");
        }
    }
}