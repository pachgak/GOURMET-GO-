using UnityEngine;
using System;

public class PlayerAimGuide : MonoBehaviour
{
    private Vector3 _mousePosition;

    private void OnEnable()
    {
        // �Ѻ�ѧ Event �ҡ PlayerInputActionsManager �����Ѻ���˹������
        PlayerInputActionsManager.instance.OnMountPosition += HandleMountPosition;
    }

    private void OnDisable()
    {
        PlayerInputActionsManager.instance.OnMountPosition -= HandleMountPosition;
    }

    private void HandleMountPosition(Vector3 position)
    {
        // �纵��˹����������ش
        _mousePosition = position;
    }

    private void Update()
    {
        // �ҡ���˹�������դ�� (�ա�ä�ԡ�Դ���)
        if (_mousePosition != Vector3.zero)
        {
            // �ӹǳ��ȷҧ�ҡ���䡴���ѧ���˹������
            Vector3 direction = _mousePosition - transform.position;

            // ������ȷҧ���躹�йҺ�ǹ͹��ҹ�� (᡹ X, Z)
            direction.y = 0;

            // �ҡ��ȷҧ�բ�Ҵ�ҡ���� 0 ���ӡ����ع
            if (direction != Vector3.zero)
            {
                // ��ع GameObject �ͧ䡴�����ѹ仵����ȷҧ���ӹǳ��
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}