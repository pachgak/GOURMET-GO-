using System;
using UnityEngine;

public class PlayerInputActionsManager : MonoBehaviour
{
    public static PlayerInputActionsManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

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

    public Action OnOpenMenuInput;

    public Action OnEscInput;

    public Action OnInteractInputDown;
    public Action OnInteractInputUp;

    public Action<int> OnSkillSlotInput;

    private void Update()
    {
        // ส่งทิศทางการเคลื่อนที่
        Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }

        // ตรวจสอบว่าค่าการเคลื่อนที่เปลี่ยนไปหรือไม่
        if (movement != _lastMovement)
        {
            OnMoveInput?.Invoke(movement);
            _lastMovement = movement;
        }

        // ตรวจสอบการกด Shift และส่งสถานะวิ่ง
        if (Input.GetKeyDown(KeyCode.LeftShift)) OnSprintInput?.Invoke(true);
        if (Input.GetKeyUp(KeyCode.LeftShift)) OnSprintInput?.Invoke(false);

        // ตรวจสอบการกด Spacebar
        if (Input.GetKeyDown(KeyCode.Space) && movement != Vector3.zero)
        {
            OnDashInput?.Invoke();
        }


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

        // ตรวจสอบการกดปุ่มเมาส์ซ้าย (ปุ่ม 0)
        if (Input.GetMouseButtonDown(0))
        {
            OnMeleeAttack?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OnOpenInventoryInput?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnOpenMenuInput?.Invoke();
            OnEscInput?.Invoke();
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            OnInteractInputDown?.Invoke();
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            OnInteractInputUp?.Invoke();
        }

        if (Input.inputString != null)
        {
            bool isNumber = int.TryParse(Input.inputString, out int number);
            if (isNumber)
            {
                OnSkillSlotInput?.Invoke(number);
            }
        }

    }
}