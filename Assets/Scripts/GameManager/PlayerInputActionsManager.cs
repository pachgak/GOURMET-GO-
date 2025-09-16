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

    [SerializeField] private LayerMask _groundLayerMask; // ���� LayerMask ����Ѻ���

    // Action ����Ѻ�觷�ȷҧ�������͹���
    public Action<Vector3> OnMoveInput;
    private Vector3 _lastMovement;

    // Action ����Ѻ�͡ʶҹС�����
    public Action<bool> OnSprintInput;

    // Action ����Ѻ��þ��
    public Action OnDashInput;

    // Action ����Ѻ����������л�ЪԴ (�觵��˹������仴���)
    public Action<Vector3> OnMountPosition;
    public Action OnMeleeAttack;

    // ���� Action ����Ѻ��ʶҹС������
    public Action<bool> OnAttackStateChange;

    public Action OnOpenInventoryInput;

    public Action OnOpenMenuInput;

    public Action OnEscInput;

    public Action OnInteractInputDown;
    public Action OnInteractInputUp;

    public Action<int> OnSkillSlotInput;

    private void Update()
    {
        // �觷�ȷҧ�������͹���
        Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }

        // ��Ǩ�ͺ��Ҥ�ҡ������͹�������¹��������
        if (movement != _lastMovement)
        {
            OnMoveInput?.Invoke(movement);
            _lastMovement = movement;
        }

        // ��Ǩ�ͺ��á� Shift �����ʶҹ����
        if (Input.GetKeyDown(KeyCode.LeftShift)) OnSprintInput?.Invoke(true);
        if (Input.GetKeyUp(KeyCode.LeftShift)) OnSprintInput?.Invoke(false);

        // ��Ǩ�ͺ��á� Spacebar
        if (Input.GetKeyDown(KeyCode.Space) && movement != Vector3.zero)
        {
            OnDashInput?.Invoke();
        }


        // ���ҧ Ray �ҡ���ͧ��ѧ���˹������
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // �ԧ Raycast �����ҵ��˹觷�誹�Ѻ��鹼��
        if (Physics.Raycast(ray, out hit, 100f, _groundLayerMask))
        {
            // ����˹觷�����������
            Vector3 mouseWorldPosition = hit.point;

            // ���ѭ�ҳ������վ�����Ѻ���˹觷�誹
            OnMountPosition?.Invoke(mouseWorldPosition);
        }

        // ��Ǩ�ͺ��á������������� (���� 0)
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